using UnityEngine;

namespace ZigdarkS.ProjectB.Core.Combat
{
    /// <summary>
    /// Абстракция над спавном эффекта попадания. Нужна, чтобы Service-слой
    /// (BallisticProjectileService) мог просить проиграть эффект попадания,
    /// не зная о WeaponEffectsService напрямую (тот живёт в Weapon.Runtime,
    /// а Service не должен на него ссылаться — иначе цикл зависимостей).
    /// </summary>
    public interface IImpactEffectSpawner
    {
        void SpawnImpactEffect(GameObject weaponOverridePrefab, RaycastHit hit);
    }
}