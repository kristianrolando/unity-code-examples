using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.ObjectPooling
{
    /// <summary>
    /// Simple per-prefab object pool.
    /// - One pool per prefab (key = prefab.GetInstanceID()).
    /// - Prewarm via initialSize.
    /// - Tracks TotalRented / TotalReturned for basic diagnostics.
    /// </summary>
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Queue<GameObject> _pool;
        private readonly int _maxSize;
        private readonly Transform _rootParent;

        public int PoolKey => _prefab.GetInstanceID();
        public int Count => _pool.Count;
        public int TotalRented { get; private set; }
        public int TotalReturned { get; private set; }

        public ObjectPool(GameObject prefab, int initialSize, int maxSize, Transform rootParent)
        {
            _prefab = prefab;
            _maxSize = Mathf.Max(1, maxSize);
            _rootParent = rootParent;
            _pool = new Queue<GameObject>(_maxSize);

            // Prewarm / initial instances
            for (int i = 0; i < initialSize; i++)
            {
                var inst = CreateInstance();
                Return(inst);
            }
        }

        /// <summary>
        /// Get an instance from the pool (or instantiate if empty).
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();

            if (!instance.TryGetComponent<PoolableKey>(out var keyComp))
                keyComp = instance.AddComponent<PoolableKey>();

            keyComp.PoolKey = PoolKey;
            keyComp.IsInPool = false;

            var targetParent = parent != null ? parent : _rootParent;
            var tr = instance.transform;
            tr.SetParent(targetParent, false);
            tr.SetPositionAndRotation(position, rotation);

            instance.SetActive(true);
            TotalRented++;

            return instance;
        }

        /// <summary>
        /// Return an instance back to this pool.
        /// If pool key doesn't match, object is destroyed as a safety fallback.
        /// </summary>
        public void Return(GameObject instance)
        {
            if (instance == null)
                return;

            if (!instance.TryGetComponent(out PoolableKey pKey))
            {
                // Not a pooled object → just destroy.
                Object.Destroy(instance);
                return;
            }

            // Wrong pool → destroy (likely returned via wrong manager/pool).
            if (pKey.PoolKey != PoolKey)
            {
                Object.Destroy(instance);
                return;
            }

            // Already in pool → ignore (prevents double-enqueue bug).
            if (pKey.IsInPool)
                return;

            pKey.IsInPool = true;

            if (_rootParent != null)
                instance.transform.SetParent(_rootParent, false);

            if (_pool.Count < _maxSize)
            {
                instance.SetActive(false);
                _pool.Enqueue(instance);
                TotalReturned++;
            }
            else
            {
                Object.Destroy(instance);
            }
        }

        private GameObject CreateInstance()
        {
            var inst = Object.Instantiate(_prefab);

            if (_rootParent != null)
                inst.transform.SetParent(_rootParent, false);

            if (!inst.TryGetComponent<PoolableKey>(out var keyComp))
                keyComp = inst.AddComponent<PoolableKey>();
            keyComp.PoolKey = PoolKey;
            keyComp.IsInPool = false;

            return inst;
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var inst = _pool.Dequeue();
                if (inst != null)
                    Object.Destroy(inst);
            }
        }
    }

}
