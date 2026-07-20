using System;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    [Serializable]
    public struct DamagePoint
    {
        [Min(0f)]
        [SerializeField] private float _distance;

        [Range(0f, 1f)]
        [SerializeField] private float _damageMultiplier;

        public DamagePoint(float distance, float damageMultiplier)
        {
            _distance = distance;
            _damageMultiplier = damageMultiplier;
        }

        public float Distance => _distance;
        public float DamageMultiplier => _damageMultiplier;
    }

    public static class DamageProfileHelper
    {
        public static float GetDamageMultiplier(DamagePoint[] profile, float distance)
        {
            if (profile == null || profile.Length == 0)
            {
                return 1f;
            }

            if (profile.Length == 1)
            {
                return Mathf.Clamp01(profile[0].DamageMultiplier);
            }

            if (distance <= profile[0].Distance)
            {
                return Mathf.Clamp01(profile[0].DamageMultiplier);
            }

            if (distance >= profile[profile.Length - 1].Distance)
            {
                return Mathf.Clamp01(profile[profile.Length - 1].DamageMultiplier);
            }

            for (int i = 0; i < profile.Length - 1; i++)
            {
                DamagePoint current = profile[i];
                DamagePoint next = profile[i + 1];

                if (distance >= current.Distance && distance <= next.Distance)
                {
                    float segmentLength = next.Distance - current.Distance;
                    if (segmentLength <= 0f)
                    {
                        return Mathf.Clamp01(current.DamageMultiplier);
                    }

                    float t = (distance - current.Distance) / segmentLength;
                    return Mathf.Clamp01(Mathf.Lerp(current.DamageMultiplier, next.DamageMultiplier, t));
                }
            }

            return Mathf.Clamp01(profile[profile.Length - 1].DamageMultiplier);
        }
    }
}
