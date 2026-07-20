using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class FireModeController
    {
        private readonly WeaponConfig _config;
        private int _currentModeIndex;
        private FireMode _currentMode;

        private readonly IFireBehaviour _standardBehaviour = new StandardFireBehaviour();
        private readonly IFireBehaviour _safetyBehaviour = new SafetyFireBehaviour();
        private readonly IFireBehaviour _burstBehaviour = new BurstFireBehaviour();

        public FireMode CurrentMode => _currentMode;

        public event Action OnFireModeChanged;

        public FireModeController(WeaponConfig config)
        {
            _config = config;
            InitializeDefaultFireMode();
        }

        private void InitializeDefaultFireMode()
        {
            if (_config.FireModes == null || _config.FireModes.Count == 0)
            {
                Debug.LogError($"У оружия {_config.WeaponName} нет доступных режимов стрельбы!");
                return;
            }

            _currentModeIndex = Mathf.Clamp(_config.DefaultModeIndex, 0, _config.FireModes.Count - 1);
            _currentMode = _config.FireModes[_currentModeIndex];
        }

        public void ValidateCurrentMode()
        {
            if (_config.FireModes == null || _config.FireModes.Count == 0) return;

            bool currentModeExists = false;
            if (_currentMode != null)
            {
                for (int i = 0; i < _config.FireModes.Count; i++)
                {
                    if (_config.FireModes[i] == _currentMode)
                    {
                        currentModeExists = true;
                        _currentModeIndex = i;
                        break;
                    }
                }
            }

            bool shouldRestoreDefaultMode = _currentMode == null
                || _currentModeIndex < 0
                || _currentModeIndex >= _config.FireModes.Count
                || !currentModeExists;

            if (shouldRestoreDefaultMode)
            {
                _currentModeIndex = Mathf.Clamp(_config.DefaultModeIndex, 0, _config.FireModes.Count - 1);
                _currentMode = _config.FireModes[_currentModeIndex];
            }
        }

        public void SwitchToNextFireMode()
        {
            if (_config.FireModes == null || _config.FireModes.Count <= 1) return;

            int attempts = 0;
            int nextIndex = _currentModeIndex;

            while (attempts < _config.FireModes.Count)
            {
                nextIndex = (nextIndex + 1) % _config.FireModes.Count;
                attempts++;

                if (!_config.FireModes[nextIndex].IsSafety)
                {
                    _currentModeIndex = nextIndex;
                    _currentMode = _config.FireModes[_currentModeIndex];

                    Debug.Log($"Режим стрельбы изменен на: {_currentMode.ModeName}");
                    OnFireModeChanged?.Invoke();
                    return;
                }
            }
        }

        public void ToggleSafetyMode()
        {
            if (_currentMode != null && _currentMode.IsSafety)
            {
                SwitchToNextFireMode();
                return;
            }

            for (int i = 0; i < _config.FireModes.Count; i++)
            {
                if (_config.FireModes[i].IsSafety)
                {
                    _currentModeIndex = i;
                    _currentMode = _config.FireModes[i];

                    Debug.Log("Оружие поставлено на предохранитель (Safety)");
                    OnFireModeChanged?.Invoke();
                    return;
                }
            }
        }

        /// <summary>Резолвит и выполняет поведение, соответствующее текущему режиму огня.</summary>
        public void ExecuteFire(WeaponInstance weapon, Func<Vector3> getOrigin, Func<Vector3> getDirection, Func<bool> holdCondition)
        {
            ResolveBehaviour(_currentMode).Execute(weapon, _currentMode, getOrigin, getDirection, holdCondition);
        }

        private IFireBehaviour ResolveBehaviour(FireMode mode)
        {
            return mode switch
            {
                SafetyFireMode => _safetyBehaviour,
                BurstFireMode => _burstBehaviour,
                _ => _standardBehaviour
            };
        }
    }
}