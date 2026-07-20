using UnityEngine;
using ZigdarkS.ProjectB.Service.Effects;
using ZigdarkS.ProjectB.Service.Pooling;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Отвечает только за спавн декалей попадания (следы от пуль на поверхностях).
    /// </summary>
    public class DecalSpawner
    {
        private readonly GameObjectPoolManager _pools;
        private const float SurfaceOffset = 0.001f;

        public DecalSpawner(GameObjectPoolManager pools)
        {
            _pools = pools;
        }

        public void Spawn(GameObject prefab, Vector3 point, Vector3 normal)
        {
            if (prefab == null) return;

            var pool = _pools.GetPool(prefab);
            var decal = pool.Get();
            decal.transform.SetPositionAndRotation(
                point + normal * SurfaceOffset,
                Quaternion.LookRotation(-normal));

            if (decal.TryGetComponent<PooledEffect>(out var pooled))
                pooled.ReturnTo(pool);
        }
    }
}