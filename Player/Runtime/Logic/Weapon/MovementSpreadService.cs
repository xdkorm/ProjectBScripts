// Player.asmdef — зависит от Weapon (нужен IWeaponInventory и IMovementSpreadProvider)
using VContainer.Unity;
using UnityEngine;
using ZigdarkS.ProjectB.Player.Logic.Movement;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Weapon.Logic; // тут лежит IMovementSpreadProvider

namespace ZigdarkS.ProjectB.Player.Logic.Weapon
{
    public class MovementSpreadService : ITickable, IMovementSpreadProvider
    {
        private readonly MovementSystem _movementSystem;
        private readonly IWeaponInventory _inventory;
        private const float SmoothSpeed = 20f;
        private float _smoothMovementOffset;
        private float _currentAdsProgress;

        public float CurrentSpreadOffset => _smoothMovementOffset;

        public MovementSpreadService(MovementSystem movementSystem, IWeaponInventory inventory)
        {
            _movementSystem = movementSystem;
            _inventory = inventory;
        }

        public void Tick()
        {
            var activeWeapon = _inventory.ActiveWeapon;
            if (activeWeapon == null)
            {
                _smoothMovementOffset = Mathf.Lerp(_smoothMovementOffset, 0f, Time.deltaTime * SmoothSpeed);
                return;
            }

            Vector3 horizontalVelocity = _movementSystem.HorizontalVelocity;
            float speed = new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude;
            bool isAirborne = !_movementSystem.IsGrounded;
            bool isCrouching = _movementSystem.IsCrouching;

            float moveSpeedMultiplier = Mathf.Lerp(
                activeWeapon.MoveSpreadMultiplier,
                activeWeapon.AdsMoveSpreadMultiplier,
                _currentAdsProgress);
            float crouchSpeedMultiplier = Mathf.Lerp(
                activeWeapon.CrouchMoveSpreadMultiplier,
                activeWeapon.AdsCrouchMoveSpreadMultiplier,
                _currentAdsProgress);
            float airbornePenalty = Mathf.Lerp(
                activeWeapon.AirborneSpreadPenalty,
                activeWeapon.AdsAirborneSpreadPenalty,
                _currentAdsProgress);

            float currentSpeedMultiplier = isCrouching ? crouchSpeedMultiplier : moveSpeedMultiplier;
            float targetMovementOffset = (speed * currentSpeedMultiplier) + (isAirborne ? airbornePenalty : 0f);
            _smoothMovementOffset = Mathf.Lerp(_smoothMovementOffset, targetMovementOffset, Time.deltaTime * SmoothSpeed);
        }

        public void SetAdsProgress(float adsProgress)
        {
            _currentAdsProgress = Mathf.Clamp01(adsProgress);
        }
    }
}