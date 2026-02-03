//
// EncryptedStorage.cs
// Purpose:
//   A drop-in ISaveStorage decorator that adds AES encryption/decryption
//   on top of any underlying storage (e.g. JsonFileStorage).
//
//   - On Save:   plaintext JSON → UTF8 → AES → Base64 → inner.SaveRaw(...)
//   - On Load:   inner.TryLoadRaw(...) → Base64 → AES → UTF8 → plaintext JSON
//
// Why:
//   - Prevent players from casually opening/modifying save files
//   - Keep your existing storage implementation (composition, not inheritance)
//   - Keep the rest of your save pipeline unchanged (SaveManager, ISaveable, etc.)
//
// Notes:
//   - You must provide a stable key + IV (16/24/32 bytes key, 16 bytes IV).
//   - If key/IV changes between runs, old files cannot be decrypted.
//   - This is “good enough” obfuscation for many games, but not anti-cheat grade.
//   - Works best when wrapped in a HybridStorage to auto-fallback between plain/encrypted.
//
// --------------------------------------------------------------
// Quick usage:
//
// var fileStorage = new JsonFileStorage("Saves");
// byte[] key = MyKeyUtil.DeriveKey("my-game-key", 16);
// byte[] iv  = MyKeyUtil.DeriveKey("my-game-iv", 16);
// ISaveStorage enc = new EncryptedStorage(fileStorage, key, iv);
//
// enc.SaveRaw("slot_01", "{ \"hp\": 100 }");
// bool ok = enc.TryLoadRaw("slot_01", out var json);
//
// --------------------------------------------------------------
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// ISaveStorage wrapper that encrypts/decrypts data using AES
    /// before delegating to the inner storage.
    /// 
    /// Flow:
    /// SaveRaw   : string -> UTF8 bytes -> AES encrypt -> Base64 -> inner.SaveRaw
    /// TryLoadRaw: inner.TryLoadRaw -> Base64 -> AES decrypt -> UTF8 string
    /// </summary>
    public sealed class EncryptedStorage : ISaveStorage
    {
        private readonly ISaveStorage _inner;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        /// <param name="inner">Underlying storage (e.g. JsonFileStorage)</param>
        /// <param name="key">AES key (16/24/32 bytes for AES-128/192/256)</param>
        /// <param name="iv">AES IV (16 bytes)</param>
        public EncryptedStorage(ISaveStorage inner, byte[] key, byte[] iv)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _iv = iv ?? throw new ArgumentNullException(nameof(iv));

            if (_key.Length != 16 && _key.Length != 24 && _key.Length != 32)
                throw new ArgumentException("AES key must be 16, 24, or 32 bytes.", nameof(key));

            if (_iv.Length != 16)
                throw new ArgumentException("AES IV must be 16 bytes.", nameof(iv));
        }

        public void SaveRaw(string fileName, string content)
        {
            if (content == null)
            {
                _inner.SaveRaw(fileName, null);
                return;
            }

            // string -> bytes
            byte[] plainBytes = Encoding.UTF8.GetBytes(content);
            // encrypt
            byte[] encrypted = EncryptAes(plainBytes, _key, _iv);
            // store as base64 so inner storage can still save as text
            string base64 = Convert.ToBase64String(encrypted);

            _inner.SaveRaw(fileName, base64);
        }

        public bool TryLoadRaw(string fileName, out string content)
        {
            content = null;

            if (!_inner.TryLoadRaw(fileName, out var base64) || string.IsNullOrEmpty(base64))
                return false;

            try
            {
                byte[] encrypted = Convert.FromBase64String(base64);
                byte[] plain = DecryptAes(encrypted, _key, _iv);
                content = Encoding.UTF8.GetString(plain);
                return true;
            }
            catch
            {
                // Most common causes:
                // - file is not actually encrypted (plain json)
                // - key/iv changed
                // - file corrupted
                content = null;
                return false;
            }
        }

        public bool Exists(string fileName) => _inner.Exists(fileName);

        public void Delete(string fileName) => _inner.Delete(fileName);

        public void DeleteAll() => _inner.DeleteAll();

        // -------------------------------------------------------------
        // AES helpers
        // -------------------------------------------------------------
        private static byte[] EncryptAes(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        private static byte[] DecryptAes(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }
    }
}

/*
--------------------------------------------------------------
Fast example usage
--------------------------------------------------------------

// 1) Wrap your existing storage
var plain = new JsonFileStorage("Saves");

// simple key/iv derivation (DON'T hardcode like this in production)
byte[] key = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // 16 bytes
byte[] iv  = Encoding.UTF8.GetBytes("FEDCBA9876543210"); // 16 bytes

ISaveStorage secure = new EncryptedStorage(plain, key, iv);

// 2) Save
secure.SaveRaw("player_slot_01", "{ \"level\": 10, \"xp\": 550 }");

// 3) Load
if (secure.TryLoadRaw("player_slot_01", out var json))
{
    UnityEngine.Debug.Log("Decrypted: " + json);
}

// 4) Plug into SaveManager
// var mgr = new SaveManager(secure, "player_slot_01");
// mgr.SaveAll(...);
// mgr.LoadAll(...);

// 5) Combine with HybridStorage if you want to auto-fallback between encrypted/plain.
*/
