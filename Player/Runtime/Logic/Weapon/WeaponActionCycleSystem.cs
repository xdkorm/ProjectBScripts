using VContainer.Unity;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Weapon.Inventory;

namespace ZigdarkS.ProjectB.Player.Logic
{
    public class WeaponActionCycleSystem : ITickable
    {
        private readonly IInputService _inputService;
        private readonly IWeaponInventory _inventory;
        private bool _wasCyclePressedLastFrame;

        public WeaponActionCycleSystem(IInputService inputService, IWeaponInventory inventory)
        {
            _inputService = inputService;
            _inventory = inventory;
        }

        /// <summary>
        /// Ручное передёргивание затвора/цевья для болтовок/помповых.
        /// Срабатывает только пока активный цикл ждёт инпут (RequiresManualCycle && IsCycling).
        /// </summary>
        public void Tick()
        {
            var activeWeapon = _inventory.ActiveWeapon;
            if (activeWeapon == null || !_inventory.IsWeaponReady) return;

            bool isCyclePressed = _inputService.IsCyclingAction();
            bool isRisingEdge = isCyclePressed && !_wasCyclePressedLastFrame;
            _wasCyclePressedLastFrame = isCyclePressed;

            if (activeWeapon.RequiresManualCycle && activeWeapon.IsCycling && isRisingEdge)
            {
                activeWeapon.TryManualCycle();
            }
        }
    }
}