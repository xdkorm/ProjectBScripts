using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Weapon.Factory;
using ZigdarkS.ProjectB.Weapon.Data;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic
{
    // ФИКС БАГА №5: Добавили IDisposable, чтобы VContainer вызывал метод Dispose()
    public class PlayerSpawner : IStartable, IDisposable
    {
        private readonly IObjectResolver _container;
        private readonly PlayerContainer _containerPrefab;
        private readonly Transform _spawnPoint;
        private readonly PlayerPresenter.Factory _presenterFactory;
        private readonly InstanceProvider<PlayerView> _playerProvider;
        private readonly WeaponFactory _weaponFactory;
        private readonly PlayerLoadoutConfig _loadoutConfig;
        private readonly WeaponInventory _inventory;

        private PlayerPresenter _playerPresenter;

        public PlayerSpawner(
            IObjectResolver container, 
            PlayerContainer playerContainerPrefab, 
            Transform spawnPoint,
            PlayerPresenter.Factory presenterFactory,
            InstanceProvider<PlayerView> playerProvider,
            WeaponFactory weaponFactory,
            PlayerLoadoutConfig loadoutConfig, 
            WeaponInventory inventory)
        {
            _container = container;
            _containerPrefab = playerContainerPrefab;
            _spawnPoint = spawnPoint;
            _presenterFactory = presenterFactory;
            _playerProvider = playerProvider;
            _weaponFactory = weaponFactory;
            _loadoutConfig = loadoutConfig;
            _inventory = inventory;
        }

        public void Start()
        {
            Vector3 spawnPosition = _spawnPoint != null ? _spawnPoint.position : Vector3.zero; 
            PlayerContainer spawnedContainer = _container.Instantiate(_containerPrefab, spawnPosition, Quaternion.identity);

            PlayerView childPlayerView = spawnedContainer.PlayerView;
            
            // Оставляем регистрацию здесь, как главную точку входа для игрока
            _playerProvider.Register(childPlayerView);

            _playerPresenter = _presenterFactory.Invoke(childPlayerView);
            _playerPresenter.Initialize();

            // Динамически заполняем инвентарь на основе переданного дата-конфига
            if (_loadoutConfig != null && _loadoutConfig.StartingWeapons != null)
            {
                for (int i = 0; i < _loadoutConfig.StartingWeapons.Count; i++)
                {
                    var weaponData = _loadoutConfig.StartingWeapons[i];
                    if (weaponData == null) continue;

                    var weaponInstance = _weaponFactory.Create(weaponData, childPlayerView.WeaponContainer);
                    _inventory.SetWeaponToSlot(i, weaponInstance);
                }
            }
            else
            {
                Debug.LogWarning("[PlayerSpawner] Настройки стартового оружия (Loadout) не заданы в GameLifetimeScope!");
            }
        }

        public void Dispose()
        {
            _inventory?.ClearAll();
            _playerProvider.Unregister();
            _playerPresenter?.Dispose();
        }
    }
}