using System;
using ZigdarkS.ProjectB.Weapon.Logic;
using ZigdarkS.ProjectB.Weapon.View;

namespace ZigdarkS.ProjectB.Weapon.Inventory
{
    public enum WeaponEquipState
    {
        Ready,
        Holstering,
        Drawing
    }

    public interface IWeaponInventory
    {
        IWeapon ActiveWeapon { get; }
        WeaponEquipState EquipState { get; }
        bool IsWeaponReady { get; }

        event Action<IWeapon> OnActiveWeaponChanged;

        void SwitchToSlot(int slotIndex);
        void Tick();
    }
}