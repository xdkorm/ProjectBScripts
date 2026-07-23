using System;
using System.Collections.Generic;
using UnityEngine;
using ZigdarkS.ProjectB.Core.Combat;
using ZigdarkS.ProjectB.Weapon.Data;
using ZigdarkS.ProjectB.Weapon.View;
using ZigdarkS.ProjectB.Service.Raycast;
using ZigdarkS.ProjectB.Service.Projectiles;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class WeaponInstance : IWeapon, IRaycastWeaponData, IDisposable
    {
        private readonly WeaponConfig _config;
        private readonly WeaponView _view;
        private readonly RaycastFireService _fireService;
        private readonly WeaponEffectsCoordinator _effectsService;
        private readonly WeaponRuntimeStats _stats;
        private readonly FireModeController _fireModeController;
        private readonly SpreadController _spreadController;
        private readonly BallisticProjectileSimulator _ballisticService;
        private readonly ActionCycleController _actionCycleController;
        private readonly WeaponStateMachine _stateMachine;
        private readonly List<WeaponModule> _equippedModules = new();

        private readonly AmmoState _ammoState;
        private readonly ReloadController _reloadController;

        public string Name => _config.WeaponName;

        public int BulletsInMagazine => _ammoState.UsesChamberSlot
            ? _ammoState.InTube + _ammoState.InChamber
            : _ammoState.InTube;
        public int ReserveAmmo => _ammoState.InReserve;
        public int MagazineSize => _config.Ammo.MagazineSize;
        public bool IsReloading => _stateMachine.Current == WeaponState.Reloading;
        public bool HasAmmoToFire => _ammoState.HasAmmoToFire;
        public float ReloadProgress01 => _reloadController.Progress01;
        public float DrawDuration => _config.Equip.DrawDuration;
        public float HolsterDuration => _config.Equip.HolsterDuration;
        public bool IsCycling => _stateMachine.Current == WeaponState.CyclingAction;
        public bool RequiresManualCycle => _actionCycleController.RequiresManualCycle;

        private float _nextTimeToFire;
        private bool _isActive;
        private Coroutine _burstCoroutine;

        public event Action OnAmmoChanged
        {
            add => _ammoState.OnAmmoChanged += value;
            remove => _ammoState.OnAmmoChanged -= value;
        }

//        public event Action<HitResult, WeaponConfig> OnHit;

        public bool CanFire => Time.time >= _nextTimeToFire
            && !_fireModeController.CurrentMode.IsSafety
            && _stateMachine.CanFire;

        public bool IsActive => _isActive;
        public WeaponConfig Config => _config;
        public FireMode CurrentFireMode => _fireModeController.CurrentMode;
        public RaycastFireService FireService => _fireService;
        public WeaponView View => _view;
        public float EffectiveSpread => _spreadController.EffectiveSpread;

        // --- Проброски статов ---
        public float Damage => _stats.Damage;
        public float Range => _stats.Range;
        public float CurrentRpm => _stats.CurrentRpm;
        public int PelletCount => _stats.PelletCount;
        public float MinHipSpread => _stats.MinHipSpread;
        public float MaxHipSpread => _stats.MaxHipSpread;
        public float AimSpread => _stats.AimSpread;
        public float SpreadPerShot => _stats.SpreadPerShot;
        public float SpreadDecaySpeed => _stats.SpreadDecaySpeed;
        public float AdsDuration => _stats.AdsDuration;
        public bool CanRunWhileAds => _stats.CanRunWhileAds;
        public float MoveSpreadMultiplier => _stats.MoveSpreadMultiplier;
        public float AdsMoveSpreadMultiplier => _stats.AdsMoveSpreadMultiplier;
        public float CrouchMoveSpreadMultiplier => _stats.CrouchMoveSpreadMultiplier;
        public float AdsCrouchMoveSpreadMultiplier => _stats.AdsCrouchMoveSpreadMultiplier;
        public float AirborneSpreadPenalty => _stats.AirborneSpreadPenalty;
        public float AdsAirborneSpreadPenalty => _stats.AdsAirborneSpreadPenalty;
        public float HipSprintSpeedMultiplier => _stats.HipSprintSpeedMultiplier;
        public float AimSpeedMultiplier => _stats.AimSpeedMultiplier;
        public float AimSprintSpeedMultiplier => _stats.AimSprintSpeedMultiplier;
        public float HipForwardSpeedMultiplier => _stats.HipForwardSpeedMultiplier;
        public float AimForwardSpeedMultiplier => _stats.AimForwardSpeedMultiplier;
        public float HipStrafeSpeedMultiplier => _stats.HipStrafeSpeedMultiplier;
        public float AimStrafeSpeedMultiplier => _stats.AimStrafeSpeedMultiplier;
        public float HipBackwardSpeedMultiplier => _stats.HipBackwardSpeedMultiplier;
        public float AimBackwardSpeedMultiplier => _stats.AimBackwardSpeedMultiplier;
        public GameObject ImpactEffectPrefab => _stats.ImpactEffectPrefab;
        public float KnockbackForce => _stats.KnockbackForce;
        public bool CanShootWhileRunning => _stats.CanShootWhileRunning;
        public float SprintToFireDelay => _config.BaseStats.SprintToFireDelay;
        public float AdsZoomMultiplier => _stats.AdsZoomMultiplier;
        public AnimationCurve AdsFovCurve => _stats.AdsFovCurve;
        public bool AdsFovIsFixed => _stats.AdsFovIsFixed;
        public float AdsFixedFov => _stats.AdsFixedFov;

        public float AdsSwayStableDuration => _stats.AdsSwayStableDuration;
        public float AdsSwayAmplitudeGrowthRate => _stats.AdsSwayAmplitudeGrowthRate;
        public float AdsSwayMaxAmplitude => _stats.AdsSwayMaxAmplitude;
        public float AdsSwayFrequency => _stats.AdsSwayFrequency;
        public float AdsSwayResponseSpeed => _stats.AdsSwayResponseSpeed;

        public float AdsProgress => _spreadController.AdsProgress;
        public float CurrentShootingSpread => _spreadController.CurrentShootingSpread;

        public float GetDamageMultiplier(float distance)
        {
            float baseMultiplier = _stats.GetBaseDamageMultiplier(distance);
            var mode = _fireModeController.CurrentMode;
            return mode != null ? mode.GetDamageMultiplier(distance, baseMultiplier) : baseMultiplier;
        }

        public WeaponInstance(
            WeaponConfig config,
            WeaponView view,
            RaycastFireService fireService,
            IMovementSpreadProvider movementSpreadProvider,
            WeaponEffectsCoordinator effectsService,
            BallisticProjectileSimulator ballisticService,
            AmmoState ammoState,
            ReloadController reloadController)
        {
            _config = config;
            _view = view;
            _fireService = fireService;
            _effectsService = effectsService;
            _ballisticService = ballisticService;
            _ammoState = ammoState;
            _reloadController = reloadController;
            _stats = new WeaponRuntimeStats(config);
            _fireModeController = new FireModeController(config);
            _spreadController = new SpreadController(_stats, movementSpreadProvider);
            _actionCycleController = new ActionCycleController(GetEffectiveActionCycleSettings, ammoState, view);
            _stateMachine = new WeaponStateMachine(
                ammoState, reloadController, _actionCycleController, config.Ammo, GetEffectiveActionCycleSettings);

            _fireModeController.OnFireModeChanged += HandleFireModeChanged;

            if (_config.DefaultModules != null)
            {
                _equippedModules.AddRange(_config.DefaultModules);
            }

            RecalculateStats();
            _spreadController.Initialize();
        }

        private Data.ActionCycleSettings GetEffectiveActionCycleSettings()
        {
            var mode = _fireModeController.CurrentMode;
            return mode != null ? mode.ResolveActionCycle(_config.ActionCycle) : _config.ActionCycle;
        }

        public void RecalculateStats()
        {
            _fireModeController.ValidateCurrentMode();
            _stats.RecalculateStats(_equippedModules, _fireModeController.CurrentMode);
        }

        private void HandleFireModeChanged()
        {
            RecalculateStats();
            ApplyFireModeAnimations();
        }

        public bool TryFire(Func<Vector3> getOrigin, Func<Vector3> getDirection, Func<bool> holdCondition)
        {
            if (!_stateMachine.HandleFireAttempt()) return false;
            if (!CanFire) return false;

            _fireModeController.ExecuteFire(this, getOrigin, getDirection, holdCondition);
            return true;
        }

        public void ExecuteSingleShot(Vector3 origin, Vector3 direction)
        {
            int pellets = _stats.PelletCount > 0 ? _stats.PelletCount : 1;
            float spread = EffectiveSpread;

            bool useBallistic = _config.Ballistic.UsesBallisticTransition;
            float hitscanDistance = useBallistic ? Mathf.Min(_config.Ballistic.HitscanRange, _stats.Range) : _stats.Range;

            for (int i = 0; i < pellets; i++)
            {
                Vector3 spreadDirection = _spreadController.CalculateSpreadDirection(direction, spread);
                RaycastShotResult result = _fireService.ExecuteRaycastShot(origin, spreadDirection, this, hitscanDistance);

                if (!result.DidHit && useBallistic)
                {
                    float remainingDistance = _stats.Range - hitscanDistance;
                    if (remainingDistance > 0f)
                    {
                        _ballisticService.SpawnProjectile(new BallisticProjectileSpawnParams(
                        origin: result.EndPoint,
                        direction: spreadDirection,
                        speed: _config.Ballistic.ProjectileSpeed,
                        gravityScale: _config.Ballistic.ProjectileGravityScale,
                        maxDistance: remainingDistance,
                        distanceTravelledBeforeSpawn: hitscanDistance,
                        damage: _stats.Damage,
                        getDamageMultiplier: GetDamageMultiplier,
                        impactEffectPrefab: _stats.ImpactEffectPrefab,
                        knockbackForce: _stats.KnockbackForce,
                        visualPrefab: _config.Ballistic.ProjectileVisualPrefab));
                    }
                }
            }

            _view?.PlayMuzzleFlash();

            if (_config.Ammo.EjectsCasingOnFire)
                _spreadController.ExpandSpread();

            _ammoState.ConsumeBullet();
            _stateMachine.NotifyShotFired();
        }

        public event Action OnReloadStarted
        {
            add => _stateMachine.OnReloadStarted += value;
            remove => _stateMachine.OnReloadStarted -= value;
        }
        public event Action OnReloadFinished
        {
            add => _stateMachine.OnReloadFinished += value;
            remove => _stateMachine.OnReloadFinished -= value;
        }

        public bool TryReload() => _stateMachine.RequestReload();
        public bool TryManualCycle() => _stateMachine.RequestManualCycle();

        public void TickReload() => _stateMachine.Tick();

        public void SwitchToNextFireMode()
        {
            if (!_stateMachine.CanSwitchFireMode) return;
            _fireModeController.SwitchToNextFireMode();
        }

        public void ToggleSafetyMode()
        {
            if (!_stateMachine.CanSwitchFireMode) return;
            _fireModeController.ToggleSafetyMode();
        }

        public void SetNextTimeToFire(float time) => _nextTimeToFire = time;
        public void RegisterBurstCoroutine(Coroutine coroutine) => _burstCoroutine = coroutine;

        public void UpdateAdsProgress(float adsProgress) => _spreadController.UpdateAdsProgress(adsProgress);
        public float GetEffectiveSpread(float adsProgress) => _spreadController.GetEffectiveSpread(adsProgress);
        public Vector3 CalculateSpreadDirection(Vector3 direction, float spread) => _spreadController.CalculateSpreadDirection(direction, spread);

        private void ApplyFireModeAnimations()
        {
            var mode = _fireModeController.CurrentMode;
            AnimatorOverrideController targetOverride = mode != null && mode.AnimatorOverride != null
                ? mode.AnimatorOverride
                : _config.Visuals.AnimatorOverride;

            if (_view != null && targetOverride != null)
            {
                _view.ApplyAnimatorOverride(targetOverride);
            }
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
            if (_view != null)
            {
                _view.gameObject.SetActive(isActive);
                if (!isActive && _burstCoroutine != null)
                {
                    _view.StopCoroutine(_burstCoroutine);
                }
            }

            if (!isActive)
            {
                _stateMachine.NotifyHolster();
            }
            else
            {
                _spreadController.ResetTimer();
                ApplyFireModeAnimations();
                _stateMachine.NotifyDraw();
            }
        }

        public void Dispose()
        {
            _fireModeController.OnFireModeChanged -= HandleFireModeChanged;
            _stateMachine.Dispose();
            _isActive = false;
            if (_view != null)
            {
                UnityEngine.Object.Destroy(_view.gameObject);
            }
        }

        public void CancelReload() => _reloadController.Cancel();

        public void ResetAds()
        {
            _spreadController.UpdateAdsProgress(0f);
        }

        public void PlayDrawAnimation()
        {
            _view?.PlayDrawAnimation();
        }

        public void PlayHolsterAnimation()
        {
            _view?.PlayHolsterAnimation();
        }
    }
}