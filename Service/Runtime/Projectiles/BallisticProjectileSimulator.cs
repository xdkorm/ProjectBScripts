using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using ZigdarkS.ProjectB.Service.Raycast;
using ZigdarkS.ProjectB.Core.Combat;

namespace ZigdarkS.ProjectB.Service.Projectiles
{
    public class BallisticProjectileSimulator : ITickable
    {
        private readonly HitApplier _hitApplier;
        private readonly IImpactEffectSpawner _effectsSpawner;
        private readonly List<ActiveProjectile> _activeProjectiles = new();
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByPrefab = new();

        private class ActiveProjectile
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float GravityScale;
            public float DistanceTravelledBeforeSpawn;
            public float DistanceTravelledInBallisticPhase;
            public float MaxBallisticDistance;
            public float Damage;
            public System.Func<float, float> GetDamageMultiplier;
            public GameObject ImpactEffectPrefab;
            public float KnockbackForce;
            public GameObject Visual;
            public GameObject VisualPrefab;
        }

        public BallisticProjectileSimulator(HitApplier hitApplier, IImpactEffectSpawner effectsSpawner)
        {
            _hitApplier = hitApplier;
            _effectsSpawner = effectsSpawner;
        }

        public void SpawnProjectile(BallisticProjectileSpawnParams spawnParams)
        {
            GameObject visual = null;
            if (spawnParams.VisualPrefab != null)
            {
                var pool = GetOrCreatePool(spawnParams.VisualPrefab);
                visual = pool.Get();
                visual.transform.SetPositionAndRotation(spawnParams.Origin, Quaternion.LookRotation(spawnParams.Direction.normalized));
            }

            var projectile = new ActiveProjectile
            {
                Position = spawnParams.Origin,
                Velocity = spawnParams.Direction.normalized * spawnParams.Speed,
                GravityScale = spawnParams.GravityScale,
                DistanceTravelledBeforeSpawn = spawnParams.DistanceTravelledBeforeSpawn,
                DistanceTravelledInBallisticPhase = 0f,
                MaxBallisticDistance = spawnParams.MaxDistance,
                Damage = spawnParams.Damage,
                GetDamageMultiplier = spawnParams.GetDamageMultiplier,
                ImpactEffectPrefab = spawnParams.ImpactEffectPrefab,
                KnockbackForce = spawnParams.KnockbackForce,
                Visual = visual,
                VisualPrefab = spawnParams.VisualPrefab
            };

            _activeProjectiles.Add(projectile);
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _activeProjectiles[i];

                projectile.Velocity += Physics.gravity * projectile.GravityScale * dt;

                Vector3 desiredNext = projectile.Position + projectile.Velocity * dt;
                float desiredStep = Vector3.Distance(projectile.Position, desiredNext);

                float remainingBudget = projectile.MaxBallisticDistance - projectile.DistanceTravelledInBallisticPhase;
                bool willExceedRange = desiredStep >= remainingBudget;
                float actualStep = willExceedRange ? remainingBudget : desiredStep;

                Vector3 stepDirection = projectile.Velocity.normalized;
                Vector3 stepEnd = projectile.Position + stepDirection * actualStep;

                if (Physics.Linecast(projectile.Position, stepEnd, out RaycastHit hit))
                {
                    float totalDistance = projectile.DistanceTravelledBeforeSpawn + projectile.DistanceTravelledInBallisticPhase + hit.distance;
                    float damageMultiplier = Mathf.Clamp01(projectile.GetDamageMultiplier(totalDistance));
                    float appliedDamage = projectile.Damage * damageMultiplier;

                    _hitApplier.Apply(hit, stepDirection, appliedDamage, projectile.KnockbackForce);
                    _effectsSpawner.SpawnImpactEffect(projectile.ImpactEffectPrefab, hit);

                    ReleaseProjectile(projectile);
                    _activeProjectiles.RemoveAt(i);
                    continue;
                }

                projectile.Position = stepEnd;
                projectile.DistanceTravelledInBallisticPhase += actualStep;

                if (projectile.Visual != null)
                {
                    projectile.Visual.transform.SetPositionAndRotation(projectile.Position, Quaternion.LookRotation(stepDirection));
                }
                else
                {
                    Debug.DrawLine(projectile.Position - stepDirection * actualStep, projectile.Position, Color.cyan);
                }

                if (willExceedRange)
                {
                    ReleaseProjectile(projectile);
                    _activeProjectiles.RemoveAt(i);
                }
            }
        }

        private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (_poolsByPrefab.TryGetValue(prefab, out var existingPool))
            {
                return existingPool;
            }

            var pool = new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(prefab),
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    if (go.TryGetComponent<TrailRenderer>(out var trail)) trail.Clear();
                    go.SetActive(false);
                },
                actionOnDestroy: go => Object.Destroy(go),
                defaultCapacity: 16);

            _poolsByPrefab[prefab] = pool;
            return pool;
        }

        private void ReleaseProjectile(ActiveProjectile projectile)
        {
            if (projectile.Visual != null && projectile.VisualPrefab != null && _poolsByPrefab.TryGetValue(projectile.VisualPrefab, out var pool))
            {
                pool.Release(projectile.Visual);
            }
        }
    }
}