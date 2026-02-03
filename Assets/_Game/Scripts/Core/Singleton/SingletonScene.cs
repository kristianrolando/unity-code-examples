using UnityEngine;

namespace Game.Core.Singleton
{
    /// <summary>
    /// A singleton that must be manually placed in the scene.
    /// Suitable for use cases that require setting references via the inspector.
    /// </summary>
    /// <typeparam name="T">The type of the singleton component.</typeparam>
    public abstract class SingletonScene<T> : BaseSingleton<T> where T : MonoBehaviour
    {
        [Tooltip("If true, this object will persist between scene loads.")]
        [SerializeField] private bool dontDestroyOnLoad = false;

        /// <summary>
        /// Unity Awake callback. Registers the singleton and applies DontDestroyOnLoad if enabled.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Instance == this && dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
