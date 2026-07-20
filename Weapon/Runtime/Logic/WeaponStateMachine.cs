using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;
using ZigdarkS.ProjectB.Weapon.View;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public enum WeaponState
    {
        Holstered,
        Drawing,
        Idle,
        Reloading,
        CyclingAction,
        PendingCycle
    }

    /// <summary>
    /// Единая точка координации между AmmoState, ReloadController и ActionCycleController.
    /// Заменяет разрозненные bool-флаги одним явным состоянием + таблицей переходов.
    /// </summary>
    public class WeaponStateMachine
    {
        private readonly AmmoState _ammoState;
        private readonly ReloadController _reloadController;
        private readonly ActionCycleController _actionCycleController;
        private readonly AmmoSettings _ammoSettings;
        private readonly Func<ActionCycleSettings> _getActionCycleSettings;

        private WeaponState _state = WeaponState.Holstered;
        private bool _autoReloadPending;
        private float _autoReloadTime;

        public WeaponState Current => _state;

        /// <summary>Разрешает фактическое исполнение выстрела (FireModeController.ExecuteFire).
        /// Не включает safety/timing — это отдельная забота WeaponInstance.CanFire.</summary>
        public bool CanFire => _state == WeaponState.Idle && _ammoState.HasAmmoToFire;

        public bool CanSwitchFireMode => _state == WeaponState.Idle;
        public bool CanStartReload => (_state == WeaponState.Idle || _state == WeaponState.PendingCycle)
                                       && _ammoState.CanReload;

        public event Action OnReloadStarted;
        public event Action OnReloadFinished;
        public event Action OnReloadCancelled;

        public WeaponStateMachine(
            AmmoState ammoState,
            ReloadController reloadController,
            ActionCycleController actionCycleController,
            AmmoSettings ammoSettings,
            Func<ActionCycleSettings> getActionCycleSettings)
        {
            _ammoState = ammoState;
            _reloadController = reloadController;
            _actionCycleController = actionCycleController;
            _ammoSettings = ammoSettings;
            _getActionCycleSettings = getActionCycleSettings;

            _reloadController.OnReloadStarted += HandleReloadStarted;
            _reloadController.OnReloadFinished += HandleReloadFinished;
            _reloadController.OnReloadCancelled += HandleReloadCancelled;

            _ammoState.OnAmmoChanged += HandleAmmoChanged;
        }

        public void Dispose()
        {
            _reloadController.OnReloadStarted -= HandleReloadStarted;
            _reloadController.OnReloadFinished -= HandleReloadFinished;
            _reloadController.OnReloadCancelled -= HandleReloadCancelled;
            _ammoState.OnAmmoChanged -= HandleAmmoChanged;
        }

        // ===================== Запросы извне (WeaponInstance) =====================

        /// <summary>
        /// Вызывается при попытке выстрелить. Возвращает true, если WeaponInstance может
        /// идти дальше к фактическому исполнению (через FireModeController). Сама разруливает
        /// побочные эффекты (мягкая отмена reload, передёргивание вместо выстрела и т.д.)
        /// </summary>
        public bool HandleFireAttempt()
        {
            switch (_state)
            {
                case WeaponState.Idle:
                    return true; // дальше решает WeaponInstance.CanFire (safety/timing) и стреляет

                case WeaponState.Reloading:
                    if (_ammoState.HasAmmoToFire)
                        _reloadController.RequestCancel(); // патрон есть — прерываем поштучную зарядку
                    // патронов нет — стрелять нечем, попытку игнорируем, зарядка продолжается
                    return false;

                case WeaponState.PendingCycle:
                    // Первый клик после Draw/Reload с недосланным патроном — трактуем как передёргивание
                    if (_actionCycleController.RequiresManualCycle)
                    {
                        _actionCycleController.StartCycle();
                        _state = WeaponState.CyclingAction;
                    }
                    return false;

                case WeaponState.CyclingAction:
                    // Повторный клик во время ручного цикла — тоже передёргивание, не выстрел
                    if (_actionCycleController.RequiresManualCycle)
                        _actionCycleController.TryManualCycle();
                    return false;

                default: // Holstered, Drawing
                    return false;
            }
        }

        public bool RequestReload()
        {
            if (!CanStartReload) return false;
            return _reloadController.TryStartReload();
        }

        public bool RequestManualCycle()
        {
            if (_state != WeaponState.CyclingAction && _state != WeaponState.PendingCycle) return false;

            if (_state == WeaponState.PendingCycle)
            {
                _actionCycleController.StartCycle();
                _state = WeaponState.CyclingAction;
                return true;
            }

            return _actionCycleController.TryManualCycle();
        }

        /// <summary>Вызывается сразу после фактического выстрела (ExecuteSingleShot -> ConsumeBullet).</summary>
        public void NotifyShotFired()
        {
            var cycleSettings = _getActionCycleSettings();
            if (cycleSettings.CycleMode == ActionCycleMode.None)
            {
                _state = WeaponState.Idle;
                CheckAutoReload();
                return;
            }

            _actionCycleController.StartCycle();
            _state = WeaponState.CyclingAction;
        }

        public void NotifyHolster()
        {
            if (_state == WeaponState.CyclingAction)
                _actionCycleController.CancelHard(); // патрон в патронник НЕ переносится — цикл прерван физически

            if (_reloadController.IsReloading)
                _reloadController.Cancel();

            _autoReloadPending = false;
            _state = WeaponState.Holstered;
        }

        public void NotifyDraw()
        {
            if (_ammoState.HasAmmoToFire)
            {
                _state = WeaponState.Idle;
                CheckAutoReload();
                return;
            }

            if (!_ammoState.UsesChamberSlot)
            {
                // Нет концепции патронника — просто "патронов нет", ждём автоперезарядку
                _state = WeaponState.Idle;
                CheckAutoReload();
                return;
            }

            // Патрон не дослан. Если для текущего режима цикл автоматический —
            // затвор досылает патрон сам сразу при доставании.
            var cycleSettings = _getActionCycleSettings();
            if (cycleSettings.CycleMode == ActionCycleMode.Automatic)
            {
                _actionCycleController.StartCycle();
                _state = WeaponState.CyclingAction;
            }
            else
            {
                _state = WeaponState.PendingCycle;
            }
        }

        public void Tick()
        {
            if (_state == WeaponState.Holstered || _state == WeaponState.Drawing) return;

            _reloadController.Tick();
            _actionCycleController.Tick();

            if (_state == WeaponState.CyclingAction && !_actionCycleController.IsCycling)
            {
                // Цикл завершился (авто или ручной) — либо готовы, либо труба тоже пуста
                _state = WeaponState.Idle;
                CheckAutoReload();
            }

            if (_autoReloadPending && _state == WeaponState.Idle && Time.time >= _autoReloadTime)
            {
                _autoReloadPending = false;
                RequestReload();
            }
        }

        // ===================== Обработчики событий =====================

        private void HandleReloadStarted()
        {
            _state = WeaponState.Reloading;
            OnReloadStarted?.Invoke();
        }

        private void HandleReloadFinished()
        {
            OnReloadFinished?.Invoke();

            if (_ammoState.HasAmmoToFire)
            {
                _state = WeaponState.Idle;
                return;
            }

            // Труба дозаряжена, но патронник пуст (ChamberAfterTube policy) — нужен цикл
            if (_ammoState.UsesChamberSlot && _ammoState.InTube > 0)
            {
                var cycleSettings = _getActionCycleSettings();
                if (cycleSettings.CycleMode == ActionCycleMode.Automatic)
                {
                    _actionCycleController.StartCycle();
                    _state = WeaponState.CyclingAction;
                }
                else
                {
                    _state = WeaponState.PendingCycle;
                }
                return;
            }

            _state = WeaponState.Idle;
            CheckAutoReload();
        }

        private void HandleReloadCancelled()
        {
            OnReloadCancelled?.Invoke();
            _state = WeaponState.Idle;
            CheckAutoReload();
        }

        private void HandleAmmoChanged()
        {
            if (_state == WeaponState.Idle)
                CheckAutoReload();
        }

        private void CheckAutoReload()
        {
            if (!_ammoState.HasAmmoToFire && !_autoReloadPending && _ammoState.CanReload)
            {
                _autoReloadPending = true;
                _autoReloadTime = Time.time + _ammoSettings.AutoReloadDelay;
            }
        }
    }
}