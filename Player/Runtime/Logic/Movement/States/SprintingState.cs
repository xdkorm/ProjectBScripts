using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class SprintingState : GroundedState
    {
        public SprintingState(IInputService input, IWeaponInventory weaponInventory, VaultDetector vaultDetector) 
            : base(input, weaponInventory, vaultDetector)   
        {
        }

        protected override float GetMaxSpeed(PlayerConfig config) => config.Movement.SprintSpeed;

        // ВОТ ЗДЕСЬ ПРОИСХОДИТ КОРРЕКЦИЯ УГЛА БЕГА
        protected override Vector2 ModifyInputDirection(Vector2 moveInput, PlayerConfig config)
        {
            if (moveInput == Vector2.zero) return moveInput;

            // Считаем текущий угол ввода относительно чистого "Вперед" (0, 1)
            float currentAngle = Vector2.Angle(Vector2.up, moveInput);

            // Если игрок зажал W+D (45°), а в конфиге лимит 35°
            if (currentAngle > config.Movement.MaxSprintAngle)
            {
                float sign = Mathf.Sign(moveInput.x); // +1 если жмет D, -1 если жмет A
                float rad = config.Movement.MaxSprintAngle * Mathf.Deg2Rad;

                // Высчитываем новые координаты вектора, прижатые ближе к оси Y (Вперед)
                float newX = Mathf.Sin(rad) * sign;
                float newY = Mathf.Cos(rad);

                // Возвращаем скорректированный вектор, сохраняя его изначальную длину
                return new Vector2(newX, newY).normalized * moveInput.magnitude;
            }

            return moveInput;
        }

        protected override void CheckTransitions(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (_input.IsSliding())
            {
                float dist = system.LastFallDistance;
                float minDist = config.FallSlideBonus.MinDistanceForBonus;
                float maxDist = config.FallSlideBonus.DistanceForMaxBonus;

                // Дедзона: если не набрали минимальную высоту — бонуса нет совсем
                float fallFactor = dist < minDist
                    ? 0f
                    : Mathf.Clamp01((dist - minDist) / (maxDist - minDist));

                system.SlideTimer = config.Slide.MaxSlideTime + config.FallSlideBonus.MaxTimeBonus * fallFactor;
                system.SlideFallSpeedBonus = config.FallSlideBonus.MaxSpeedBonus * fallFactor;
                system.LastFallDistance = 0f;
                system.SlideDirection = view.Forward;
                currentEnum = MovementState.Sliding;
                return;
            }
            if (_input.IsCrouching())
            {
                currentEnum = MovementState.Crouching;
                return;
            }
            if (_input.GetMovement() == Vector2.zero)
            {
                currentEnum = MovementState.Idle;
                return;
            }
            if (!IsSprintingAllowed(_input, system))
            {
                currentEnum = MovementState.Walking;
            }
        }
    }
}