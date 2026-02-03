//
// SaveManager.cs
// Summary:
//   Orchestrates saving and loading for all runtime objects that implement ISaveable.
//   It does NOT know about scenes or slot discovery � it only handles "this storage + this slot".
//   Actual file I/O is delegated to an ISaveStorage (plain JSON, encrypted, hybrid, cloud, etc.).
//   Each ISaveable provides a unique key and a serializable DTO; SaveManager packs them into
//   a single SaveContainer and writes it out.
//
// Notes:
//   - Designed to be production-friendly: has versioning, metadata timestamps, and Unity-type converters.
//   - Load flow is tolerant: if one ISaveable fails to restore, the others still restore.
//   - Save flow preserves createdUtc by reading the old container first, then updating lastModifiedUtc.
//   - You typically don�t create this directly in game code � SaveSystem (the facade) does.
//   - Add more JsonConverters here if your project uses more Unity-specific types.
//

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// Coordinates saving and loading of multiple ISaveable objects.
    /// Uses an ISaveStorage for the actual I/O.
    /// </summary>
    public class SaveManager
    {
        /// <summary>
        /// Current logical format version of the save. Bump when you change container schema.
        /// </summary>
        public const int CurrentVersion = 1;

        private readonly ISaveStorage _storage;
        private readonly string _slotName;
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Create a manager bound to one storage backend and one logical slot name.
        /// </summary>
        /// <param name="storage">Where to save/load the data.</param>
        /// <param name="slotName">Logical slot name, e.g. "save_01".</param>
        public SaveManager(ISaveStorage storage, string slotName = "save_01")
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _slotName = string.IsNullOrWhiteSpace(slotName) ? "save_01" : slotName;

            // shared JSON settings used for every state
            _jsonSettings = new JsonSerializerSettings
            {
                // We control the DTO types through ISaveable, so we don't need to embed type info in JSON.
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters =
                {
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new Vector4Converter(),
                    new QuaternionConverter(),
                    new ColorConverter(),
                    new RectConverter(),
                    new BoundsConverter(),
                    new Matrix4x4Converter(),
                    new LayerMaskConverter()
                }
            };
        }

        /// <summary>
        /// Save all provided ISaveable objects into one file (this slot).
        /// </summary>
        public void SaveAll(IEnumerable<ISaveable> saveables)
        {
            if (saveables == null) throw new ArgumentNullException(nameof(saveables));

            SaveContainer container = null;

            // 1) Try to read existing container, so we don't lose createdUtc.
            if (_storage.TryLoadRaw(_slotName, out var existingJson) && !string.IsNullOrWhiteSpace(existingJson))
            {
                try
                {
                    container = JsonConvert.DeserializeObject<SaveContainer>(existingJson, _jsonSettings);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveManager] Failed to read existing container, will create new. {e.Message}");
                }
            }

            // 2) If nothing was there, create a fresh container and stamp creation time.
            if (container == null)
            {
                container = new SaveContainer
                {
                    version = CurrentVersion,
                    createdUtc = DateTime.UtcNow
                };
            }

            // 3) Always update last modified.
            container.lastModifiedUtc = DateTime.UtcNow;

            // 4) Refill the states dictionary with current runtime states.
            foreach (var s in saveables)
            {
                if (s == null) continue;
                var key = s.GetSaveKey();
                if (string.IsNullOrWhiteSpace(key))
                {
                    Debug.LogWarning("[SaveManager] ISaveable returned empty key. Skipping.");
                    continue;
                }

                var stateObj = s.CaptureState();
                var json = JsonConvert.SerializeObject(stateObj, _jsonSettings);
                container.states[key] = json;
            }

            // 5) Write to the storage (this could be encrypted/hybrid/etc.).
            var finalJson = JsonConvert.SerializeObject(container, Formatting.Indented, _jsonSettings);
            _storage.SaveRaw(_slotName, finalJson);
        }

        /// <summary>
        /// Load save file for this slot and apply to all provided ISaveable (matched by key).
        /// </summary>
        public void LoadAll(IEnumerable<ISaveable> saveables)
        {
            if (!_storage.TryLoadRaw(_slotName, out var rawJson))
            {
                Debug.Log($"[SaveManager] No save found for slot '{_slotName}'.");
                return;
            }

            SaveContainer container = null;
            try
            {
                container = JsonConvert.DeserializeObject<SaveContainer>(rawJson, _jsonSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to deserialize save container. Error: {e.Message}");
                return;
            }

            if (container == null)
            {
                Debug.LogError("[SaveManager] Save container is null.");
                return;
            }

            // Versioning hook � add migration logic here later.
            if (container.version != CurrentVersion)
            {
                Debug.LogWarning($"[SaveManager] Save version {container.version} != current {CurrentVersion}. Add migration here.");
            }

            // Apply to every ISaveable that has matching key in the container.
            foreach (var s in saveables)
            {
                if (s == null) continue;
                var key = s.GetSaveKey();
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!container.states.TryGetValue(key, out var json))
                {
                    // No data for this system � that's ok, skip.
                    continue;
                }

                try
                {
                    var targetType = s.GetStateType();
                    var stateObj = JsonConvert.DeserializeObject(json, targetType, _jsonSettings);
                    s.RestoreState(stateObj);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to restore state for key '{key}'. Error: {e.Message}");
                }
            }
        }

        /// <summary>Check if this slot has a save.</summary>
        public bool HasSave() => _storage.Exists(_slotName);

        /// <summary>Delete save for this slot.</summary>
        public void DeleteSave() => _storage.Delete(_slotName);

        /// <summary>
        /// Placeholder if you later want to reuse one SaveManager and just switch the logical slot.
        /// Currently does nothing.
        /// </summary>
        public void SwitchSlot(string newSlot)
        {
            if (string.IsNullOrWhiteSpace(newSlot)) return;
            // Implement if you decide to make SaveManager multi-slot aware.
        }
    }
}

