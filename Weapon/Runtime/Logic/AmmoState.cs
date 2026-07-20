using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class AmmoState
    {
        private readonly AmmoSettings _settings;
        private int _tubeSize;

        public event Action OnAmmoChanged;

        public int InTube { get; private set; }     // была InMagazine — труба/магазин
        public int InChamber { get; private set; }  // 0 или 1, актуально только если UsesChamberSlot
        public int InReserve { get; private set; }
        public bool IsReloading { get; private set; }

        public bool UsesChamberSlot => _settings.UsesChamberSlot;

        public AmmoState(AmmoSettings settings)
        {
            _settings = settings;
            _tubeSize = settings.MagazineSize;
            InReserve = settings.StartReserveAmmo;

            if (settings.UsesChamberSlot)
            {
                // Стартуем как будто уже заряжено штатно: труба полная, патрон в патроннике
                InChamber = Mathf.Min(1, InReserve);
                InReserve -= InChamber;
                InTube = Mathf.Min(_tubeSize, InReserve);
                InReserve -= InTube;
            }
            else
            {
                InTube = _tubeSize;
            }
        }

        public void SetMagazineSize(int size)
        {
            _tubeSize = size;
            InTube = Mathf.Min(InTube, _tubeSize);
        }

        // --- Готовность стрелять ---
        public bool HasAmmoToFire => _settings.UsesChamberSlot ? InChamber > 0 : InTube > 0;

        public bool CanReload =>
            !IsReloading &&
            InReserve > 0 &&
            (_settings.CanReloadWithFullMag || InTube < _tubeSize ||
             (_settings.UsesChamberSlot && InChamber == 0));

        // --- Выстрел ---
        public void ConsumeBullet()
        {
            if (_settings.UsesChamberSlot)
                InChamber = Mathf.Max(0, InChamber - 1);
            else
                InTube = Mathf.Max(0, InTube - 1);

            OnAmmoChanged?.Invoke();
        }

        // --- Патронник (используется ActionCycleController) ---
        /// <summary>Переносит патрон из трубы в патронник. Вызывается при передёргивании.</summary>
        public bool ChamberRoundFromTube()
        {
            if (!_settings.UsesChamberSlot) return false;
            if (InChamber > 0 || InTube <= 0) return false;
            InTube--;
            InChamber = 1;
            OnAmmoChanged?.Invoke();
            return true;
        }

        // --- Reload (труба) ---
        public void BeginReload() => IsReloading = true;

        public void CompleteReload()
        {
            IsReloading = false;
            OnAmmoChanged?.Invoke();
        }

        public void CancelReload() => IsReloading = false;

        /// <summary>Обычное пополнение трубы/магазина из резерва.</summary>
        public void FillFromReserve(int tubeSize, int maxCount)
        {
            int space = tubeSize - InTube;
            int toLoad = Mathf.Min(space, InReserve, maxCount);
            InTube += toLoad;
            InReserve -= toLoad;
            OnAmmoChanged?.Invoke();
        }

        /// <summary>Сценарий А: первый патрон при полностью пустом оружии идёт сразу в патронник, минуя трубу.</summary>
        public bool LoadDirectlyToChamber()
        {
            if (!_settings.UsesChamberSlot) return false;
            if (InChamber > 0 || InReserve <= 0) return false;
            InChamber = 1;
            InReserve--;
            OnAmmoChanged?.Invoke();
            return true;
        }
    }
}