using UnityEngine;

namespace Game.Core.Singleton
{
    /// <summary>
    /// A temporary singleton that is auto-created but does not persist between scenes.
    /// Suitable for short-lived systems like managers specific to a single scene.
    /// </summary>
    /// <typeparam name="T">The type of the singleton component.</typeparam>
    public abstract class SingletonTransient<T> : BaseSingleton<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Unity Awake callback. Does not persist the object between scene loads.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Enables auto-creation of the singleton without applying DontDestroyOnLoad.
        /// </summary>
        protected override bool AllowAutoCreation => true;
    }
}
