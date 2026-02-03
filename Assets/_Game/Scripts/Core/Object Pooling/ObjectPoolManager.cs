using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.ObjectPooling
{
    /// <summary>
    /// Global facade for pooling.
    /// - Automatically creates a pool per prefab on first use.
    /// - Clears all pools automatically on scene change.
    /// </summary>
    public static class ObjectPoolManager
    {
        private static readonly Dictionary<int, ObjectPool> _pools = new();

        private static Transform _poolRoot;
        private static bool _initialized;

        static ObjectPoolManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_poolRoot == null)
                _poolRoot = new GameObject("__ObjectPoolRoot__").transform;

            if (_initialized)
                return;

            SceneManager.activeSceneChanged += OnSceneChanged;
            _initialized = true;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            ClearAll();
            _poolRoot = null;
        }

        /// <summary>
        /// Register a prefab with optional prewarm & maxSize.
        /// If already registered, this is a no-op.
        /// </summary>
        public static void RegisterPrefab(GameObject prefab, int initialSize = 0, int maxSize = 200)
        {
            if (prefab == null)
                return;

            Initialize();

            // Guard: user accidentally passes scene object instead of prefab asset
            if (prefab.scene.IsValid())
            {
                Debug.LogWarning(
                    $"[{nameof(ObjectPoolManager)}] Trying to register a prefab that is part of a scene: {prefab.name}. " +
                    "Use a Prefab Asset instead.");
            }

            int key = prefab.GetInstanceID();
            if (_pools.ContainsKey(key))
                return;

            var newPool = new ObjectPool(prefab, initialSize, maxSize, _poolRoot);
            _pools[key] = newPool;
        }

        public static GameObject Get(GameObject prefab, Vector3 position, Transform parent = null)
        {
            return Get(prefab, position, Quaternion.identity, parent);
        }

        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
                return null;

            Initialize();
            RegisterPrefab(prefab);

            int key = prefab.GetInstanceID();
            return _pools[key].Get(position, rotation, parent);
        }

        public static void Return(GameObject obj)
        {
            if (obj == null)
                return;

            Initialize();

            if (obj.TryGetComponent(out PoolableKey poolableKey))
            {
                int key = poolableKey.PoolKey;
                if (_pools.TryGetValue(key, out var pool))
                {
                    pool.Return(obj);
                }
                else
                {
                    // Pool no longer exists (e.g., cleared) → destroy instance.
                    Object.Destroy(obj);
                }
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        public static void ClearPoolForPrefab(GameObject prefab)
        {
            if (prefab == null)
                return;

            Initialize();

            int key = prefab.GetInstanceID();
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
                _pools.Remove(key);
            }
        }

        public static void ClearAll()
        {
            foreach (var pool in _pools.Values)
                pool.Clear();

            _pools.Clear();
        }

        public static bool HasPool(int poolKey) => _pools.ContainsKey(poolKey);
    }
}
