//
// SaveSystem.cs
// Summary:
//   Static façade over the save/load subsystem so game code can stay very simple.
//   Handles: slot selection, storage creation (plain / encrypted / hybrid), scene-wide
//   discovery of ISaveable, and lightweight per-slot metadata caching.
//
// Notes:
//   - Call Initialize(...) once at startup, or use SaveToSlot/LoadFromSlot which will
//     auto-initialize the requested slot.
//   - Uses a HybridStorage so you can switch between encrypted/plain without breaking
//     old files (it will try encrypted first, then plain).
//   - Keeps an in-memory cache of slot metadata (createdUtc, lastModifiedUtc, version);
//     after SaveScene it refreshes the cache so UI can display updated info.
//   - This class is meant to be the entry point for gameplay code; lower-level details
//     (ISaveStorage, SaveManager, converters) stay hidden.
//


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// Static facade so existing code can stay simple:
    /// SaveSystem.Initialize();
    /// SaveSystem.SaveScene();
    /// SaveSystem.LoadScene();
    /// </summary>
    public static class SaveSystem
    {
        private static SaveManager _manager;
        private static ISaveStorage _storage;
        private static string _currentSlotName;
        private static bool _preferEncrypted = true;

        // In-memory metadata cache: slotName -> SaveContainer
        // Used to show slot list quickly without re-reading all files every time.
        private static readonly Dictionary<string, SaveContainer> _slotMetaCache =
            new Dictionary<string, SaveContainer>(StringComparer.Ordinal);

        private const string kFolderName = "Saves";
        private const string kExtension = ".json";

        /// <summary>
        /// Initialize the save system for a given slot and storage mode.
        /// Call this once, or let SaveToSlot/LoadFromSlot call it for you.
        /// </summary>
        public static void Initialize(string slotName = "save_01", bool useEncryption = true)
        {
            // detect change in preference → rebuild storage
            var previousPreferEncrypted = _preferEncrypted;
            _preferEncrypted = useEncryption;
            if (previousPreferEncrypted != useEncryption || _storage == null)
            {
                _storage = CreateStorage(_preferEncrypted);
            }

            _manager = new SaveManager(_storage, slotName);
            _currentSlotName = slotName;
            RefreshSlotMeta(slotName);
        }

        /// <summary>
        /// Find all ISaveable in the active scene (enabled or disabled) and save them.
        /// Also refreshes slot metadata cache.
        /// </summary>
        public static void SaveScene()
        {
            EnsureInitialized();
            var saveables = FindAllSaveablesInScene();
            _manager.SaveAll(saveables);

            // after saving, update cached metadata for this slot
            RefreshSlotMeta(_currentSlotName);
        }

        /// <summary>
        /// Find all ISaveable in the active scene and load data into them.
        /// </summary>
        public static void LoadScene()
        {
            EnsureInitialized();
            var saveables = FindAllSaveablesInScene();
            _manager.LoadAll(saveables);
            // optionally RefreshSlotMeta(_currentSlotName);
        }

        /// <summary>Delete the currently active slot file and remove from cache.</summary>
        public static void DeleteCurrent()
        {
            EnsureInitialized();
            _manager.DeleteSave();
            _slotMetaCache.Remove(_currentSlotName);
        }

        /// <summary>Check if the currently active slot has a save file.</summary>
        public static bool HasCurrentSave()
        {
            EnsureInitialized();
            return _manager.HasSave();
        }

        /// <summary>Return the name of the currently selected slot.</summary>
        public static string GetCurrentSlotName()
        {
            EnsureInitialized();
            return _currentSlotName;
        }

        // --------------------------------------------------------------------
        // Slot helpers
        // --------------------------------------------------------------------

        /// <summary>
        /// Save the active scene to a specific slot (auto-inits that slot).
        /// </summary>
        public static void SaveToSlot(string slotName)
        {
            Initialize(slotName, _preferEncrypted);
            SaveScene();
        }

        /// <summary>
        /// Load the active scene from a specific slot (auto-inits that slot).
        /// </summary>
        public static void LoadFromSlot(string slotName)
        {
            Initialize(slotName, _preferEncrypted);
            LoadScene();
        }

        /// <summary>
        /// Delete a specific slot file (if present) and clear its metadata cache.
        /// </summary>
        public static void DeleteSlot(string slotName)
        {
            var path = GetSlotFilePath(slotName);
            if (File.Exists(path))
            {
                File.Delete(path);
                _slotMetaCache.Remove(slotName);
                Debug.Log($"[SaveSystem] Deleted slot '{slotName}' at {path}");
            }
            else
            {
                Debug.LogWarning($"[SaveSystem] DeleteSlot: slot '{slotName}' not found.");
            }
        }

        /// <summary>
        /// Delete all save slot files under the saves directory and clear cache.
        /// </summary>
        public static void DeleteAllSlots()
        {
            var dir = GetSavesDirectory();
            if (!Directory.Exists(dir))
            {
                Debug.Log("[SaveSystem] DeleteAllSlots: no Saves directory.");
                return;
            }

            int count = 0;
            foreach (var file in Directory.GetFiles(dir, $"*{kExtension}"))
            {
                File.Delete(file);
                count++;
            }

            _slotMetaCache.Clear();

            Debug.Log($"[SaveSystem] DeleteAllSlots: deleted {count} file(s).");
        }

        /// <summary>
        /// Check if a slot file with this name exists on disk.
        /// </summary>
        public static bool SlotExists(string slotName)
        {
            var path = GetSlotFilePath(slotName);
            return File.Exists(path);
        }

        /// <summary>
        /// Get all existing slot names (file names without extension) sorted alphabetically.
        /// </summary>
        public static IReadOnlyList<string> ListAllSlots()
        {
            var dir = GetSavesDirectory();
            if (!Directory.Exists(dir)) return Array.Empty<string>();

            var list = Directory
                .GetFiles(dir, $"*{kExtension}", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            return list;
        }

        /// <summary>
        /// Count how many slot files are present.
        /// </summary>
        public static int GetSlotCount() => ListAllSlots().Count;

        /// <summary>
        /// Read info for all slot files on disk.
        /// Uses cache when possible; falls back to storage if not cached.
        /// Useful for save/load UI.
        /// </summary>
        public static IReadOnlyList<SlotInfo> GetAllSlotInfos(bool tryDecrypt = true)
        {
            var dir = GetSavesDirectory();
            if (!Directory.Exists(dir))
                return Array.Empty<SlotInfo>();

            var files = Directory.GetFiles(dir, $"*{kExtension}", SearchOption.TopDirectoryOnly);
            var result = new List<SlotInfo>(files.Length);

            // helper storages
            var plainStorage = new JsonFileStorage(kFolderName);
            var encryptedStorage = CreateStorage(); // prefer encrypted

            foreach (var file in files)
            {
                var slotName = Path.GetFileNameWithoutExtension(file);
                SaveContainer container = null;

                // 1) try cache
                if (_slotMetaCache.TryGetValue(slotName, out var cached))
                {
                    container = cached;
                }
                else
                {
                    // 2) load from storage (encrypted first, then plain)
                    string raw = null;
                    bool loaded = false;

                    if (tryDecrypt && encryptedStorage.TryLoadRaw(slotName, out var decrypted))
                    {
                        raw = decrypted;
                        loaded = true;
                    }
                    else if (plainStorage.TryLoadRaw(slotName, out var plain))
                    {
                        raw = plain;
                        loaded = true;
                    }

                    if (loaded && !string.IsNullOrWhiteSpace(raw))
                    {
                        try
                        {
                            container = JsonConvert.DeserializeObject<SaveContainer>(raw);
                            if (container != null)
                            {
                                _slotMetaCache[slotName] = container;
                            }
                        }
                        catch
                        {
                            // ignore broken files
                        }
                    }
                }

                var fi = new FileInfo(file);
                DateTime fsModified = File.GetLastWriteTimeUtc(file);

                result.Add(new SlotInfo
                {
                    SlotName = slotName,
                    FilePath = file,
                    FileSizeBytes = fi.Exists ? fi.Length : 0,
                    LastModifiedUtc = container?.lastModifiedUtc != default ? container.lastModifiedUtc : fsModified,
                    CreatedTimeUtc = container?.createdUtc != default ? container.createdUtc : fi.CreationTimeUtc,
                    Version = container?.version ?? 0
                });
            }

            return result;
        }

        // --------------------------------------------------------------------
        // Internal helpers
        // --------------------------------------------------------------------
        private static void EnsureInitialized()
        {
            if (_manager == null)
            {
                Initialize();
            }
        }

        private static IEnumerable<ISaveable> FindAllSaveablesInScene()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return behaviours.OfType<ISaveable>();
        }

        private static ISaveStorage CreateStorage(bool preferEncrypted = true)
        {
            var fileStorage = new JsonFileStorage(kFolderName);

            byte[] key = DeriveKey("Save_Key_EternalClover", 16);
            byte[] iv = DeriveKey("Save_IV_EternalClover", 16);
            var encrypted = new EncryptedStorage(fileStorage, key, iv);

            // Hybrid: save using preferred mode, read both
            return new HybridStorage(fileStorage, encrypted, preferEncrypted);
        }

        private static byte[] DeriveKey(string seed, int length = 16)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(seed);
                var hash = sha.ComputeHash(bytes); // 32 bytes
                var key = new byte[length];
                Array.Copy(hash, key, length);
                return key;
            }
        }

        private static string GetSavesDirectory()
        {
            return Path.Combine(Application.persistentDataPath, kFolderName);
        }

        private static string GetSlotFilePath(string slotName)
        {
            var safe = SanitizeFileName(slotName);
            return Path.Combine(GetSavesDirectory(), safe + kExtension);
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? "save_01" : cleaned;
        }

        /// <summary>
        /// Force-refresh metadata for exactly one slot using the active storage.
        /// Called after save and after init.
        /// </summary>
        private static void RefreshSlotMeta(string slotName)
        {
            if (_storage == null) return;
            if (string.IsNullOrWhiteSpace(slotName)) return;

            if (_storage.TryLoadRaw(slotName, out var raw) && !string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    var container = JsonConvert.DeserializeObject<SaveContainer>(raw);
                    if (container != null)
                    {
                        _slotMetaCache[slotName] = container;
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// Lightweight struct used by UI to display save slot list.
        /// </summary>
        public struct SlotInfo
        {
            public string SlotName;
            public string FilePath;
            public long FileSizeBytes;
            public DateTime LastModifiedUtc;
            public DateTime CreatedTimeUtc;
            public int Version;
        }
    }
}

/*
--------------------------------------------------------------
Fast example usage
--------------------------------------------------------------

// 1) Basic startup
void Awake()
{
    // create or use "save_01", encrypted
    SaveSystem.Initialize("save_01", useEncryption: true);
}

// 2) Save whole scene
void OnSaveButton()
{
    SaveSystem.SaveScene();
}

// 3) Load whole scene
void OnLoadButton()
{
    SaveSystem.LoadScene();
}

// 4) Work with slots directly
void SaveToSlot2()
{
    SaveSystem.SaveToSlot("slot_02");
}

void LoadSlot2()
{
    if (SaveSystem.SlotExists("slot_02"))
        SaveSystem.LoadFromSlot("slot_02");
}

// 5) Populate a UI list
void BuildSaveListUI()
{
    var infos = SaveSystem.GetAllSlotInfos();
    foreach (var info in infos)
    {
        Debug.Log($"Slot: {info.SlotName}, Size: {info.FileSizeBytes} bytes, Created: {info.CreatedTimeUtc}, Modified: {info.LastModifiedUtc}, Ver: {info.Version}");
    }
}

// 6) Delete everything
void ResetAllSaves()
{
    SaveSystem.DeleteAllSlots();
}

*/