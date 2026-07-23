using System;
using UnityEngine;

namespace ZigdarkS.ProjectB.Service.Projectiles
{
    public readonly struct BallisticProjectileSpawnParams
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;
        public readonly float Speed;
        public readonly float GravityScale;
        public readonly float MaxDistance;
        public readonly float DistanceTravelledBeforeSpawn;
        public readonly float Damage;
        public readonly Func<float, float> GetDamageMultiplier;
        public readonly GameObject ImpactEffectPrefab;
        public readonly float KnockbackForce;
        public readonly GameObject VisualPrefab;

        public BallisticProjectileSpawnParams(
            Vector3 origin, Vector3 direction, float speed, float gravityScale,
            float maxDistance, float distanceTravelledBeforeSpawn, float damage,
            Func<float, float> getDamageMultiplier, GameObject impactEffectPrefab, float knockbackForce,
            GameObject visualPrefab)
        {
            Origin = origin;
            Direction = direction;
            Speed = speed;
            GravityScale = gravityScale;
            MaxDistance = maxDistance;
            DistanceTravelledBeforeSpawn = distanceTravelledBeforeSpawn;
            Damage = damage;
            GetDamageMultiplier = getDamageMultiplier;
            ImpactEffectPrefab = impactEffectPrefab;
            KnockbackForce = knockbackForce;
            VisualPrefab = visualPrefab;
        }
    }
}