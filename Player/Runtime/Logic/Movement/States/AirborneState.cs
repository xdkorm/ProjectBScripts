using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class AirborneState : IMovementState
    {
        private const float CapsuleCenterDivider = 2f;
        private readonly IInputService _input;

        public AirborneState(IInputService input)
        {
            _input = input;
        }

        public void Enter(PlayerView view, PlayerConfig config)
        {
            view.SetHitboxSize(config.Hitbox.StandingHeight, config.Hitbox.StandingHeight / CapsuleCenterDivider);
            view.SetTargetEyeHeight(config.Hitbox.StandingHeight - config.Hitbox.CameraOffsetFromTop);
        }

        public void Update(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (view.IsGrounded)
            {
                currentEnum = _input.IsCrouching() ? MovementState.Crouching : MovementState.Walking;
                return;
            }
            if (_input.IsCrouching())
            {
                currentEnum = MovementState.AirborneCrouch;
            }
        }

        // Сигнатура теперь совпадает с интерфейсом
        public void ProcessMovement(MovementSystem system, PlayerView view, PlayerConfig config)
        {
            Vector3 wishDir = system.CalculateWishDir(view, _input.GetMovement());
            system.AirMove(view, wishDir, config.Movement.WalkSpeed);
        }

        public void Exit(PlayerView view) { }
    }
}