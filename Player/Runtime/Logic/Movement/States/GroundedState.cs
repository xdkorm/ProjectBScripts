using UnityEngine;
using ZigdarkS.ProjectB.Player.Logic.Movement.States;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public abstract class GroundedState : IMovementState
    {
        protected readonly IInputService _input;
        protected readonly IWeaponInventory _weaponInventory;
        private readonly VaultDetector _vaultDetector;

        protected GroundedState(IInputService input, IWeaponInventory weaponInventory, VaultDetector vaultDetector)
        {
            _input = input;
            _weaponInventory = weaponInventory;
            _vaultDetector = vaultDetector;
        }

        public virtual void Enter(PlayerView view, PlayerConfig config)
        {
            view.SetHitboxSize(config.Hitbox.StandingHeight, config.Hitbox.StandingHeight / 2f);
            view.SetTargetEyeHeight(config.Hitbox.StandingHeight - config.Hitbox.CameraOffsetFromTop);
        }

        public void Update(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (!view.IsGrounded)
            {
                currentEnum = MovementState.Airborne;
                return;
            }

            if (_input.IsVaulting())
            {
                var result = _vaultDetector.TryDetect(view, config);
                if (result.Type == VaultType.Vault)
                {
                    currentEnum = MovementState.Vaulting;
                    return;
                }
                if (result.Type == VaultType.Mantle)
                {
                    currentEnum = MovementState.Mantling;
                    return;
                }
            }

            CheckTransitions(system, view, config, ref currentEnum);
        }

        public void ProcessMovement(MovementSystem system, PlayerView view, PlayerConfig config)
        {
            Vector2 moveInput = _input.GetMovement();
            moveInput = ModifyInputDirection(moveInput, config);
            Vector3 wishDir = system.CalculateWishDir(view, moveInput);
            float maxSpeed = GetMaxSpeed(config) * GetWalkPenaltyModifier(system);
            if (_input.IsJumping())
            {
                system.ExecuteJump(view, wishDir, maxSpeed);
            }
            else
            {
                system.ApplyFriction();
                system.GroundMove(view, wishDir, maxSpeed);
            }
        }

        protected virtual float GetWalkPenaltyModifier(MovementSystem system) => 1f;

        protected virtual Vector2 ModifyInputDirection(Vector2 moveInput, PlayerConfig config)
        {
            return moveInput;
        }

        protected bool IsSprintingAllowed(IInputService input, MovementSystem system)
        {
            /// <summary>
            /// Спринт разрешён, если зажата клавиша спринта, есть движение вперёд,
            /// и при этом не зажаты ADS или атака (ADS/стрельба всегда приоритетнее бега).
            /// Также запрещён во время перезарядки — чтобы начать бежать, нужно сменить оружие,
            /// что автоматически отменяет reload.
            /// </summary>
            if (!input.IsSprinting()) return false;
            if (input.GetMovement().y <= 0f) return false;
            if (input.IsAttacking()) return false;
            if (IsAdsActiveOrTransitioning()) return false;
            if (IsReloading()) return false;
            if (Time.time < system.SprintLockedUntil) return false;
            return true;
        }

        private bool IsReloading()
        {
            var activeWeapon = _weaponInventory?.ActiveWeapon;
            return activeWeapon != null && activeWeapon.IsReloading;
        }

        private bool IsAdsActiveOrTransitioning()
        {
            var activeWeapon = _weaponInventory?.ActiveWeapon;
            if (activeWeapon == null) return false;

            // Используем тот же эпсилон, что и в PlayerShootingSystem.HandleADS (isAiming = progress > 0.001f),
            // чтобы границы совпадали и не было рассинхрона на стыке состояний.
            return activeWeapon.AdsProgress > 0.001f;
        }

        public virtual void Exit(PlayerView view) { }

        protected abstract void CheckTransitions(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum);
        protected abstract float GetMaxSpeed(PlayerConfig config);
    }
}