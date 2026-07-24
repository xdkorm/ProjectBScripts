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

        /// <summary>
        /// Активное оружие, если оно полностью готово к действиям (EquipState == Ready).
        /// null во время Drawing/Holstering или если оружия нет вообще.
        /// Используй это вместо связки ActiveWeapon + IsWeaponReady в системах-потребителях.
        /// </summary>
        IWeapon ReadyWeapon { get; }

        event Action<IWeapon> OnActiveWeaponChanged;

        void SwitchToSlot(int slotIndex);
        void Tick();
    }
}