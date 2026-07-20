using UnityEngine;
using ZigdarkS.ProjectB.Player.Logic.Movement.States;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class AirborneCrouchState : IMovementState
    {
        private const float CapsuleCenterDivider = 2f;
        private readonly IInputService _input;

        public AirborneCrouchState(IInputService input)
        {
            _input = input;
        }

        public void Enter(PlayerView view, PlayerConfig config)
        {
            view.SetHitboxSize(config.Hitbox.CrouchHeight, config.Hitbox.CrouchHeight / CapsuleCenterDivider);
            view.SetTargetEyeHeight(config.Hitbox.CrouchHeight - config.Hitbox.CameraOffsetFromTop);
        }

        public void Update(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (view.IsGrounded)
            {
                currentEnum = _input.IsCrouching() ? MovementState.Crouching : MovementState.Walking;
                return;
            }

            if (!_input.IsCrouching() && !IsCeilingAbove(view, config))
            {
                currentEnum = MovementState.Airborne;
            }
        }

        public void ProcessMovement(MovementSystem system, PlayerView view, PlayerConfig config)
        {
            Vector3 wishDir = system.CalculateWishDir(view, _input.GetMovement());
            system.AirMove(view, wishDir, config.Movement.WalkSpeed);
        }

        public void Exit(PlayerView view) { }

        private bool IsCeilingAbove(PlayerView view, PlayerConfig config)
        {
            float radius = view.ControllerRadius;
            Vector3 origin = view.Position + Vector3.up * (config.Hitbox.CrouchHeight - radius);
            float checkDistance = config.Hitbox.StandingHeight - config.Hitbox.CrouchHeight + radius;
            return Physics.SphereCast(origin, radius, Vector3.up, out _, checkDistance);
        }
    }
}