using UnityEngine;

namespace ZigdarkS.ProjectB.Core
{
    public interface IRaycastWeaponData
    {
        float Range { get; }
        float Damage { get; }
        float GetDamageMultiplier(float distance);
        GameObject ImpactEffectPrefab { get; }
        float KnockbackForce { get; }
    }
}