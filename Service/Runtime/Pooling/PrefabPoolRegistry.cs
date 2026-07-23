using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ZigdarkS.ProjectB.Service.Pooling
{
    /// <summary>
    /// Универсальный менеджер пулов GameObject'ов по префабу.
    /// Не завязан на конкретную предметную область (оружие, VFX и т.д.) —
    /// может использоваться где угодно, где нужен пул инстансов префаба.
    /// </summary>
    public class PrefabPoolRegistry
    {
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
        private readonly Transform _poolRoot;
        private readonly int _defaultCapacity;

        public PrefabPoolRegistry(Transform poolRoot, int defaultCapacity = 16)
        {
            _poolRoot = poolRoot;
            _defaultCapacity = defaultCapacity;
        }

        public ObjectPool<GameObject> GetPool(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("[GameObjectPoolManager] Попытка получить пул для null-префаба.");
                return null;
            }

            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<GameObject>(
                    createFunc: () => Object.Instantiate(prefab, _poolRoot),
                    actionOnGet: go => go.SetActive(true),
                    actionOnRelease: go => go.SetActive(false),
                    actionOnDestroy: Object.Destroy,
                    defaultCapacity: _defaultCapacity);
                _pools[prefab] = pool;
            }
            return pool;
        }
    }
}