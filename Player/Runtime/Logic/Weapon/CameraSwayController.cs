using UnityEngine;
using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Player.HUD.Crosshair;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic
{
    /// <summary>
    /// Покачивание камеры и оружия вместе (общий pivot) по траектории "восемь"
    /// при удержании прицеливания. Настройки берутся из активного IWeapon
    /// (учитывают модификаторы модулей). Первые AdsSwayStableDuration секунд —
    /// стабильность ("задержка дыхания"), затем амплитуда растёт со временем удержания ADS.
    /// </summary>
    public class CameraSwayController : ITickable
    {
        private readonly InstanceProvider<PlayerView> _playerProvider;
        private readonly CrosshairModel _crosshairModel;
        private readonly IWeaponInventory _inventory;

        private float _adsHoldTime;
        private float _currentAmplitude;
        private float _cycleTime;

        public CameraSwayController(
            InstanceProvider<PlayerView> playerProvider,
            CrosshairModel crosshairModel,
            IWeaponInventory inventory)
        {
            _playerProvider = playerProvider;
            _crosshairModel = crosshairModel;
            _inventory = inventory;
        }

        public void Tick()
        {
            var view = _playerProvider.Instance;
            var activeWeapon = _inventory.ActiveWeapon;
            if (view == null || activeWeapon == null)
            {
                return;
            }

            bool isFullyAiming = _crosshairModel.AdsProgress > 0.95f;

            if (isFullyAiming)
            {
                _adsHoldTime += Time.deltaTime;
                float overStable = Mathf.Max(0f, _adsHoldTime - activeWeapon.AdsSwayStableDuration);
                float targetAmplitude = Mathf.Min(overStable * activeWeapon.AdsSwayAmplitudeGrowthRate, activeWeapon.AdsSwayMaxAmplitude);
                _currentAmplitude = Mathf.MoveTowards(_currentAmplitude, targetAmplitude, Time.deltaTime * activeWeapon.AdsSwayResponseSpeed);
            }
            else
            {
                _adsHoldTime = 0f;
                _currentAmplitude = Mathf.MoveTowards(_currentAmplitude, 0f, Time.deltaTime * activeWeapon.AdsSwayResponseSpeed);
            }

            if (_currentAmplitude <= 0.0001f)
            {
                view.ApplyAdsSwayOffset(Quaternion.identity);
                _cycleTime = 0f;
                return;
            }

            _cycleTime += Time.deltaTime * activeWeapon.AdsSwayFrequency;

            float yaw = Mathf.Sin(_cycleTime) * _currentAmplitude;
            float pitch = Mathf.Sin(_cycleTime * 2f) * _currentAmplitude * 0.5f;

            view.ApplyAdsSwayOffset(Quaternion.Euler(pitch, yaw, 0f));
        }
    }
}