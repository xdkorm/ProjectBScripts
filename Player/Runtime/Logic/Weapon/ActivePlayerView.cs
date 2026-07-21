using System;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Player.View;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public interface IActivePlayerView
    {
        PlayerView Current { get; }
    }

    /// <summary>
    /// Единственное место, которое подписывается на спавн/деспавн игрока
    /// и кеширует текущий PlayerView. Все остальные системы стрельбы
    /// читают IActivePlayerView.Current вместо своей копии подписки.
    /// </summary>
    public class ActivePlayerViewTracker : IActivePlayerView, IDisposable
    {
        private readonly InstanceProvider<PlayerView> _playerProvider;

        public PlayerView Current { get; private set; }

        public ActivePlayerViewTracker(InstanceProvider<PlayerView> playerProvider)
        {
            _playerProvider = playerProvider;
            _playerProvider.OnSpawned += HandleSpawned;
            _playerProvider.OnDespawned += HandleDespawned;
            if (_playerProvider.Instance != null)
            {
                HandleSpawned(_playerProvider.Instance);
            }
        }

        private void HandleSpawned(PlayerView view) => Current = view;
        private void HandleDespawned() => Current = null;

        public void Dispose()
        {
            _playerProvider.OnSpawned -= HandleSpawned;
            _playerProvider.OnDespawned -= HandleDespawned;
        }
    }
}