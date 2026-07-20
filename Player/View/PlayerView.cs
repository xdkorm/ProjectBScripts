using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using Unity.Cinemachine;

namespace ZigdarkS.ProjectB.Player.View
{
    public class PlayerView : MonoBehaviour, IDamageable, ITargetable
    {
        [Header("Ссылки на компоненты Unity")]
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Transform _playerEyes;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private Animator _armsAnimator;
        [SerializeField] private Transform _weaponContainer;
        [SerializeField] private CinemachineCamera _playerCamera;
        [SerializeField] private Transform _cameraSwayPivot;

        [Header("Настройки визуализации (Разрешено во View)")]
        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;
        public Vector3 Right => transform.right;
        public Vector3 EyesPosition => _playerEyes.position;
        public Vector3 EyesForward => _playerEyes.forward;
        public Vector3 EyesRight => _playerEyes.right;

        public Vector3 Velocity => _controller != null ? _controller.velocity : Vector3.zero;

        public float ControllerRadius => _controller.radius;

        public float ControllerHeight => _controller.height;

        public bool IsGrounded => _controller.isGrounded;

        public Transform WeaponContainer => _weaponContainer;

        public event Action<float> OnDamageReceived;

        private float _targetEyeY;
        private float _eyeYVelocity;
        private float _cameraSmoothTime;

        // Инициализацию высоты теперь тоже будет настраивать Презентер при старте,
        // запрашивая данные из конфига и вызывая публичные методы View.
        
        private void LateUpdate()
        {
            Vector3 localPos = _playerEyes.localPosition;
            localPos.y = Mathf.SmoothDamp(localPos.y, _targetEyeY, ref _eyeYVelocity, _cameraSmoothTime);
            _playerEyes.localPosition = localPos;
        }

        // === МЕТОДЫ-ИСПОЛНИТЕЛИ ===

        public void ApplyAdsSwayOffset(Quaternion localRotationOffset)
        {
            if (_cameraSwayPivot == null) return;
            _cameraSwayPivot.localRotation = localRotationOffset;
        }

        public void Move(Vector3 motion)
        {
            _controller.Move(motion);
        }

        public void RotateCamera(float xRotation)
        {
            _playerEyes.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        public void RotateBody(Vector3 angularVelocity)
        {
            transform.Rotate(angularVelocity);
        }

        public void SetAnimationOverride(AnimatorOverrideController overrideController)
        {
            if (_armsAnimator != null)
            {
                _armsAnimator.runtimeAnimatorController = overrideController;
            }
        }

        public void ForceSetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            Physics.SyncTransforms();
        }

        public void PlayArmsAnimation(string stateName)
        {
            if (_armsAnimator != null) _armsAnimator.Play(stateName, -1, 0f);
        }

        public void PlayArmsShootAnimation()
        {
            if (_armsAnimator != null) _armsAnimator.Play("Shoot", -1, 0f);
        }

        public void SetMovementAnimationSpeed(float speed)
        {
            if (_armsAnimator != null) _armsAnimator.SetFloat("Speed", speed);
        }

        public void Initialize(float cameraSmoothTime)
        {
            _cameraSmoothTime = cameraSmoothTime;
        }

        public void SetHitboxSize(float height, float centerY)
        {
            _controller.height = height;
            _controller.center = new Vector3(0, centerY, 0);
        }

        public void SetTargetEyeHeight(float localY)
        {
            _targetEyeY = localY;
        }

        public void TakeDamage(float damage)
        {
            OnDamageReceived?.Invoke(damage);
        }

        public void SetCameraFOV(float fov)
        {
            if (_playerCamera != null)
            {
                _playerCamera.Lens.FieldOfView = fov;
            }
        }
    }
}