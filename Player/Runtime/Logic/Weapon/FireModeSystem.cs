using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class FireModeSystem : ITickable
    {
        private readonly IInputService _inputService;
        private readonly IWeaponInventory _inventory;
        private bool _wasSwitchModePressedLastFrame;
        private bool _wasSafetyPressedLastFrame;

        public FireModeSystem(IInputService inputService, IWeaponInventory inventory)
        {
            _inputService = inputService;
            _inventory = inventory;
        }

        public void Tick()
        {
            var activeWeapon = _inventory.ActiveWeapon;
            if (activeWeapon == null) return;

            bool isSwitchingNow = _inputService.IsSwitchingFireModes();
            if (isSwitchingNow && !_wasSwitchModePressedLastFrame)
            {
                activeWeapon.SwitchToNextFireMode();
            }
            _wasSwitchModePressedLastFrame = isSwitchingNow;

            bool isSafetyNow = _inputService.IsSafetyPressed();
            if (isSafetyNow && !_wasSafetyPressedLastFrame)
            {
                activeWeapon.ToggleSafetyMode();
            }
            _wasSafetyPressedLastFrame = isSafetyNow;
        }
    }
}