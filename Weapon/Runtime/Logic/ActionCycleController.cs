using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.View;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class ActionCycleController
    {
        private readonly Func<Data.ActionCycleSettings> _getSettings;
        private readonly AmmoState _ammoState;
        private readonly WeaponView _view;

        private bool _isCycling;
        private float _cycleEndTime;
        private Data.ActionCycleSettings _activeSettings; // настройки, актуальные на момент старта цикла

        public bool IsCycling => _isCycling;

        /// <summary>Требует ли ручного завершения ТЕКУЩИЙ активный цикл (если он идёт), иначе — режим по умолчанию для текущего режима огня.</summary>
        public bool RequiresManualCycle =>
            _isCycling
                ? _activeSettings.CycleMode == Data.ActionCycleMode.Manual
                : _getSettings().CycleMode == Data.ActionCycleMode.Manual;

        public ActionCycleController(Func<Data.ActionCycleSettings> getSettings, AmmoState ammoState, WeaponView view)
        {
            _getSettings = getSettings;
            _ammoState = ammoState;
            _view = view;
        }

        /// <summary>Вызывается сразу после выстрела — берёт актуальные настройки для ТЕКУЩЕГО режима огня.</summary>
        public void StartCycle()
        {
            _activeSettings = _getSettings();
            if (_activeSettings.CycleMode == Data.ActionCycleMode.None) return;

            _isCycling = true;
            _view.PlayCycleAnimation(_activeSettings.CycleDuration);

            if (_activeSettings.CycleMode == Data.ActionCycleMode.Automatic)
            {
                _cycleEndTime = Time.time + _activeSettings.CycleDuration;
            }
        }

        public bool TryManualCycle()
        {
            if (!_isCycling || _activeSettings.CycleMode != Data.ActionCycleMode.Manual) return false;
            CompleteCycle();
            return true;
        }

        public void Tick()
        {
            if (!_isCycling) return;
            if (_activeSettings.CycleMode != Data.ActionCycleMode.Automatic) return;
            if (Time.time >= _cycleEndTime)
            {
                CompleteCycle();
            }
        }

        private void CompleteCycle()
        {
            _isCycling = false;
            _ammoState.ChamberRoundFromTube();
        }

        public void CancelHard()
        {
            _isCycling = false;
        }
    }
}