/*
--------------------------------------------------------------
Fast example usage
--------------------------------------------------------------

// 1) Basic usage in code (usually wrapped by SaveSystem):
void SaveGameplay()
{
    // Create a storage that writes JSON to /Saves
    ISaveStorage storage = new JsonFileStorage("Saves");

    // Bind manager to slot "player_slot_01"
    var mgr = new SaveManager(storage, "player_slot_01");

    // Find all saveables in scene
    var saveables = UnityEngine.Object
        .FindObjectsByType<UnityEngine.MonoBehaviour>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)
        .OfType<ISaveable>();

    // Save them
    mgr.SaveAll(saveables);
}

void LoadGameplay()
{
    ISaveStorage storage = new JsonFileStorage("Saves");
    var mgr = new SaveManager(storage, "player_slot_01");

    var saveables = UnityEngine.Object
        .FindObjectsByType<UnityEngine.MonoBehaviour>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)
        .OfType<ISaveable>();

    mgr.LoadAll(saveables);
}

// 2) With encrypted/hybrid storage:
void SaveEncrypted()
{
    var plain = new JsonFileStorage("Saves");
    var key   = System.Text.Encoding.UTF8.GetBytes("My_Save_Key_1234");
    var iv    = System.Text.Encoding.UTF8.GetBytes("My_Save_IV__1234");
    var enc   = new EncryptedStorage(plain, key, iv);
    ISaveStorage storage = new HybridStorage(plain, enc, preferEncrypted: true);

    var mgr = new SaveManager(storage, "secure_slot");
    var saveables = UnityEngine.Object.FindObjectsByType<UnityEngine.MonoBehaviour>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).OfType<ISaveable>();
    mgr.SaveAll(saveables);
}

// 3) Implementing a saveable component:
public class PlayerSave : BaseSaveHandler<PlayerSave.State>
{
    [Serializable]
    public class State
    {
        public float health;
        public Vector3 position;
    }

    public override object CaptureState()
    {
        return new State
        {
            health = 100f,
            position = transform.position
        };
    }

    public override void RestoreState(object state)
    {
        var s = (State)state;
        transform.position = s.position;
        // apply health...
    }

    // optional: public override string GetSaveKey() => "player";
}

*/
