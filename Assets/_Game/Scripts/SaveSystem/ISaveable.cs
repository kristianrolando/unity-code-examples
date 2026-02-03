//
// ISaveable.cs
// Summary:
//   Core contract for anything in the game that wants to be saved/loaded.
//   SaveManager will scan the scene, find all ISaveable, and ask them for data.
//   This lets every system define its own DTO (data transfer object) and keep
//   the save/load logic close to the component.
//
// Notes:
//   - Keep the DTO you return from CaptureState() plain (no MonoBehaviour / no UnityEngine.Object)
//   - GetSaveKey() must be unique per save file, otherwise systems will overwrite each other
//   - GetStateType() is used so the manager can deserialize to the correct type
//   - This interface is intentionally small so it is easy to implement across many systems
//
// Typical flow:
//   1) SaveSystem calls SaveManager.SaveAll(...)
//   2) SaveManager loops ISaveable → GetSaveKey() + CaptureState()
//   3) Save file stores a dictionary<string, string-json>
//   4) On load, SaveManager looks up the json by key, deserializes using GetStateType(), then calls RestoreState(...)
//
using System;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// Implement this on any runtime object / gameplay system that should be persisted.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Must return a unique key for this chunk of data inside the save file.
        /// Example: "player", "player.inventory", "level.progress".
        /// </summary>
        string GetSaveKey();

        /// <summary>
        /// Capture the current state of this object as a plain serializable DTO.
        /// Must be JSON-serializable by the SaveManager's serializer.
        /// </summary>
        object CaptureState();

        /// <summary>
        /// Restore the object from a previously captured state.
        /// The 'state' will be an instance of the type returned by GetStateType().
        /// </summary>
        void RestoreState(object state);

        /// <summary>
        /// Tell the manager what concrete type to deserialize that JSON into.
        /// This allows each ISaveable to define its own DTO shape.
        /// </summary>
        Type GetStateType();
    }
}

/*
--------------------------------------------------------------
Fast example usage
--------------------------------------------------------------

// 1) Define a DTO:
[System.Serializable]
public class PlayerStateDTO
{
    public float health;
    public float mana;
    public UnityEngine.Vector3 position;
}

// 2) Implement ISaveable on your Player component:
public class PlayerSave : UnityEngine.MonoBehaviour, Aldo.Runtime.Systems.SaveSystem.ISaveable
{
    public PlayerController controller;

    public string GetSaveKey() => "player";

    public object CaptureState()
    {
        return new PlayerStateDTO
        {
            health = controller.Health,
            mana   = controller.Mana,
            position = controller.transform.position
        };
    }

    public void RestoreState(object state)
    {
        var dto = (PlayerStateDTO)state;
        controller.Health = dto.health;
        controller.Mana   = dto.mana;
        controller.transform.position = dto.position;
    }

    public System.Type GetStateType() => typeof(PlayerStateDTO);
}

// 3) Now calling SaveSystem.SaveScene() will pick this up automatically
//    (as long as the component is in the scene).
*/
