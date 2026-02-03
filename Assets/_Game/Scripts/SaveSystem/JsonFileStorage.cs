using System;
using System.IO;
using UnityEngine;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// File-based storage using JSON files in Application.persistentDataPath.
    /// This is the "default" storage.
    /// </summary>
    public sealed class JsonFileStorage : ISaveStorage
    {
        private readonly string _directory;
        private readonly string _extension;

        /// <param name="directory">Optional subfolder inside persistentDataPath, default empty.</param>
        /// <param name="extension">File extension, default ".json".</param>
        public JsonFileStorage(string directory = "", string extension = ".json")
        {
            _directory = directory ?? string.Empty;
            _extension = string.IsNullOrWhiteSpace(extension) ? ".json" : extension;

            // make sure folder exists
            var full = GetDirectoryPath();
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }
        }

        public void SaveRaw(string fileName, string content)
        {
            var path = GetFilePath(fileName);
            try
            {
                File.WriteAllText(path, content);
                Debug.Log($"[JsonFileStorage] Saved: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonFileStorage] Failed to save to {path}. Error: {e.Message}");
            }
        }

        public bool TryLoadRaw(string fileName, out string content)
        {
            var path = GetFilePath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[JsonFileStorage] File not found: {path}");
                content = null;
                return false;
            }

            try
            {
                content = File.ReadAllText(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonFileStorage] Failed to read {path}. Error: {e.Message}");
                content = null;
                return false;
            }
        }

        public bool Exists(string fileName)
        {
            return File.Exists(GetFilePath(fileName));
        }

        public void Delete(string fileName)
        {
            var path = GetFilePath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[JsonFileStorage] Nothing to delete at: {path}");
                return;
            }

            try
            {
                File.Delete(path);
                Debug.Log($"[JsonFileStorage] Deleted: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonFileStorage] Failed to delete {path}. Error: {e.Message}");
            }
        }

        public void DeleteAll()
        {
            var dir = GetDirectoryPath();
            if (!Directory.Exists(dir))
            {
                return;
            }

            try
            {
                var files = Directory.GetFiles(dir, "*" + _extension);
                foreach (var file in files)
                {
                    File.Delete(file);
                    Debug.Log($"[JsonFileStorage] Deleted: {file}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonFileStorage] Failed to delete all in {dir}. Error: {e.Message}");
            }
        }

        private string GetDirectoryPath()
        {
            return string.IsNullOrEmpty(_directory)
                ? Application.persistentDataPath
                : Path.Combine(Application.persistentDataPath, _directory);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(GetDirectoryPath(), fileName + _extension);
        }
    }
}
