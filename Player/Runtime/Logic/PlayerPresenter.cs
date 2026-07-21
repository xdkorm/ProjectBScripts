using System;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;
using UnityEngine;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class PlayerPresenter : IDisposable
    {
        public delegate PlayerPresenter Factory(PlayerView playerView);

        private readonly PlayerModel _playerModel;
        private readonly PlayerView _playerView;
        private readonly PlayerSettings _playerSettings;

        public PlayerPresenter(PlayerModel playerModel, PlayerView playerView, PlayerSettings playerSettings)
        {
            _playerModel = playerModel;
            _playerView = playerView;
            _playerSettings = playerSettings;
        }

        public void Initialize()
        {
            _playerView.OnDamageReceived += OnPhysicsDamageReceived;

            _playerView.SetCameraFOV(_playerSettings.Fov); // пример применения настройки

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnPhysicsDamageReceived(float damage)
        {
            _playerModel.TakeDamage(damage);
        }

        public void Dispose()
        {
            _playerView.OnDamageReceived -= OnPhysicsDamageReceived;
        }
    }
}