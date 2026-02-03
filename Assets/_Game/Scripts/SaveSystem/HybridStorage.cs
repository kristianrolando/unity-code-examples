//
// HybridStorage.cs
// Purpose:
//   An ISaveStorage that can read/write BOTH encrypted and plain JSON save files.
//   - On SAVE  : follows the current preference (encrypted or plain)
//   - On LOAD  : always tries encrypted first, then falls back to plain
//
// Why:
//   - You might ship v1 of the game without encryption, then enable encryption later
//   - You want to migrate players without breaking their old saves
//   - You want to toggle encryption in dev builds without changing all code
//
// How it works:
//   - It wraps TWO storages:
//        1) _plain     : usually JsonFileStorage
//        2) _encrypted : EncryptedStorage(JsonFileStorage, key, iv)
//   - SaveRaw(...) picks one of them
//   - TryLoadRaw(...) tries encrypted first → if decryption fails → tries plain
//
// Notes:
//   - Exists/Delete/DeleteAll operate on BOTH so files don't get orphaned.
//   - This class does NOT generate keys/IVs; you must do that when building the encrypted storage.
//   - Keep this class lightweight; it belongs in the "composition over inheritance" approach.
// -----------------------------------------------------------------------------
// Quick usage:
//
// var plain = new JsonFileStorage("Saves");
// var enc   = new EncryptedStorage(plain, key, iv);
// var storage = new HybridStorage(plain, enc, preferEncrypted: true);
//
// storage.SaveRaw("slot_01", json); // will encrypt
// storage.TryLoadRaw("slot_01", out var json); // will try decrypt, fallback to plain
//
// Pass `storage` into your SaveManager.
//

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// ISaveStorage that can read both encrypted and plain saves.
    /// Saves using the preferred mode, loads by trying encrypted first then plain.
    /// </summary>
    public sealed class HybridStorage : ISaveStorage
    {
        private readonly ISaveStorage _plain;
        private readonly EncryptedStorage _encrypted;

        // preferEncrypted = true → use encrypted storage when saving
        private bool _preferEncrypted;

        /// <param name="plain">The non-encrypted storage (e.g. JsonFileStorage)</param>
        /// <param name="encrypted">Encrypted wrapper built on top of the same underlying storage</param>
        /// <param name="preferEncrypted">If true, SaveRaw will use encrypted by default</param>
        public HybridStorage(ISaveStorage plain, EncryptedStorage encrypted, bool preferEncrypted = true)
        {
            _plain = plain;
            _encrypted = encrypted;
            _preferEncrypted = preferEncrypted;
        }

        /// <summary>
        /// Toggle at runtime whether new saves should be encrypted.
        /// Useful for debug builds or A/B migrations.
        /// </summary>
        public void SetPreferEncrypted(bool enabled) => _preferEncrypted = enabled;

        public void SaveRaw(string fileName, string content)
        {
            if (_preferEncrypted)
                _encrypted.SaveRaw(fileName, content);
            else
                _plain.SaveRaw(fileName, content);
        }

        public bool TryLoadRaw(string fileName, out string content)
        {
            // 1) Always try encrypted first.
            //    If file is plain, EncryptedStorage.TryLoadRaw will return false (or null),
            //    then we can safely fallback to plain.
            if (_encrypted.TryLoadRaw(fileName, out content) && !string.IsNullOrEmpty(content))
                return true;

            // 2) Fallback: plain JSON
            if (_plain.TryLoadRaw(fileName, out content) && !string.IsNullOrEmpty(content))
                return true;

            content = null;
            return false;
        }

        public bool Exists(string fileName)
        {
            // A slot may exist in either plain or encrypted form
            return _plain.Exists(fileName) || _encrypted.Exists(fileName);
        }

        public void Delete(string fileName)
        {
            // Delete both variants so we don't leave stale data behind
            _plain.Delete(fileName);
            _encrypted.Delete(fileName);
        }

        public void DeleteAll()
        {
            _plain.DeleteAll();
            _encrypted.DeleteAll();
        }
    }
}

/*
--------------------------------------------------------------
Fast example usage
--------------------------------------------------------------

// 1) Build the underlying plain storage
var plainStorage = new JsonFileStorage("Saves");

// 2) Build the encrypted storage using the same plain storage
byte[] key = System.Text.Encoding.UTF8.GetBytes("0123456789ABCDEF"); // demo only
byte[] iv  = System.Text.Encoding.UTF8.GetBytes("FEDCBA9876543210"); // demo only
var encryptedStorage = new EncryptedStorage(plainStorage, key, iv);

// 3) Wrap them in HybridStorage
var hybrid = new HybridStorage(plainStorage, encryptedStorage, preferEncrypted: true);

// 4) Save (will go to encrypted file)
hybrid.SaveRaw("player_slot_01", "{ \"gold\": 999 }");

// 5) Load (will try encrypted first, then plain)
if (hybrid.TryLoadRaw("player_slot_01", out var json))
{
    UnityEngine.Debug.Log("Loaded slot: " + json);
}

// 6) Plug into your SaveManager
// var manager = new SaveManager(hybrid, "player_slot_01");
// manager.SaveAll(...);
// manager.LoadAll(...);

// 7) Toggle at runtime (e.g. debug menu)
// hybrid.SetPreferEncrypted(false); // subsequent saves will be plain
*/
