using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class WeaponReloadSystem : ITickable
    {
        private readonly IInputService _inputService;
        private readonly IWeaponInventory _inventory;
        private bool _wasReloadPressedLastFrame;

        public WeaponReloadSystem(IInputService inputService, IWeaponInventory inventory)
        {
            _inputService = inputService;
            _inventory = inventory;
        }

        public void Tick()
        {
            var activeWeapon = _inventory.ReadyWeapon;
            if (activeWeapon == null) return;

            bool isReloadPressed = _inputService.IsReloading();
            if (isReloadPressed && !_wasReloadPressedLastFrame)
            {
                activeWeapon.TryReload();
            }
            _wasReloadPressedLastFrame = isReloadPressed;
            activeWeapon.TickReload();
        }
    }
}