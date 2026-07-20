using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    [Serializable]
    public abstract class FireMode
    {
        [SerializeField] private string _modeName;
        [SerializeField] private AnimatorOverrideController _animatorOverride;

        [Header("Stat Modifiers")]
        [Tooltip("Все характеристики, которые этот режим огня модифицирует")]
        [SerializeField] private List<StatModifier> _modifiers = new();

        [Header("Damage Profile Modifiers")]
        [Tooltip("Точечные модификаторы профиля урона по дистанции")]
        [SerializeField] private List<DamageProfilePointModifier> _damageProfileModifiers = new();

        [Header("Damage Profile Override")]
        [SerializeField] private bool _overrideDamageProfile;
        [SerializeField] private DamagePoint[] _damageProfileOverride = new[]
        {
            new DamagePoint(0f, 1f),
            new DamagePoint(100f, 0f)
        };

        [Header("Action Cycle Override")]
        [Tooltip("Например SPAS-12: в semi-auto затвор передёргивается сам, в pump — вручную. " +
                 "Если выключено — используются настройки WeaponConfig.ActionCycle по умолчанию.")]
        [SerializeField] private bool _overrideActionCycle;
        [SerializeField] private ActionCycleSettings _actionCycleOverride = new();

        public string ModeName => _modeName;
        public AnimatorOverrideController AnimatorOverride => _animatorOverride;
        public IReadOnlyList<StatModifier> Modifiers => _modifiers;
        public IReadOnlyList<DamageProfilePointModifier> DamageProfileModifiers => _damageProfileModifiers;

        public float GetDamageMultiplier(float distance, float baseMultiplier)
        {
            if (!_overrideDamageProfile || _damageProfileOverride == null || _damageProfileOverride.Length == 0)
            {
                return Mathf.Clamp01(baseMultiplier);
            }
            return Mathf.Clamp01(DamageProfileHelper.GetDamageMultiplier(_damageProfileOverride, distance));
        }

        /// <summary>Возвращает настройки цикла затвора для этого режима огня, либо дефолт оружия, если не переопределено.</summary>
        public ActionCycleSettings ResolveActionCycle(ActionCycleSettings weaponDefault)
        {
            return _overrideActionCycle ? _actionCycleOverride : weaponDefault;
        }

        public virtual bool IsAutomatic => false;
        public virtual bool IsSafety => false;
    }

    [Serializable]
    public class SafetyFireMode : FireMode
    {
        public override bool IsSafety => true;
    }

    [Serializable]
    public class SingleFireMode : FireMode
    {
    }

    [Serializable]
    public class AutoFireMode : FireMode
    {
        public override bool IsAutomatic => true;
    }

    [Serializable]
    public class BurstFireMode : FireMode
    {
        [Header("Burst Settings")]
        [Min(2)] [SerializeField] private int _burstShotCount = 3;
        [Tooltip("Время в секундах, сколько нельзя стрелять ПОСЛЕ окончания или прерывания бёрста")]
        [Min(0f)] [SerializeField] private float _burstDelay = 0f;
        [Tooltip("Если TRUE: нужно держать кнопку. Если отпустить — бёрст прервется. Если FALSE: один клик выпускает всю очередь.")]
        [SerializeField] private bool _requireHoldToContinue = false;
        public int BurstShotCount => _burstShotCount;
        public float BurstDelay => _burstDelay;
        public bool RequireHoldToContinue => _requireHoldToContinue;
    }
}