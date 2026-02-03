using UnityEngine;

namespace Game.Core.Singleton
{
    /// <summary>
    /// A global singleton that is automatically created if not found in the scene.
    /// This object persists between scene loads.
    /// </summary>
    /// <typeparam name="T">The type of the singleton component.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class SingletonGlobal<T> : BaseSingleton<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Unity Awake callback. Applies DontDestroyOnLoad to persist across scenes.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Allows this singleton type to auto-create itself at runtime if no instance is found.
        /// </summary>
        protected override bool AllowAutoCreation => true;
    }
}
