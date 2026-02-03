using UnityEngine;


namespace Game.Core.Singleton
{
    /// <summary>
    /// Abstract base class for implementing Singleton pattern in Unity.
    /// Ensures a single instance exists and provides global access to it.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class.</typeparam>
    public abstract class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _applicationIsQuitting;

        /// <summary>
        /// Gets the current instance of the singleton.
        /// If the instance is not set and auto-creation is enabled, it will be created at runtime.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of '{typeof(T)}' is already destroyed due to application quitting.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();
                        if (_instance == null)
                        {
                            var tempInstance = new GameObject(typeof(T).Name).AddComponent<T>();
                            _instance = tempInstance;

                            if (!(tempInstance as BaseSingleton<T>).AllowAutoCreation)
                            {
                                Debug.LogError($"[Singleton] Auto-creation is disabled for {typeof(T)}. Destroying.");
                                Destroy(tempInstance.gameObject);
                                _instance = null;
                            }
                            else
                            {
                                Debug.LogWarning($"[Singleton] Auto-created instance of {typeof(T)}.");
                            }
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Returns true if the singleton instance has been initialized.
        /// </summary>
        public static bool IsInitialized => _instance != null;

        /// <summary>
        /// Indicates whether the singleton is allowed to auto-create its own instance if none is found.
        /// Override this in derived classes to enable auto-creation.
        /// </summary>
        //protected static bool AllowAutoCreation => false;
        protected virtual bool AllowAutoCreation => false;

        /// <summary>
        /// Unity Awake callback. Initializes the singleton instance.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                OnSingletonInit();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of '{typeof(T)}' detected. Destroying redundant object.");
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Unity OnDestroy callback. Handles cleanup on destruction.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
                OnSingletonDestroyed();
            }
        }

        /// <summary>
        /// Unity OnApplicationQuit callback. Sets application quitting flag to prevent new instance creation.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// Called once during the singleton initialization. Can be overridden for custom logic.
        /// </summary>
        protected virtual void OnSingletonInit() { }

        /// <summary>
        /// Called when the singleton instance is being destroyed. Can be overridden for custom cleanup.
        /// </summary>
        protected virtual void OnSingletonDestroyed() { }
    }
}

