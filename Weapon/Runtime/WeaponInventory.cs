using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using ZigdarkS.ProjectB.Weapon.Logic;

namespace ZigdarkS.ProjectB.Weapon.Inventory
{
    public class WeaponInventory : IWeaponInventory, ITickable
    {
        private readonly Dictionary<int, IWeapon> _slots = new();
        private int _currentSlotIndex = 0;
        private int _pendingSlotIndex = -1;

        private WeaponEquipState _equipState = WeaponEquipState.Ready;
        private float _equipProgress = 1f; // 0 = полностью убрано, 1 = полностью готово

        public event Action<IWeapon> OnActiveWeaponChanged;

        public IWeapon ActiveWeapon => _slots.TryGetValue(_currentSlotIndex, out var w) ? w : null;
        public WeaponEquipState EquipState => _equipState;
        public bool IsWeaponReady => _equipState == WeaponEquipState.Ready;

        public void SetWeaponToSlot(int slotIndex, IWeapon weapon)
        {
            if (slotIndex < 0) return;

            if (_slots.TryGetValue(slotIndex, out var oldWeapon))
            {
                oldWeapon?.Dispose();
            }

            _slots[slotIndex] = weapon;
            weapon?.SetActive(slotIndex == _currentSlotIndex);

            if (slotIndex == _currentSlotIndex)
            {
                OnActiveWeaponChanged?.Invoke(ActiveWeapon);
            }
        }

        public void SwitchToSlot(int slotIndex)
        {
            if (slotIndex < 0) return;
            if (!_slots.TryGetValue(slotIndex, out var targetWeapon) || targetWeapon == null) return;

            // Уже держим это оружие и оно готово — нечего делать
            if (slotIndex == _currentSlotIndex && _pendingSlotIndex == -1 && _equipState == WeaponEquipState.Ready)
                return;

            // РЕВЕРС: убираем текущее (Holstering), но игрок передумал и снова жмёт этот же слот —
            // не долистываем до 0, а сразу разворачиваем прогресс обратно вверх, без переключения оружия.
            if (_equipState == WeaponEquipState.Holstering && slotIndex == _currentSlotIndex)
            {
                _pendingSlotIndex = -1;
                _equipState = WeaponEquipState.Drawing;
                ActiveWeapon?.PlayDrawAnimation();
                return;
            }

            if (slotIndex == _pendingSlotIndex) return; // уже переключаемся туда

            var current = ActiveWeapon;

            if (current == null)
            {
                // Пусто в руках — сразу начинаем доставать целевое
                _currentSlotIndex = slotIndex;
                _pendingSlotIndex = -1;
                _equipProgress = 0f;
                _equipState = WeaponEquipState.Drawing;

                var newWeapon = ActiveWeapon;
                newWeapon?.SetActive(true);
                newWeapon?.PlayDrawAnimation();
                OnActiveWeaponChanged?.Invoke(newWeapon);
                return;
            }

            _pendingSlotIndex = slotIndex;
            current.CancelReload();
            _equipState = WeaponEquipState.Holstering;
            current.PlayHolsterAnimation();
        }

        public void Tick()
        {
            if (_equipState == WeaponEquipState.Ready) return;

            var current = ActiveWeapon;

            if (_equipState == WeaponEquipState.Holstering)
            {
                float rate = current != null && current.HolsterDuration > 0f ? 1f / current.HolsterDuration : 1000f;
                _equipProgress = Mathf.MoveTowards(_equipProgress, 0f, Time.deltaTime * rate);

                if (_equipProgress <= 0f)
                {
                    current?.SetActive(false);
                    SwapToPendingWeapon();
                }
            }
            else if (_equipState == WeaponEquipState.Drawing)
            {
                float rate = current != null && current.DrawDuration > 0f ? 1f / current.DrawDuration : 1000f;
                _equipProgress = Mathf.MoveTowards(_equipProgress, 1f, Time.deltaTime * rate);

                if (_equipProgress >= 1f)
                {
                    _equipState = WeaponEquipState.Ready;
                }
            }
        }

        private void SwapToPendingWeapon()
        {
            _currentSlotIndex = _pendingSlotIndex;
            _pendingSlotIndex = -1;

            var newWeapon = ActiveWeapon;
            newWeapon?.SetActive(true);
            newWeapon?.PlayDrawAnimation();

            _equipState = WeaponEquipState.Drawing;
            // _equipProgress уже 0 — так что Draw начнётся от 0, естественно

            OnActiveWeaponChanged?.Invoke(newWeapon);
        }

        public void ClearAll()
        {
            foreach (var weapon in _slots.Values)
            {
                weapon?.Dispose();
            }
            _slots.Clear();
            _currentSlotIndex = 0;
            _pendingSlotIndex = -1;
            _equipState = WeaponEquipState.Ready;
            _equipProgress = 1f;
            OnActiveWeaponChanged?.Invoke(null);
        }
    }
}