using System;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    public enum StatType
    {
        Damage,
        Range,
        Rpm,
        MinHipSpread,
        MaxHipSpread,
        AimSpread,
        SpreadPerShot,
        SpreadDecaySpeed,
        AdsDuration,
        PelletCount,
        HipSprintSpeedMultiplier,
        AimSpeedMultiplier,
        AimSprintSpeedMultiplier,
        HipForwardSpeedMultiplier,
        AimForwardSpeedMultiplier,
        HipStrafeSpeedMultiplier,
        AimStrafeSpeedMultiplier,
        HipBackwardSpeedMultiplier,
        AimBackwardSpeedMultiplier,
        CanRunWhileAds,
        CanShootWhileRunning,
        AdsZoomMultiplier, // <-- новое: множитель от текущего FOV игрока
        AdsFixedFov,        // <-- новое: если Override — фиксированный FOV оптики
        AdsFovCurve,
        AdsSwayStableDuration,
        AdsSwayAmplitudeGrowthRate,
        AdsSwayMaxAmplitude,
        AdsSwayFrequency,
        AdsSwayResponseSpeed
    }

    public enum ModifierType
    {
        Additive,       // Плюсует (например, +2 к урону)
        Multiplicative, // Умножает (например, * 1.15 к дальности)
        Override        // Прямо задаёт значение (например, 30 для дальности максимального урона)
    }

    [Serializable]
    public struct StatModifier
    {
        [SerializeField] private StatType _stat;
        [SerializeField] private ModifierType _type;
        [SerializeField] private float _value;

        public StatModifier(StatType stat, ModifierType type, float value)
        {
            _stat = stat;
            _type = type;
            _value = value;
        }

        public StatType Stat => _stat;
        public ModifierType Type => _type;
        public float Value => _value;
    }

    [Serializable]
    public struct DamageProfilePointModifier
    {
        [SerializeField] private ModifierType _type;
        [SerializeField] private float _distance;
        [SerializeField] private float _value;

        public DamageProfilePointModifier(ModifierType type, float distance, float value)
        {
            _type = type;
            _distance = distance;
            _value = value;
        }

        public ModifierType Type => _type;
        public float Distance => _distance;
        public float Value => _value;
    }
}