//
// ISaveStorage.cs
// Summary:
//   Minimal storage abstraction for the save system.
//   The SaveManager only knows how to produce/consume raw JSON strings.
//   ISaveStorage hides *where* that string goes: disk, encrypted disk,
//   remote server, PlayerPrefs, hybrid, etc.
//
// Why:
//   - Makes the save pipeline testable (can plug in a mock storage).
//   - Lets you swap JsonFileStorage → EncryptedStorage → HybridStorage
//     without changing gameplay code.
//   - Keeps SaveManager focused on orchestrating ISaveable objects, not I/O.
//
// Notes:
//   - fileName is a logical identifier (often "save_01"), not a full path.
//   - Implementations are responsible for mapping that name to real storage.
//   - DeleteAll is intentionally broad; implementors should define the scope
//     (e.g. all .json files under /Saves).
//

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// Abstraction over where and how raw save data is persisted.
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>
        /// Persist raw string content under the given logical name.
        /// Implementations decide the actual path/location.
        /// </summary>
        void SaveRaw(string fileName, string content);

        /// <summary>
        /// Try to retrieve raw string content by name.
        /// Returns true if found and 'content' is populated.
        /// </summary>
        bool TryLoadRaw(string fileName, out string content);

        /// <summary>
        /// Check whether a save entry with the given name currently exists.
        /// </summary>
        bool Exists(string fileName);

        /// <summary>
        /// Remove a specific save entry.
        /// </summary>
        void Delete(string fileName);

        /// <summary>
        /// Remove all saves that this storage is responsible for
        /// (e.g. all .json files in a directory).
        /// </summary>
        void DeleteAll();
    }
}

/*
--------------------------------------------------------------
Fast example usage
--------------------------------------------------------------

// Example 1: Simple file storage
public class MyFileStorage : ISaveStorage
{
    public void SaveRaw(string fileName, string content)
    {
        var path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, fileName + ".json");
        System.IO.File.WriteAllText(path, content);
    }

    public bool TryLoadRaw(string fileName, out string content)
    {
        var path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, fileName + ".json");
        if (!System.IO.File.Exists(path))
        {
            content = null;
            return false;
        }
        content = System.IO.File.ReadAllText(path);
        return true;
    }

    public bool Exists(string fileName)
    {
        var path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, fileName + ".json");
        return System.IO.File.Exists(path);
    }

    public void Delete(string fileName)
    {
        var path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, fileName + ".json");
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    public void DeleteAll()
    {
        var dir = UnityEngine.Application.persistentDataPath;
        foreach (var f in System.IO.Directory.GetFiles(dir, "*.json"))
            System.IO.File.Delete(f);
    }
}
*/

// Example 2: Plug into your SaveManager
// var storage   = new MyFileStorage();
// var saveMgr   = new SaveManager(storage, "save_01");
// saveMgr.SaveAll(foundSaveables);
// saveMgr.LoadAll(foundSaveables);
