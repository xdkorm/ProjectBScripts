using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Service.Weapon;
using ZigdarkS.ProjectB.Weapon.Logic;

namespace ZigdarkS.ProjectB.Service.Raycast
{
    public class RaycastFireService
    {
        private readonly HitApplier _hitApplier;
        private readonly WeaponEffectsService _effectsService;

        public RaycastFireService(HitApplier hitApplier, WeaponEffectsService effectsService)
        {
            _hitApplier = hitApplier;
            _effectsService = effectsService;
        }

        public RaycastShotResult ExecuteRaycastShot(Vector3 origin, Vector3 direction, IRaycastWeaponData weaponData, float maxDistance)
        {
            Vector3 traceEndPoint = origin + direction * maxDistance;

            if (Physics.Raycast(origin, direction, out RaycastHit bulletHit, maxDistance))
            {
                Debug.DrawLine(origin, bulletHit.point, Color.green, 1.5f);
                Debug.DrawRay(bulletHit.point, bulletHit.normal * 0.3f, Color.red, 1.5f);

                float damageMultiplier = Mathf.Clamp01(weaponData.GetDamageMultiplier(bulletHit.distance));
                float appliedDamage = weaponData.Damage * damageMultiplier;

                _hitApplier.Apply(bulletHit, direction, appliedDamage, weaponData.KnockbackForce);
                _effectsService.SpawnImpactEffect(weaponData.ImpactEffectPrefab, bulletHit);

                return new RaycastShotResult(true, bulletHit.point, bulletHit.normal, bulletHit.collider);
            }

            Debug.DrawLine(origin, traceEndPoint, Color.yellow, 0.5f);
            return new RaycastShotResult(false, traceEndPoint, Vector3.zero, null);
        }
    }
}