using UnityEngine;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    /// <summary>
    /// Отслеживает переходы grounded/airborne и считает дистанцию последнего падения.
    /// LastFallDistance используется, например, SprintingState для бонуса скорости слайда
    /// после приземления, и сбрасывается автоматически спустя ResetDelay после посадки.
    /// </summary>
    public class FallDistanceTracker
    {
        private const float ResetDelay = 0.2f;

        private float _airborneStartY;
        private bool _wasGrounded;
        private float _landingTime = float.NegativeInfinity;

        public float LastFallDistance { get; set; }

        public void Tick(Vector3 currentPosition, bool isGrounded)
        {
            if (!_wasGrounded && isGrounded)
            {
                float fallen = _airborneStartY - currentPosition.y;
                LastFallDistance = Mathf.Max(0f, fallen);
                _landingTime = Time.time;
            }

            if (_wasGrounded && !isGrounded)
            {
                _airborneStartY = currentPosition.y;
            }

            if (isGrounded && Time.time - _landingTime > ResetDelay)
            {
                LastFallDistance = 0f;
            }

            _wasGrounded = isGrounded;
        }
    }
}