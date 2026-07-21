using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class WeaponSwitchingSystem : ITickable
    {
        private readonly IInputService _inputService;
        private readonly IWeaponInventory _inventory;

        public WeaponSwitchingSystem(IInputService inputService, IWeaponInventory inventory)
        {
            _inputService = inputService;
            _inventory = inventory;
        }

        public void Tick()
        {
            int targetSlotIndex = _inputService.GetSelectedSlotIndex();
            if (targetSlotIndex != -1)
            {
                _inventory.SwitchToSlot(targetSlotIndex);
            }
        }
    }
}