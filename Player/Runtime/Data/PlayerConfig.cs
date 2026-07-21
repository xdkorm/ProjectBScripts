using UnityEngine;

namespace ZigdarkS.ProjectB.Player.Data
{
    [CreateAssetMenu(fileName = "NewPlayerConfig", menuName = "ProjectB/Player/Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] private DefaultPrefs _defaultPrefs;
        public DefaultPrefs DefaulPrefs => _defaultPrefs;
        [SerializeField] private HealthConfig _health;
        public HealthConfig Health => _health;
        [SerializeField] private MovementConfig _movement;
        public MovementConfig Movement => _movement;
        [SerializeField] private HitboxConfig _hitbox;
        public HitboxConfig Hitbox => _hitbox;
        [SerializeField] private SlideConfig _slide;
        public SlideConfig Slide => _slide;
        [SerializeField] private SlideCancelConfig _slideCancel;
        public SlideCancelConfig SlideCancel => _slideCancel;
        [SerializeField] private FallSlideBonusConfig _fallSlideBonus;
        public FallSlideBonusConfig FallSlideBonus => _fallSlideBonus;
        [SerializeField] private PhysicsConfig _physics;
        public PhysicsConfig Physics => _physics;
        [SerializeField] private VaultConfig _vault;
        public VaultConfig Vault => _vault;
    }

    [System.Serializable]
    public class DefaultPrefs
    {
        [Min(0)] [SerializeField] private float _sensitivity = 2.5f;
        public float Sensitivity => _sensitivity;
        [Min(0)] [SerializeField] private float _playerFov = 90f;
        public float PlayerFov => _playerFov; 
    }

    [System.Serializable]
    public class HealthConfig
    {
        [Min(0)] [SerializeField] private float _maxHP = 100f;
        public float MaxHP => _maxHP;
    }

    [System.Serializable]
    public class MovementConfig
    {
        [SerializeField] private float _walkSpeed = 8f;
        [SerializeField] private float _sprintSpeed = 13f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _maxSprintAngle = 35f;
        [SerializeField] private float _forwardSpeedMultiplier = 1.0f;
        [SerializeField] private float _strafeSpeedMultiplier = 0.9f;
        [SerializeField] private float _backwardSpeedMultiplier = 0.85f;

        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float MaxSprintAngle => _maxSprintAngle;
        public float ForwardSpeedMultiplier => _forwardSpeedMultiplier;
        public float StrafeSpeedMultiplier => _strafeSpeedMultiplier;
        public float BackwardSpeedMultiplier => _backwardSpeedMultiplier;
    }

    [System.Serializable]
    public class HitboxConfig
    {
        [SerializeField] private float _standingHeight = 2.0f;
        public float StandingHeight => _standingHeight;
        [SerializeField] private float _crouchHeight = 1.0f;
        public float CrouchHeight => _crouchHeight;
        [SerializeField] private float _cameraOffsetFromTop = 0.2f;
        public float CameraOffsetFromTop => _cameraOffsetFromTop;
        [SerializeField] private float _crouchSpeedMultiplier = 0.4f;
        public float CrouchSpeedMultiplier => _crouchSpeedMultiplier;
    }

    [System.Serializable]
    public class SlideConfig
    {
        [SerializeField] private float _slideSpeedBoost = 1.6f;
        [SerializeField] private float _maxSlideTime = 0.7f;
        [SerializeField] private float _slideSteerDegreesPerSecond = 90f;
        [SerializeField] private float _slideSteerAngle = 25f;

        public float SlideSpeedBoost => _slideSpeedBoost;
        public float MaxSlideTime => _maxSlideTime;
        public float SlideSteerDegreesPerSecond => _slideSteerDegreesPerSecond;
        public float SlideSteerAngle => _slideSteerAngle;
    }

    [System.Serializable]
    public class SlideCancelConfig
    {
        [Min(0)] [SerializeField] private float _sprintLockDuration = 0.5f;
        [Min(0)] [SerializeField] private float _walkPenaltyDuration = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float _walkSpeedMultiplier = 0.7f;

        public float SprintLockDuration => _sprintLockDuration;
        public float WalkPenaltyDuration => _walkPenaltyDuration;
        public float WalkSpeedMultiplier => _walkSpeedMultiplier;
    }

    [System.Serializable]
    public class FallSlideBonusConfig
    {
        [Min(0)] [SerializeField] private float _minDistanceForBonus = 2f;
        [Min(0)] [SerializeField] private float _distanceForMaxBonus = 8f;
        [Min(0)] [SerializeField] private float _maxTimeBonus = 0.5f;
        [Min(0)] [SerializeField] private float _maxSpeedBonus = 5f;

        public float MinDistanceForBonus => _minDistanceForBonus;
        public float DistanceForMaxBonus => _distanceForMaxBonus;
        public float MaxTimeBonus => _maxTimeBonus;
        public float MaxSpeedBonus => _maxSpeedBonus;
    }

    [System.Serializable]
    public class PhysicsConfig
    {
        [SerializeField] private float _friction = 6.0f;
        [SerializeField] private float _groundAccelerate = 10.0f;
        [SerializeField] private float _airAccelerate = 100.0f;
        [SerializeField] private float _maxAirWishSpeed = 1.5f;
        [SerializeField] private float _stopSpeed = 4.0f;
        [SerializeField] private float _cameraSmoothTime = 0.12f;

        public float Friction => _friction;
        public float GroundAccelerate => _groundAccelerate;
        public float AirAccelerate => _airAccelerate;
        public float MaxAirWishSpeed => _maxAirWishSpeed;
        public float StopSpeed => _stopSpeed;
        public float CameraSmoothTime => _cameraSmoothTime;
    }

    [System.Serializable]
    public class VaultConfig
    {
        [Header("Классификация препятствия")]
        [Tooltip("Высота от пола, ниже которой считается 'низкое' (обычная ходьба/step offset, без спец. состояния)")]
        [SerializeField] private float _minObstacleHeight = 0.4f;
        public float MinObstacleHeight => _minObstacleHeight;
        [Tooltip("Высота, до которой применяется Vault (средние, viewmodel-анимация)")]
        [SerializeField] private float _maxVaultHeight = 1.3f;
        public float MaxVaultHeight => _maxVaultHeight;
        [Tooltip("Высота, до которой применяется Mantle (высокие, требуют подтягивания)")]
        [SerializeField] private float _maxMantleHeight = 2.2f;
        public float MaxMantleHeight => _maxMantleHeight;

        [Header("Детекция")]
        [SerializeField] private float _forwardCheckDistance = 0.8f;
        public float ForwardCheckDistance => _forwardCheckDistance;
        [SerializeField] private float _topCheckHeight = 2.5f; // насколько высоко проверяем верх препятствия
        public float TopCheckHeight => _topCheckHeight;
        [SerializeField] private float _landingForwardOffset = 0.4f; // насколько дальше от края приземляемся
        public float LandingForwardOffset => _landingForwardOffset;

        [Header("Тайминг")]
        [SerializeField] private float _vaultDuration = 0.45f;
        public float VaultDuration => _vaultDuration;
        [SerializeField] private float _mantleDuration = 0.8f;
        public float MantleDuration => _mantleDuration;
    }
}