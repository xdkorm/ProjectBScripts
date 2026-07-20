using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public interface IWeapon
    {
        string Name { get; }
        bool CanFire { get; }
        bool IsActive { get; }
        WeaponConfig Config { get; }
        FireMode CurrentFireMode { get; }
        float MinHipSpread { get; }
        float MaxHipSpread { get; }
        float AimSpread { get; }
        float SpreadPerShot { get; }
        float SpreadDecaySpeed { get; }
        float CurrentShootingSpread { get; }
        float AdsDuration { get; }
        float AdsProgress { get; }
        float MoveSpreadMultiplier { get; }
        float AdsMoveSpreadMultiplier { get; }
        float CrouchMoveSpreadMultiplier { get; }
        float AdsCrouchMoveSpreadMultiplier { get; }
        float AirborneSpreadPenalty { get; }
        float AdsAirborneSpreadPenalty { get; }
        float HipSprintSpeedMultiplier { get; }
        float AimSpeedMultiplier { get; }
        float AimSprintSpeedMultiplier { get; }
        float HipForwardSpeedMultiplier { get; }
        float AimForwardSpeedMultiplier { get; }
        float HipStrafeSpeedMultiplier { get; }
        float AimStrafeSpeedMultiplier { get; }
        float HipBackwardSpeedMultiplier { get; }
        float AimBackwardSpeedMultiplier { get; }
        float SprintToFireDelay { get; }
        float GetEffectiveSpread(float adsProgress);
        bool TryFire(Func<Vector3> getOrigin, Func<Vector3> getDirection, Func<bool> holdCondition);
        void SwitchToNextFireMode();
        void ToggleSafetyMode();
        void UpdateAdsProgress(float adsProgress);
        void SetActive(bool isActive);
        void Dispose();
        float AdsZoomMultiplier { get; }
        AnimationCurve AdsFovCurve { get; }
        bool AdsFovIsFixed { get; }
        float AdsFixedFov { get; }
        int BulletsInMagazine { get; }
        int ReserveAmmo { get; }
        int MagazineSize { get; }
        bool IsReloading { get; }
        float ReloadProgress01 { get; } // для HUD-полоски перезарядки, 0..1
        bool HasAmmoToFire { get; }
        bool TryReload();
        void TickReload(); // либо назвать Tick(), если хочешь унифицировать на будущее
        event Action OnReloadStarted;
        event Action OnReloadFinished;
        event Action OnAmmoChanged;
        void CancelReload();
        float DrawDuration { get; }
        float HolsterDuration { get; }
        void ResetAds();
        void PlayDrawAnimation();
        void PlayHolsterAnimation();
//        event Action<HitResult, WeaponConfig> OnHit;
        bool IsCycling { get; }
        bool RequiresManualCycle { get; }
        bool TryManualCycle();

        float AdsSwayStableDuration { get; }
        float AdsSwayAmplitudeGrowthRate { get; }
        float AdsSwayMaxAmplitude { get; }
        float AdsSwayFrequency { get; }
        float AdsSwayResponseSpeed { get; }
    }
}