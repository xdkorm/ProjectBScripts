using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using ZigdarkS.ProjectB.Service.Raycast;
using ZigdarkS.ProjectB.Service.Projectiles;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class BallisticProjectileService : ITickable
    {
        private readonly HitApplier _hitApplier;
        private readonly WeaponEffectsService _effectsService;
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

        public BallisticProjectileService(HitApplier hitApplier, WeaponEffectsService effectsService)
        {
            _hitApplier = hitApplier;
            _effectsService = effectsService;
        }

        public void SpawnProjectile(BallisticProjectileSpawnParams p)
        {
            GameObject visual = null;
            if (p.VisualPrefab != null)
            {
                var pool = GetOrCreatePool(p.VisualPrefab);
                visual = pool.Get();
                visual.transform.SetPositionAndRotation(p.Origin, Quaternion.LookRotation(p.Direction.normalized));
            }

            var projectile = new ActiveProjectile
            {
                Position = p.Origin,
                Velocity = p.Direction.normalized * p.Speed,
                GravityScale = p.GravityScale,
                DistanceTravelledBeforeSpawn = p.DistanceTravelledBeforeSpawn,
                DistanceTravelledInBallisticPhase = 0f,
                MaxBallisticDistance = p.MaxDistance,
                Damage = p.Damage,
                GetDamageMultiplier = p.GetDamageMultiplier,
                ImpactEffectPrefab = p.ImpactEffectPrefab,
                KnockbackForce = p.KnockbackForce,
                Visual = visual,
                VisualPrefab = p.VisualPrefab
            };

            _activeProjectiles.Add(projectile);
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var p = _activeProjectiles[i];

                p.Velocity += Physics.gravity * p.GravityScale * dt;

                Vector3 desiredNext = p.Position + p.Velocity * dt;
                float desiredStep = Vector3.Distance(p.Position, desiredNext);

                float remainingBudget = p.MaxBallisticDistance - p.DistanceTravelledInBallisticPhase;
                bool willExceedRange = desiredStep >= remainingBudget;
                float actualStep = willExceedRange ? remainingBudget : desiredStep;

                Vector3 stepDirection = p.Velocity.normalized;
                Vector3 stepEnd = p.Position + stepDirection * actualStep;

                if (Physics.Linecast(p.Position, stepEnd, out RaycastHit hit))
                {
                    float totalDistance = p.DistanceTravelledBeforeSpawn + p.DistanceTravelledInBallisticPhase + hit.distance;
                    float damageMultiplier = Mathf.Clamp01(p.GetDamageMultiplier(totalDistance));
                    float appliedDamage = p.Damage * damageMultiplier;

                    _hitApplier.Apply(hit, stepDirection, appliedDamage, p.KnockbackForce);
                    _effectsService.SpawnImpactEffect(p.ImpactEffectPrefab, hit);

                    ReleaseProjectile(p);
                    _activeProjectiles.RemoveAt(i);
                    continue;
                }

                p.Position = stepEnd;
                p.DistanceTravelledInBallisticPhase += actualStep;

                if (p.Visual != null)
                {
                    p.Visual.transform.SetPositionAndRotation(p.Position, Quaternion.LookRotation(stepDirection));
                }
                else
                {
                    Debug.DrawLine(p.Position - stepDirection * actualStep, p.Position, Color.cyan);
                }

                if (willExceedRange)
                {
                    ReleaseProjectile(p);
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

        private void ReleaseProjectile(ActiveProjectile p)
        {
            if (p.Visual != null && p.VisualPrefab != null && _poolsByPrefab.TryGetValue(p.VisualPrefab, out var pool))
            {
                pool.Release(p.Visual);
            }
        }
    }
}