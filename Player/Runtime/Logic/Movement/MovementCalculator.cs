using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    public class MovementCalculator
    {
        private readonly PlayerConfig _playerConfig;

        public Vector3 HorizontalVelocity { get; set; }
        public Vector3 VerticalVelocity { get; set; }
        public Vector3 SlideDirection { get; set; }
        public float SlideTimer { get; set; }

        // Множители скорости, которые применяются за счёт оружия
        private float _weaponSprintSpeedMultiplier = 1.0f;
        private float _weaponForwardSpeedMultiplier = 1.0f;
        private float _weaponStrafeSpeedMultiplier = 1.0f;
        private float _weaponBackwardSpeedMultiplier = 1.0f;

        public MovementCalculator(PlayerConfig playerConfig)
        {
            _playerConfig = playerConfig;
        }

        public void ExecuteJump(Vector3 viewForward, Vector3 viewRight, Vector3 wishDir, float jumpHeight, float gravity, float maxSpeed)
        {
            VerticalVelocity = new Vector3(0, Mathf.Sqrt(jumpHeight * -2f * gravity), 0);
            AirMove(viewForward, viewRight, wishDir, maxSpeed);
        }

        public void ApplyGravity(PlayerView view, float gravity)
        {
            Vector3 vertical = VerticalVelocity;
            if (view.IsGrounded && vertical.y < 0)
            {
                vertical.y = -2f;
            }

            vertical.y += gravity * Time.deltaTime;
            VerticalVelocity = vertical;
        }

        public void UpdateWeaponSpeedMultipliers(
            float sprintSpeedMult = 1.0f,
            float forwardSpeedMult = 1.0f,
            float strafeSpeedMult = 1.0f,
            float backwardSpeedMult = 1.0f)
        {
            _weaponSprintSpeedMultiplier = sprintSpeedMult;
            _weaponForwardSpeedMultiplier = forwardSpeedMult;
            _weaponStrafeSpeedMultiplier = strafeSpeedMult;
            _weaponBackwardSpeedMultiplier = backwardSpeedMult;
        }

        public Vector3 CalculateWishDir(PlayerView view, Vector2 input)
        {
            // Используем горизонтальные векторы ТЕЛА (yaw), а не глаз (которые наклоняются по pitch).
            // При взгляде строго вверх/вниз EyesForward становится вертикальным, и после
            // обнуления Y движение вперёд/назад пропадало полностью.
            Vector3 right = view.Right;
            Vector3 forward = view.Forward;

            Vector3 wishDir = right * input.x + forward * input.y;
            wishDir.y = 0f;

            if (wishDir.magnitude > 1f)
            {
                wishDir.Normalize();
            }

            return wishDir;
        }

        public void GroundMove(Vector3 viewForward, Vector3 viewRight, Vector3 wishDir, float maxSpeed)
        {
            // Определяем, это walk или sprint скорость, и применяем соответствующий множитель от оружия (только для спринта)
            float walkSpeedThreshold = _playerConfig.Movement.WalkSpeed + 0.1f;
            float weaponSpeedMultiplier = maxSpeed > walkSpeedThreshold ? _weaponSprintSpeedMultiplier : 1.0f;

            float adjustedMaxSpeed = maxSpeed * weaponSpeedMultiplier;
            adjustedMaxSpeed = ApplyDirectionSpeedMultiplier(viewForward, viewRight, wishDir, adjustedMaxSpeed);
            
            float wishSpeed = wishDir.magnitude * adjustedMaxSpeed;
            Accelerate(wishDir, wishSpeed, _playerConfig.Physics.GroundAccelerate);
        }

        public void AirMove(Vector3 viewForward, Vector3 viewRight, Vector3 wishDir, float maxSpeed)
        {
            // Определяем, это walk или sprint скорость, и применяем соответствующий множитель от оружия (только для спринта)
            float walkSpeedThreshold = _playerConfig.Movement.WalkSpeed + 0.1f;
            float weaponSpeedMultiplier = maxSpeed > walkSpeedThreshold ? _weaponSprintSpeedMultiplier : 1.0f;

            float adjustedMaxSpeed = maxSpeed * weaponSpeedMultiplier;
            adjustedMaxSpeed = ApplyDirectionSpeedMultiplier(viewForward, viewRight, wishDir, adjustedMaxSpeed);
            
            float wishSpeed = wishDir.magnitude * adjustedMaxSpeed;
            if (wishSpeed > _playerConfig.Physics.MaxAirWishSpeed) wishSpeed = _playerConfig.Physics.MaxAirWishSpeed;
            Accelerate(wishDir, wishSpeed, _playerConfig.Physics.AirAccelerate);
        }

        public void ApplyFriction()
        {
            Vector3 horizVel = HorizontalVelocity;
            float speed = horizVel.magnitude;
            if (speed < 0.001f)
            {
                HorizontalVelocity = Vector3.zero;
                return;
            }

            float control = speed < _playerConfig.Physics.StopSpeed ? _playerConfig.Physics.StopSpeed : speed;
            float drop = control * _playerConfig.Physics.Friction * Time.deltaTime;
            float newSpeed = speed - drop;

            if (newSpeed < 0f)
            {
                newSpeed = 0f;
            }

            newSpeed /= speed;
            HorizontalVelocity = horizVel * newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            Vector3 horizVel = HorizontalVelocity;
            float currentSpeed = Vector3.Dot(horizVel, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return;

            float accelSpeed = accel * wishSpeed * Time.deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            HorizontalVelocity = horizVel + (wishDir * accelSpeed);
        }

        private float ApplyDirectionSpeedMultiplier(Vector3 viewForward, Vector3 viewRight, Vector3 wishDir, float maxSpeed)
        {
            // Compute components relative to view forward/right using dot products
            Vector3 flatForward = viewForward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 flatRight = viewRight;
            flatRight.y = 0f;
            flatRight.Normalize();

            Vector3 dirNorm = wishDir;
            dirNorm.y = 0f;
            if (dirNorm.sqrMagnitude > 0.0001f) dirNorm.Normalize();

            float forwardDot = Vector3.Dot(dirNorm, flatForward);
            float rightDot = Vector3.Dot(dirNorm, flatRight);

            float forwardComponent = Mathf.Max(0f, forwardDot);
            float backwardComponent = Mathf.Max(0f, -forwardDot);
            float strafeComponent = Mathf.Abs(rightDot);

            float configMultiplier = 1.0f;
            if (forwardComponent > 0.1f)
                configMultiplier = _playerConfig.Movement.ForwardSpeedMultiplier;
            else if (backwardComponent > 0.1f)
                configMultiplier = _playerConfig.Movement.BackwardSpeedMultiplier;
            else if (strafeComponent > 0.1f)
                configMultiplier = _playerConfig.Movement.StrafeSpeedMultiplier;

            float resultMultiplier = configMultiplier;
            if (forwardComponent > 0.1f)
                resultMultiplier *= _weaponForwardSpeedMultiplier;
            else if (backwardComponent > 0.1f)
                resultMultiplier *= _weaponBackwardSpeedMultiplier;
            else if (strafeComponent > 0.1f)
                resultMultiplier *= _weaponStrafeSpeedMultiplier;

            return maxSpeed * resultMultiplier;
        }
    }
}