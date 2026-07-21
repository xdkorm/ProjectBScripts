using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Core;
namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    public class MouseLookController
    {
        private float _xRotation = 0f;
        private readonly PlayerSettings _playerSettings;

        public MouseLookController(PlayerSettings playerSettings)
        {
            _playerSettings = playerSettings;
        }

        // currentFov — реальный FOV камеры в этом кадре (baseFov, если не целится)
        public void HandleMouseLook(PlayerView playerView, IInputService inputService, float currentFov)
        {
            if (playerView == null || inputService == null || _playerSettings == null)
            {
                Debug.LogError("MouseLookController: dependencies are null!");
                return;
            }

            Vector2 mouseInput = inputService.GetMouseLook();

            float baseFov = _playerSettings.Fov;
            float adsSensScale = GetAdsSensitivityScale(baseFov, currentFov);

            // 0.022f — это стандартный m_yaw / m_pitch из CS:GO
            float mouseX = mouseInput.x * _playerSettings.MouseSensitivity * 0.022f * adsSensScale;
            float mouseY = mouseInput.y * _playerSettings.MouseSensitivity * 0.022f * adsSensScale;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            playerView.RotateCamera(_xRotation);
            playerView.RotateBody(Vector3.up * mouseX);
        }

        private float GetAdsSensitivityScale(float baseFov, float currentFov)
        {
            if (baseFov <= 0f || currentFov <= 0f) return 1f;

            float fovRatio = Mathf.Tan(currentFov * Mathf.Deg2Rad * 0.5f)
                            / Mathf.Tan(baseFov * Mathf.Deg2Rad * 0.5f);

            float scale = fovRatio * _playerSettings.AdsSensitivityMultiplier;

            // защита от "мёртвой зоны" на сильной оптике (x8 и т.д.)
            return Mathf.Max(0.05f, scale);
        }
    }
}