using UnityEngine;
using ZigdarkS.ProjectB.Core.Audio;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    /// <summary>
    /// Раз в StepDistance метров пройденного пути по земле поднимает SoundEvent(Footstep),
    /// на который реагирует слух врагов (EnemySoundPerception).
    /// </summary>
    public class FootstepEmitter
    {
        private const float StepDistance = 1.8f;
        private const float FootstepSoundRadius = 8f;
        private const float MinSpeedThreshold = 0.1f;

        private readonly ISoundEventBus _soundBus;

        private bool _hasOrigin;
        private Vector3 _lastFootstepPosition;

        public FootstepEmitter(ISoundEventBus soundBus)
        {
            _soundBus = soundBus;
        }

        public void Tick(Vector3 currentPosition, bool isGrounded, float horizontalSpeed)
        {
            if (!isGrounded || horizontalSpeed <= MinSpeedThreshold)
            {
                _hasOrigin = false;
                return;
            }

            if (!_hasOrigin)
            {
                _lastFootstepPosition = currentPosition;
                _hasOrigin = true;
                return;
            }

            float distanceSinceLastStep = Vector3.Distance(currentPosition, _lastFootstepPosition);
            if (distanceSinceLastStep >= StepDistance)
            {
                _soundBus.Raise(new SoundEvent(currentPosition, FootstepSoundRadius, SoundType.Footstep));
                _lastFootstepPosition = currentPosition;
            }
        }
    }
}