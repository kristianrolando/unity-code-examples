using System;
using UnityEngine;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// A generic base class for saveable MonoBehaviours.
    /// 
    /// <para>
    /// This class defines the contract for capturing and restoring an object's state.
    /// The <typeparamref name="TState"/> type represents the serializable data model (DTO)
    /// used for saving and loading the state.
    /// </para>
    /// 
    /// <para>
    /// Note: <typeparamref name="TState"/> must be a plain data class (POCO) � 
    /// not a <see cref="MonoBehaviour"/> or <see cref="ScriptableObject"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TState">The serializable state data type for this handler.</typeparam>
    public abstract class BaseSaveHandler<TState> : MonoBehaviour, ISaveable
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            var ts = typeof(TState);
            if (typeof(MonoBehaviour).IsAssignableFrom(ts) || typeof(ScriptableObject).IsAssignableFrom(ts))
            {
                Debug.LogError($"{GetType().Name}: TState ({ts.Name}) must not derive from MonoBehaviour or ScriptableObject. Use a plain DTO instead.");
            }
        }
#endif

        /// <summary>
        /// Returns the unique key used to identify this save handler's data.
        /// </summary>
        public virtual string GetSaveKey() => GetType().Name + "_file";

        /// <summary>
        /// Returns the data type of the state object associated with this handler.
        /// </summary>
        public Type GetStateType() => typeof(TState);

        /// <summary>
        /// Captures the current state of this object into a serializable DTO.
        /// </summary>
        /// <returns>A serializable object representing the current state.</returns>
        public abstract object CaptureState();

        /// <summary>
        /// Restores the object's state from a previously saved DTO.
        /// </summary>
        /// <param name="state">The deserialized state data to restore.</param>
        public abstract void RestoreState(object state);
    }
}
