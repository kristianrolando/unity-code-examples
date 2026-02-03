using UnityEngine;

namespace Game.Core.ObjectPooling
{
    /// <summary>
    /// Small component used to link an instance to its pool via PoolKey,
    /// and track whether it is currently inside the pool.
    /// </summary>
    public class PoolableKey : MonoBehaviour
    {
        public int PoolKey;
        public bool IsInPool;
    }
}
