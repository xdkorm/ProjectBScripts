using System;
using VContainer.Unity;
using ZigdarkS.ProjectB.Player.Data;
using ZigdarkS.ProjectB.Weapon.Inventory;
using ZigdarkS.ProjectB.Weapon.Logic;

namespace ZigdarkS.ProjectB.Player.HUD
{
    public class HUDPresenter : IStartable, IDisposable
    {
        private readonly PlayerModel _playerModel;
        private readonly HUDView _playerUiView;
        private readonly IWeaponInventory _inventory;

        private IWeapon _subscribedWeapon;

        public HUDPresenter(HUDView playerUiView, PlayerModel playerModel, IWeaponInventory inventory)
        {
            _playerModel = playerModel;
            _playerUiView = playerUiView;
            _inventory = inventory;
        }

        public void Start()
        {
            _playerModel.OnHPChanged += UpdateHealthBar;
            UpdateHealthBar(_playerModel.HPPercent);

            _inventory.OnActiveWeaponChanged += HandleActiveWeaponChanged;
            HandleActiveWeaponChanged(_inventory.ActiveWeapon);
        }

        private void HandleActiveWeaponChanged(IWeapon newWeapon)
        {
            if (_subscribedWeapon != null)
            {
                _subscribedWeapon.OnAmmoChanged -= UpdateAmmoFromActiveWeapon;
            }

            _subscribedWeapon = newWeapon;

            if (_subscribedWeapon != null)
            {
                _subscribedWeapon.OnAmmoChanged += UpdateAmmoFromActiveWeapon;
                UpdateAmmoFromActiveWeapon();
            }
            else
            {
                _playerUiView.ClearAmmo();
            }
        }

        private void UpdateAmmoFromActiveWeapon()
        {
            _playerUiView.UpdateAmmo(_subscribedWeapon.BulletsInMagazine, _subscribedWeapon.ReserveAmmo);
        }

        private void UpdateHealthBar(float hpPercent)
        {
            _playerUiView.UpdateHealthBar(hpPercent);
        }

        public void Dispose()
        {
            _playerModel.OnHPChanged -= UpdateHealthBar;
            _inventory.OnActiveWeaponChanged -= HandleActiveWeaponChanged;

            if (_subscribedWeapon != null)
            {
                _subscribedWeapon.OnAmmoChanged -= UpdateAmmoFromActiveWeapon;
            }
        }
    }
}