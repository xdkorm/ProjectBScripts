using System;
using UnityEngine;
using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Weapon.Logic;
using ZigdarkS.ProjectB.Player.Logic.Movement;

namespace ZigdarkS.ProjectB.Player.Logic
{
    /// <summary>
    /// Отвечает только за принятие решения "стрелять / не стрелять" в этом кадре
    /// и вызов TryFire. Переключение оружия, режимов, ADS, reload и ручной цикл
    /// вынесены в отдельные ITickable-системы.
    /// </summary>
    public class PlayerShootingSystem : ITickable
    {
        private readonly IActivePlayerView _activeView;
        private readonly IInputService _inputService;
        private readonly IWeaponInventory _inventory;
        private readonly MovementSystem _movementSystem;
        private float _sprintEndTime = float.NegativeInfinity;
        private bool _wasAttackPressedLastFrame;
        private bool _wasSprintingLastFrame;
        private bool _isAttackPending;
        public event Action OnWeaponFired;

        public PlayerShootingSystem(
            IActivePlayerView activeView,
            IInputService inputService,
            IWeaponInventory inventory,
            MovementSystem movementSystem)
        {
            _activeView = activeView;
            _inputService = inputService;
            _inventory = inventory;
            _movementSystem = movementSystem;
        }

        public void Tick()
        {
            var view = _activeView.Current;
            if (view == null) return;

            var activeWeapon = _inventory.ReadyWeapon;
            if (activeWeapon == null) return;

            UpdateSprintFireGate();

            bool isAttackingNow = _inputService.IsAttacking();
            bool isRisingEdge = isAttackingNow && !_wasAttackPressedLastFrame;
            _wasAttackPressedLastFrame = isAttackingNow;

            if (!isAttackingNow)
            {
                _isAttackPending = false;
                return;
            }

            bool isBlocked = IsBlockedBySprint(activeWeapon);
            bool shouldFire;

            if (activeWeapon.CurrentFireMode.IsAutomatic)
            {
                shouldFire = !isBlocked;
            }
            else
            {
                if (isRisingEdge)
                {
                    _isAttackPending = true;
                }
                shouldFire = _isAttackPending && !isBlocked;
                if (shouldFire)
                {
                    _isAttackPending = false;
                }
            }

            if (!shouldFire) return;

            bool didFire = activeWeapon.TryFire(
                () => view.EyesPosition,
                () => view.EyesForward,
                () => _inputService.IsAttacking());

            if (didFire)
            {
                view.PlayArmsShootAnimation();
                OnWeaponFired?.Invoke();
            }
        }

        private void UpdateSprintFireGate()
        {
            bool isSprintingNow = _movementSystem.IsSprinting;
            if (_wasSprintingLastFrame && !isSprintingNow)
            {
                _sprintEndTime = Time.time;
            }
            _wasSprintingLastFrame = isSprintingNow;
        }

        private bool IsBlockedBySprint(IWeapon activeWeapon)
        {
            return Time.time - _sprintEndTime < activeWeapon.SprintToFireDelay;
        }
    }
}