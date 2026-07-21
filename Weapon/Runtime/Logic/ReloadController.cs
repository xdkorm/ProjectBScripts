using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;
using ZigdarkS.ProjectB.Weapon.Logic;
using ZigdarkS.ProjectB.Weapon.View;
using ZigdarkS.ProjectB.Service.Projectiles;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class ReloadController
    {
        private enum InternalState { Idle, Stepping, Exiting }

        private readonly Data.AmmoSettings _settings;
        private readonly Data.ReloadMode _mode;
        private readonly AmmoState _state;
        private readonly WeaponView _view;
        private readonly WeaponEffectsService _effectsService;

        private InternalState _internalState = InternalState.Idle;
        private bool _isFirstStep;
        private float _nextEventTime;

        public bool IsReloading => _internalState != InternalState.Idle;
        public float Progress01 { get; private set; }

        public event Action OnReloadStarted;
        public event Action OnReloadFinished;
        public event Action OnReloadCancelled;

        public ReloadController(Data.AmmoSettings settings, Data.ReloadMode mode, AmmoState state,
            WeaponView view, WeaponEffectsService effectsService)
        {
            _settings = settings;
            _mode = mode;
            _state = state;
            _view = view;
            _effectsService = effectsService;
        }

        public bool TryStartReload()
        {
            if (_internalState != InternalState.Idle) return false;
            if (_state.InReserve <= 0) return false;

            // Труба полная и (если есть патронник) он тоже занят — перезаряжать нечего
            bool tubeFull = _state.InTube >= _settings.MagazineSize;
            bool chamberBlocksReload = _state.UsesChamberSlot && _state.InChamber > 0;
            if (!_settings.CanReloadWithFullMag && tubeFull && (!_state.UsesChamberSlot || chamberBlocksReload))
                return false;

            StartStep(isFirstStep: true);

            if (_settings.DropsMagazineOnReload)
                _effectsService?.SpawnMagazineDrop(_view, _settings.MagazinePrefab);

            _view.SetMagazineVisualActive(false);
            _view.PlayReloadAnimation(_mode.GetStepDuration(_settings, true));

            OnReloadStarted?.Invoke();
            return true;
        }

        private void StartStep(bool isFirstStep)
        {
            _internalState = InternalState.Stepping;
            _isFirstStep = isFirstStep;
            float duration = _mode.GetStepDuration(_settings, isFirstStep);
            _nextEventTime = Time.time + duration;
        }

        public void Tick()
        {
            if (_internalState == InternalState.Idle) return;
            if (Time.time < _nextEventTime) return;

            if (_internalState == InternalState.Stepping)
            {
                bool needsMoreSteps = _mode.ApplyStep(_state, _settings);

                if (needsMoreSteps)
                {
                    StartStep(isFirstStep: false);
                    _view.PlayReloadAnimation(_mode.GetStepDuration(_settings, false));
                }
                else
                {
                    FinishReload();
                }
            }
            else if (_internalState == InternalState.Exiting)
            {
                _internalState = InternalState.Idle;
                _view.SetMagazineVisualActive(true);
                OnReloadCancelled?.Invoke();
            }
        }

        private void FinishReload()
        {
            _internalState = InternalState.Idle;
            _view.SetMagazineVisualActive(true);
            OnReloadFinished?.Invoke();
        }

        public void RequestCancel()
        {
            if (_internalState != InternalState.Stepping) return;

            float exitDuration = _mode.GetExitDuration();
            if (exitDuration <= 0f)
            {
                _internalState = InternalState.Idle;
                _view.SetMagazineVisualActive(true);
                OnReloadCancelled?.Invoke();
                return;
            }

            _internalState = InternalState.Exiting;
            _nextEventTime = Time.time + exitDuration;
            _view.PlayReloadExitAnimation(exitDuration);
        }

        public void Cancel()
        {
            if (_internalState == InternalState.Idle) return;
            _internalState = InternalState.Idle;
            _view.SetMagazineVisualActive(true);
            OnReloadCancelled?.Invoke();
        }
    }
}