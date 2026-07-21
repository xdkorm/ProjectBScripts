using UnityEngine;
using ZigdarkS.ProjectB.Service.Projectiles;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Владеет рантайм-разбросом от стрельбы (decay/expansion) и ADS-прогрессом.
    /// Берёт пределы и скорости из WeaponRuntimeStats, смешивает с движением через IMovementSpreadProvider.
    /// </summary>
    public class SpreadController
    {
        private readonly WeaponRuntimeStats _stats;
        private readonly IMovementSpreadProvider _movementSpreadProvider;

        private float _currentShootingSpread;
        private float _lastSpreadUpdateTime;
        private float _currentAdsProgress;

        public float AdsProgress => _currentAdsProgress;
        public float EffectiveSpread => GetEffectiveSpread(_currentAdsProgress);

        public float CurrentShootingSpread
        {
            get
            {
                UpdateSpreadDecay();
                return _currentShootingSpread;
            }
        }

        public SpreadController(WeaponRuntimeStats stats, IMovementSpreadProvider movementSpreadProvider)
        {
            _stats = stats;
            _movementSpreadProvider = movementSpreadProvider;
        }

        public void Initialize()
        {
            _currentShootingSpread = _stats.MinHipSpread;
            _lastSpreadUpdateTime = Time.time;
        }

        /// <summary>Сбрасывает таймер decay — вызывается при активации оружия (SetActive(true)).</summary>
        public void ResetTimer()
        {
            _lastSpreadUpdateTime = Time.time;
        }

        public void UpdateAdsProgress(float adsProgress)
        {
            _currentAdsProgress = Mathf.Clamp01(adsProgress);
        }

        public float GetEffectiveSpread(float adsProgress)
        {
            float normalizedAdsProgress = Mathf.Clamp01(adsProgress);
            float baseSpread = Mathf.Lerp(CurrentShootingSpread, _stats.AimSpread, normalizedAdsProgress);

            float movementOffset = _movementSpreadProvider != null
                ? _movementSpreadProvider.CurrentSpreadOffset
                : 0f;

            return baseSpread + movementOffset;
        }

        public Vector3 CalculateSpreadDirection(Vector3 direction, float spread)
        {
            if (spread <= 0f) return direction;
            return (direction + Random.insideUnitSphere * spread).normalized;
        }

        public void ExpandSpread()
        {
            if (_stats.MaxHipSpread >= _stats.MinHipSpread)
            {
                _currentShootingSpread = Mathf.Min(_currentShootingSpread + _stats.SpreadPerShot, _stats.MaxHipSpread);
            }
            else
            {
                _currentShootingSpread = Mathf.Max(_currentShootingSpread - _stats.SpreadPerShot, _stats.MaxHipSpread);
            }
        }

        private void UpdateSpreadDecay()
        {
            if (Time.time <= _lastSpreadUpdateTime) return;
            float timeSinceLastShot = Time.time - _lastSpreadUpdateTime;
            _lastSpreadUpdateTime = Time.time;

            if (_stats.MaxHipSpread >= _stats.MinHipSpread)
            {
                _currentShootingSpread = Mathf.Max(_currentShootingSpread - _stats.SpreadDecaySpeed * timeSinceLastShot, _stats.MinHipSpread);
            }
            else
            {
                _currentShootingSpread = Mathf.Min(_currentShootingSpread + _stats.SpreadDecaySpeed * timeSinceLastShot, _stats.MinHipSpread);
            }
        }
    }
}