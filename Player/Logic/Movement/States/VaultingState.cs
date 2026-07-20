using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic.Movement.States
{
    public class VaultingState : IMovementState
    {
        private readonly VaultDetector _vaultDetector;
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _elapsed;
        private float _duration;

        public VaultingState(VaultDetector vaultDetector)
        {
            _vaultDetector = vaultDetector;
        }

        public void Enter(PlayerView view, PlayerConfig config)
        {
            var result = _vaultDetector.TryDetect(view, config);
            _startPosition = view.Position;
            _endPosition = result.Type == VaultType.Vault ? result.LandingPosition : view.Position;
            _duration = config.Vault.VaultDuration;
            _elapsed = 0f;

            view.PlayArmsAnimation("Vault");
        }

        public void Update(MovementSystem system, PlayerView view, PlayerConfig config, ref MovementState currentEnum)
        {
            if (_elapsed >= _duration)
            {
                currentEnum = view.IsGrounded ? MovementState.Walking : MovementState.Airborne;
            }
        }

        public void ProcessMovement(MovementSystem system, PlayerView view, PlayerConfig config)
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = Vector3.Lerp(_startPosition, _endPosition, eased);
            view.ForceSetPosition(pos);

            // Полностью берём контроль на себя — гасим скорость, чтобы автоматический Move() в Tick() не мешал
            system.HorizontalVelocity = Vector3.zero;
            system.VerticalVelocity = Vector3.zero;
        }

        public void Exit(PlayerView view) { }
    }
}