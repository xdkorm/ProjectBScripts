using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class SlidingState : IMovementState
    {
        private readonly IInputService _input;
        private bool _wasSlidingLastFrame;

        public SlidingState(IInputService input)
        {
            _input = input;
        }

        public void Enter(PlayerView playerView, PlayerConfig playerConfig)
        {
            playerView.SetHitboxSize(playerConfig.Hitbox.CrouchHeight, playerConfig.Hitbox.CrouchHeight / 2f);
            playerView.SetTargetEyeHeight(playerConfig.Hitbox.CrouchHeight - playerConfig.Hitbox.CameraOffsetFromTop);
            // Считаем кнопку "уже зажатой" — иначе первый кадр немедленно отменит слайд
            _wasSlidingLastFrame = true;
        }

        public void Update(MovementSystem system, PlayerView playerView, PlayerConfig playerConfig, ref MovementState movementState)
        {
            system.SlideTimer -= Time.deltaTime;

            if (!playerView.IsGrounded)
            {
                movementState = MovementState.Airborne;
                return;
            }

            // Rising edge кнопки слайда во время активного слайда = отмена
            bool isSlidingNow = _input.IsSliding();
            bool cancelledByPlayer = isSlidingNow && !_wasSlidingLastFrame;
            _wasSlidingLastFrame = isSlidingNow;

            if (cancelledByPlayer)
            {
                ApplyCancelPenalties(system, playerConfig);
                movementState = _input.IsCrouching() ? MovementState.Crouching : MovementState.Walking;
                return;
            }

            // Естественное завершение — без штрафов
            if (system.SlideTimer <= 0 || system.HorizontalVelocity.magnitude < playerConfig.Movement.WalkSpeed)
            {
                system.SlideFallSpeedBonus = 0f;
                movementState = _input.IsCrouching() ? MovementState.Crouching : MovementState.Walking;
            }
        }

        private void ApplyCancelPenalties(MovementSystem system, PlayerConfig config)
        {
            system.WalkPenaltyStartTime = Time.time;

            system.SlideFallSpeedBonus = 0f;
            system.SprintLockedUntil = Time.time + config.SlideCancel.SprintLockDuration;
            system.WalkPenaltyUntil = Time.time + config.SlideCancel.WalkPenaltyDuration;
            system.WalkPenaltyMultiplier = config.SlideCancel.WalkSpeedMultiplier;
        }

        public void ProcessMovement(MovementSystem system, PlayerView playerView, PlayerConfig playerConfig)
        {
            float speedFactor = system.SlideTimer / playerConfig.Slide.MaxSlideTime;
            // SlideFallSpeedBonus тоже затухает вместе со speedFactor — чем дольше едем, тем меньше бонус
            float currentSlideSpeed = (playerConfig.Movement.SprintSpeed * playerConfig.Slide.SlideSpeedBoost + system.SlideFallSpeedBonus) * speedFactor;
            float targetMagnitude = Mathf.Max(currentSlideSpeed, playerConfig.Movement.WalkSpeed);

            Vector3 currentDir = system.SlideDirection.normalized;
            float steerInput = Mathf.Clamp(_input.GetMovement().x, -1f, 1f);

            Vector3 desiredDir = Mathf.Abs(steerInput) > 0.01f
                ? Quaternion.AngleAxis(steerInput * playerConfig.Slide.SlideSteerAngle, Vector3.up) * playerView.Forward
                : currentDir;

            Vector3 steeredDir = Vector3.RotateTowards(
                currentDir,
                desiredDir,
                playerConfig.Slide.SlideSteerDegreesPerSecond * Mathf.Deg2Rad * Time.deltaTime,
                0f);

            system.SlideDirection = steeredDir;
            system.HorizontalVelocity = steeredDir * targetMagnitude;
        }

        public void Exit(PlayerView playerView) { }
    }
}