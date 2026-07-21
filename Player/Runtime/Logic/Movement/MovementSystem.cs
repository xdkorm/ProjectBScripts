using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Player.Logic.Movement;
using ZigdarkS.ProjectB.Player.Logic.Movement.States;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Core.Audio;
using ZigdarkS.ProjectB.Weapon.Logic;
using ZigdarkS.ProjectB.Player.Logic;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    public class MovementSystem : ITickable, IDisposable
    {
        public bool IsGrounded => _cachedView != null && _cachedView.IsGrounded;
        public bool IsSprinting => _stateMachine != null && _stateMachine.CurrentStateEnum == MovementState.Sprinting;
        public bool IsCrouching => _stateMachine != null && 
            (_stateMachine.CurrentStateEnum == MovementState.Crouching || 
             _stateMachine.CurrentStateEnum == MovementState.AirborneCrouch || 
             _stateMachine.CurrentStateEnum == MovementState.Sliding);

        private readonly PlayerConfig _playerConfig;
        private readonly IInputService _inputService;
        private readonly InstanceProvider<PlayerView> _playerProvider;
        private readonly MovementCalculator _movementCalculator;
        private readonly MouseLookController _mouseLookController;
        private readonly MovementStateMachine _stateMachine;
        private readonly IWeaponInventory _weaponInventory;
        private readonly ISoundEventBus _soundBus;
        private readonly PlayerSettings _playerSettings;
        private readonly IFovCalculator _fovCalculator;

        // Блокировка спринта после отмены слайда
        public float SprintLockedUntil { get; set; } = float.NegativeInfinity;
        // Временный штраф к скорости ходьбы после отмены слайда
        public float WalkPenaltyUntil { get; set; } = float.NegativeInfinity;
        public float WalkPenaltyMultiplier { get; set; } = 1f;
        public float WalkPenaltyStartTime { get; set; } = float.NegativeInfinity;
        // Скорость вертикального падения в момент приземления — читается при входе в слайд
        public float LastFallDistance { get; set; } = 0f;
        // Бонус скорости слайда, рассчитанный при старте слайда из LastFallSpeed
        public float SlideFallSpeedBonus { get; set; } = 0f;
        private float _airborneStartY;
        private bool _wasGrounded;
        private float _landingTime = float.NegativeInfinity;
        private bool _hasFootstepOrigin;
        private Vector3 _lastFootstepPosition;

        private PlayerView _cachedView;

        public Vector3 HorizontalVelocity
        {
            get => _movementCalculator.HorizontalVelocity;
            set => _movementCalculator.HorizontalVelocity = value;
        }

        public Vector3 VerticalVelocity
        {
            get => _movementCalculator.VerticalVelocity;
            set => _movementCalculator.VerticalVelocity = value;
        }

        public Vector3 SlideDirection
        {
            get => _movementCalculator.SlideDirection;
            set => _movementCalculator.SlideDirection = value;
        }

        public float SlideTimer
        {
            get => _movementCalculator.SlideTimer;
            set => _movementCalculator.SlideTimer = value;
        }

        public MovementSystem(
            PlayerConfig playerConfig,
            IInputService inputService,
            InstanceProvider<PlayerView> playerProvider,
            MovementCalculator movementCalculator,
            MouseLookController mouseLookController,
            IWeaponInventory weaponInventory,
            MovementStateFactory stateFactory,
            ISoundEventBus soundBus,
            PlayerSettings playerSettings,
            IFovCalculator fovCalculator)
        {
            _playerConfig = playerConfig;
            _inputService = inputService;
            _playerProvider = playerProvider;
            _movementCalculator = movementCalculator;
            _mouseLookController = mouseLookController;
            _weaponInventory = weaponInventory;
            _soundBus = soundBus;
            _playerSettings = playerSettings;
            _fovCalculator = fovCalculator;

            _stateMachine = new MovementStateMachine(new Dictionary<MovementState, IMovementState>
            {
                { MovementState.Idle, stateFactory.Create(MovementState.Idle) },
                { MovementState.Walking, stateFactory.Create(MovementState.Walking) },
                { MovementState.Sprinting, stateFactory.Create(MovementState.Sprinting) },
                { MovementState.Airborne, stateFactory.Create(MovementState.Airborne) },
                { MovementState.Crouching, stateFactory.Create(MovementState.Crouching) },
                { MovementState.Sliding, stateFactory.Create(MovementState.Sliding) },
                { MovementState.AirborneCrouch, stateFactory.Create(MovementState.AirborneCrouch) },
                { MovementState.Vaulting, stateFactory.Create(MovementState.Vaulting) },
                { MovementState.Mantling, stateFactory.Create(MovementState.Mantling) },
            });

            _playerProvider.OnSpawned += HandlePlayerSpawned;
            _playerProvider.OnDespawned += HandlePlayerDespawned;

            if (_playerProvider.Instance != null)
            {
                HandlePlayerSpawned(_playerProvider.Instance);
            }
        }

        private void HandlePlayerSpawned(PlayerView view)
        {
            _cachedView = view;
            _cachedView.Initialize(_playerConfig.Physics.CameraSmoothTime);
            _stateMachine.Initialize(_cachedView, _playerConfig);
        }

        private void HandlePlayerDespawned()
        {
            _cachedView = null;
        }

        public void Tick()
        {
            //Debug.Log($"[Movement] Frame {Time.frameCount}, State={_stateMachine.CurrentStateEnum}");
            if (_cachedView == null) return;

            _inputService.UpdateInput();
            UpdateWeaponSpeedMultipliers();
            
            float currentFov = GetCurrentAdsFov();
            _mouseLookController.HandleMouseLook(_cachedView, _inputService, currentFov);

            bool isGrounded = _cachedView.IsGrounded;

            if (!_wasGrounded && isGrounded)
            {
                // Приземлились — фиксируем дистанцию падения
                float fallen = _airborneStartY - _cachedView.Position.y;
                LastFallDistance = Mathf.Max(0f, fallen);
                _landingTime = Time.time;
            }

            if (_wasGrounded && !isGrounded)
            {
                // Только что оторвались от земли — запоминаем стартовую высоту
                _airborneStartY = _cachedView.Position.y;
            }

            // Сбрасываем через 0.2с после приземления
            if (isGrounded && Time.time - _landingTime > 0.2f)
            {
                LastFallDistance = 0f;
            }

            _wasGrounded = isGrounded;

            _stateMachine.UpdateState(this, _cachedView, _playerConfig);

            _stateMachine.CurrentStateLogic.ProcessMovement(this, _cachedView, _playerConfig);
            _movementCalculator.ApplyGravity(_cachedView, _playerConfig.Movement.Gravity);

            Vector3 horizontalMovement = _movementCalculator.HorizontalVelocity * Time.deltaTime;
            Vector3 verticalMovement = _movementCalculator.VerticalVelocity * Time.deltaTime;
            
            _cachedView.Move(horizontalMovement + verticalMovement);

            Vector3 realVelocity = _cachedView.Velocity;
            
            HorizontalVelocity = new Vector3(realVelocity.x, 0f, realVelocity.z);

            if (isGrounded && HorizontalVelocity.magnitude > 0.1f)
            {
                if (!_hasFootstepOrigin)
                {
                    _lastFootstepPosition = _cachedView.Position;
                    _hasFootstepOrigin = true;
                }
                else
                {
                    float distanceSinceLastStep = Vector3.Distance(_cachedView.Position, _lastFootstepPosition);
                    if (distanceSinceLastStep >= 1.8f)
                    {
                        _soundBus.Raise(new SoundEvent(_cachedView.Position, 8f, SoundType.Footstep));
                        _lastFootstepPosition = _cachedView.Position;
                    }
                }
            }
            else
            {
                _hasFootstepOrigin = false;
            }
        }

        private float GetCurrentAdsFov()
        {
            return _fovCalculator.Calculate(_weaponInventory.ActiveWeapon, _playerSettings.Fov);
        }

        public void Dispose()
        {
            _playerProvider.OnSpawned -= HandlePlayerSpawned;
            _playerProvider.OnDespawned -= HandlePlayerDespawned;
        }

        public void ExecuteJump(PlayerView view, Vector3 wishDir, float maxSpeed)
        {
            _movementCalculator.ExecuteJump(view.Forward, view.Right, wishDir, _playerConfig.Movement.JumpHeight, _playerConfig.Movement.Gravity, maxSpeed);
        }

        // Спринт и ADS теперь взаимоисключающие на уровне стейт-машины, поэтому
        // здесь больше не нужно учитывать adsProgress при расчёте sprintMultiplier —
        // пока активен Sprinting, AdsProgress всегда равен 0.
        private void UpdateWeaponSpeedMultipliers()
        {
            var activeWeapon = _weaponInventory.ActiveWeapon;

            if (activeWeapon == null)
            {
                _movementCalculator.UpdateWeaponSpeedMultipliers(1.0f, 1.0f, 1.0f, 1.0f);
                return;
            }

            float adsProgress = activeWeapon.AdsProgress;

            float hipSprint = activeWeapon.HipSprintSpeedMultiplier;
            float hipForward = activeWeapon.HipForwardSpeedMultiplier;
            float aimForward = activeWeapon.AimForwardSpeedMultiplier * activeWeapon.AimSpeedMultiplier;
            float hipStrafe = activeWeapon.HipStrafeSpeedMultiplier;
            float aimStrafe = activeWeapon.AimStrafeSpeedMultiplier * activeWeapon.AimSpeedMultiplier;
            float hipBackward = activeWeapon.HipBackwardSpeedMultiplier;
            float aimBackward = activeWeapon.AimBackwardSpeedMultiplier * activeWeapon.AimSpeedMultiplier;

            // Спринт всегда на "хип"-множителе — ADS на бегу невозможен по дизайну.
            float sprintMultiplier = hipSprint;
            float forwardMultiplier = Mathf.Lerp(hipForward, aimForward, adsProgress);
            float strafeMultiplier = Mathf.Lerp(hipStrafe, aimStrafe, adsProgress);
            float backwardMultiplier = Mathf.Lerp(hipBackward, aimBackward, adsProgress);

            _movementCalculator.UpdateWeaponSpeedMultipliers(
                sprintMultiplier,
                forwardMultiplier,
                strafeMultiplier,
                backwardMultiplier);
        }

        public Vector3 CalculateWishDir(PlayerView view, Vector2 input)
        {
            return _movementCalculator.CalculateWishDir(view, input);
        }

        public void GroundMove(PlayerView view, Vector3 wishDir, float maxSpeed)
        {
            _movementCalculator.GroundMove(view.Forward, view.Right, wishDir, maxSpeed);
        }

        public void AirMove(PlayerView view, Vector3 wishDir, float maxSpeed)
        {
            _movementCalculator.AirMove(view.Forward, view.Right, wishDir, maxSpeed);
        }

        public void ApplyFriction()
        {
            _movementCalculator.ApplyFriction();
        }
    }
}