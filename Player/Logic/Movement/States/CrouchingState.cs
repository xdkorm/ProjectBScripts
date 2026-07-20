using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class CrouchingState : GroundedState
    {
        public CrouchingState(IInputService input, IWeaponInventory weaponInventory, VaultDetector vaultDetector) 
            : base(input, weaponInventory, vaultDetector)   
        {
        }

        private const float CapsuleCenterDivider = 2f;

        public override void Enter(PlayerView view, PlayerConfig config)
        {
            view.SetHitboxSize(config.Hitbox.CrouchHeight, config.Hitbox.CrouchHeight / 2f);
            view.SetTargetEyeHeight(config.Hitbox.CrouchHeight - config.Hitbox.CameraOffsetFromTop);
        }

        protected override float GetMaxSpeed(PlayerConfig config) => config.Movement.WalkSpeed * config.Hitbox.CrouchSpeedMultiplier;

        protected override void CheckTransitions(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (!_input.IsCrouching() && !IsCeilingAbove(view, config))
            {   
                currentEnum = _input.GetMovement() == Vector2.zero ? MovementState.Idle : MovementState.Walking;
            }
        }

        private bool IsCeilingAbove(PlayerView view, PlayerConfig config)
        {
            float radius = view.ControllerRadius;
            Vector3 origin = view.Position + Vector3.up * (config.Hitbox.CrouchHeight - radius);
            float checkDistance = config.Hitbox.StandingHeight - config.Hitbox.CrouchHeight + radius;
            return Physics.SphereCast(origin, radius, Vector3.up, out _, checkDistance);
        }
    }
}