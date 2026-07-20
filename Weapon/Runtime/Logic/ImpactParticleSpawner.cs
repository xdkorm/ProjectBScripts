using UnityEngine;
using ZigdarkS.ProjectB.Service.Pooling;
using ZigdarkS.ProjectB.Service.Effects;
namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Отвечает только за спавн партикловых эффектов попадания.
    /// </summary>
    public class ImpactParticleSpawner
    {
        private readonly GameObjectPoolManager _pools;

        public ImpactParticleSpawner(GameObjectPoolManager pools)
        {
            _pools = pools;
        }

        public void Spawn(GameObject prefab, Vector3 point, Vector3 normal)
        {
            if (prefab == null) return;

            var pool = _pools.GetPool(prefab);
            var instance = pool.Get();
            instance.transform.SetPositionAndRotation(point, Quaternion.LookRotation(normal));

            if (instance.TryGetComponent<ParticleSystem>(out var ps))
                ps.Play();

            if (instance.TryGetComponent<PooledEffect>(out var pooled))
                pooled.ReturnTo(pool);
        }
    }
}