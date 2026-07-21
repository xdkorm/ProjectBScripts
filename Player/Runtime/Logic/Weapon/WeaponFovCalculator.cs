using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Logic;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class WeaponFovCalculator : IFovCalculator
    {
        public float Calculate(IWeapon activeWeapon, float baseFov)
        {
            if (activeWeapon == null)
            {
                return baseFov;
            }

            float adsFov = activeWeapon.AdsFovIsFixed
                ? activeWeapon.AdsFixedFov
                : baseFov * activeWeapon.AdsZoomMultiplier;

            // AdsFovCurve может быть не задана (см. TODO в WeaponRuntimeStats: Override кривой
            // модулями сейчас не реализован) — без кривой считаем прогресс линейным.
            float easedProgress = activeWeapon.AdsFovCurve != null
                ? activeWeapon.AdsFovCurve.Evaluate(activeWeapon.AdsProgress)
                : activeWeapon.AdsProgress;

            return Mathf.Lerp(baseFov, adsFov, easedProgress);
        }
    }
}