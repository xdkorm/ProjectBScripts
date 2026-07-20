using UnityEngine;
using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Player.HUD.Crosshair;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class WeaponAdsSystem : ITickable
    {
        private readonly IActivePlayerView _activeView;
        private readonly IInputService _inputService;
        private readonly IWeaponInventory _inventory;
        private readonly CrosshairModel _crosshairModel;
        private readonly PlayerSettings _playerSettings;
        private readonly IFovCalculator _fovCalculator;
        private float _currentAdsProgress;

        public WeaponAdsSystem(
            IActivePlayerView activeView,
            IInputService inputService,
            IWeaponInventory inventory,
            CrosshairModel crosshairModel,
            PlayerSettings playerSettings,
            IFovCalculator fovCalculator)
        {
            _activeView = activeView;
            _inputService = inputService;
            _inventory = inventory;
            _crosshairModel = crosshairModel;
            _playerSettings = playerSettings;
            _fovCalculator = fovCalculator;
        }

        public void Tick()
        {
            var view = _activeView.Current;
            if (view == null) return;

            var activeWeapon = _inventory.ActiveWeapon;
            if (activeWeapon == null)
            {
                _currentAdsProgress = Mathf.MoveTowards(_currentAdsProgress, 0f, Time.deltaTime * 5f);
                _crosshairModel.UpdateAdsProgress(_currentAdsProgress);
                view.SetCameraFOV(_playerSettings.Fov);
                return;
            }

            bool isReady = _inventory.IsWeaponReady;
            bool isAimingInput = isReady && _inputService.IsAiming();
            float targetAds = isAimingInput ? 1f : 0f;
            float duration = activeWeapon.AdsDuration;
            if (duration <= 0f) duration = 0.001f;

            _currentAdsProgress = Mathf.MoveTowards(_currentAdsProgress, targetAds, Time.deltaTime / duration);
            activeWeapon.UpdateAdsProgress(_currentAdsProgress);
            _crosshairModel.UpdateAdsProgress(_currentAdsProgress);

            float currentFov = _fovCalculator.Calculate(activeWeapon, _playerSettings.Fov);
            view.SetCameraFOV(currentFov);
        }
    }
}