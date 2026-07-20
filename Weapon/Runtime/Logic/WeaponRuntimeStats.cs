using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>
    /// Чистый калькулятор рантайм-статов оружия. Не знает о стрельбе, ADS, View или режимах огня —
    /// только применяет модификаторы модулей и текущего FireMode к базовым значениям из конфига.
    /// </summary>
    public class WeaponRuntimeStats
    {
        private readonly Data.WeaponConfig _config;

        private float _currentDamage;
        private float _currentRange;
        private float _currentRpm;
        private int _currentPelletCount;
        private float _currentMinHipSpread;
        private float _currentMaxHipSpread;
        private float _currentAimSpread;
        private float _currentSpreadPerShot;
        private float _currentSpreadDecaySpeed;
        private float _currentAdsDuration;
        private bool _currentCanShootWhileRunning;
        private Data.DamagePoint[] _currentDamageProfile;

        private float _currentHipSprintSpeedMultiplier = 1f;
        private float _currentAimSpeedMultiplier = 1f;
        private float _currentAimSprintSpeedMultiplier = 1f;
        private float _currentHipForwardSpeedMultiplier = 1f;
        private float _currentAimForwardSpeedMultiplier = 1f;
        private float _currentHipStrafeSpeedMultiplier = 1f;
        private float _currentAimStrafeSpeedMultiplier = 1f;
        private float _currentHipBackwardSpeedMultiplier = 1f;
        private float _currentAimBackwardSpeedMultiplier = 1f;
        private bool _currentCanRunWhileAds;

        private float _currentAdsZoomMultiplier = 1f;
        private bool _currentAdsFovIsFixed;
        private float _currentAdsFixedFov;

        private float _currentAdsSwayStableDuration;
        private float _currentAdsSwayAmplitudeGrowthRate;
        private float _currentAdsSwayMaxAmplitude;
        private float _currentAdsSwayFrequency;
        private float _currentAdsSwayResponseSpeed;

        private AnimationCurve _currentAdsFovCurve;
        public AnimationCurve AdsFovCurve => _currentAdsFovCurve;

        public float AdsZoomMultiplier => _currentAdsZoomMultiplier;
        public bool AdsFovIsFixed => _currentAdsFovIsFixed;
        public float AdsFixedFov => _currentAdsFixedFov;

        public float Damage => _currentDamage;
        public float Range => _currentRange;
        public float CurrentRpm => _currentRpm;
        public int PelletCount => _currentPelletCount;
        public float MinHipSpread => _currentMinHipSpread;
        public float MaxHipSpread => _currentMaxHipSpread;
        public float AimSpread => _currentAimSpread;
        public float SpreadPerShot => _currentSpreadPerShot;
        public float SpreadDecaySpeed => _currentSpreadDecaySpeed;
        public float AdsDuration => _currentAdsDuration;
        public bool CanRunWhileAds => _currentCanRunWhileAds;
        public bool CanShootWhileRunning => _currentCanShootWhileRunning;

        public float HipSprintSpeedMultiplier => _currentHipSprintSpeedMultiplier;
        public float AimSpeedMultiplier => _currentAimSpeedMultiplier;
        public float AimSprintSpeedMultiplier => _currentAimSprintSpeedMultiplier;
        public float HipForwardSpeedMultiplier => _currentHipForwardSpeedMultiplier;
        public float AimForwardSpeedMultiplier => _currentAimForwardSpeedMultiplier;
        public float HipStrafeSpeedMultiplier => _currentHipStrafeSpeedMultiplier;
        public float AimStrafeSpeedMultiplier => _currentAimStrafeSpeedMultiplier;
        public float HipBackwardSpeedMultiplier => _currentHipBackwardSpeedMultiplier;
        public float AimBackwardSpeedMultiplier => _currentAimBackwardSpeedMultiplier;

        public float AdsSwayStableDuration => _currentAdsSwayStableDuration;
        public float AdsSwayAmplitudeGrowthRate => _currentAdsSwayAmplitudeGrowthRate;
        public float AdsSwayMaxAmplitude => _currentAdsSwayMaxAmplitude;
        public float AdsSwayFrequency => _currentAdsSwayFrequency;
        public float AdsSwayResponseSpeed => _currentAdsSwayResponseSpeed;

        // Прямые проброски на конфиг — модификаторов для них пока не существует (см. известное ограничение
        // MovementSpreadService: модули/фаермоды не могут их менять до тех пор, пока сюда не добавят StatType).
        public float MoveSpreadMultiplier => _config.SpreadPenalties.HipMoveMultiplier;
        public float AdsMoveSpreadMultiplier => _config.SpreadPenalties.AdsMoveMultiplier;
        public float CrouchMoveSpreadMultiplier => _config.SpreadPenalties.HipCrouchMoveMultiplier;
        public float AdsCrouchMoveSpreadMultiplier => _config.SpreadPenalties.AdsCrouchMoveMultiplier;
        public float AirborneSpreadPenalty => _config.SpreadPenalties.HipAirbornePenalty;
        public float AdsAirborneSpreadPenalty => _config.SpreadPenalties.AdsAirbornePenalty;

        // Свойства, которые модули обычно не трогают — прямая проброска на конфиг
        public GameObject ImpactEffectPrefab => _config.Visuals.ImpactEffectPrefab;
        public float KnockbackForce => _config.BaseStats.KnockbackForce;

        public WeaponRuntimeStats(Data.WeaponConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Базовый (без учёта кривой текущего FireMode) мультипликатор урона по дистанции.
        /// Финальное смешивание с кривой режима огня — ответственность вызывающей стороны (WeaponInstance).
        /// </summary>
        public float GetBaseDamageMultiplier(float distance)
        {
            return _currentDamageProfile != null && _currentDamageProfile.Length > 0
                ? Data.DamageProfileHelper.GetDamageMultiplier(_currentDamageProfile, distance)
                : _config.GetDamageMultiplier(distance);
        }

        public void RecalculateStats(IReadOnlyList<Data.WeaponModule> equippedModules, Data.FireMode currentMode)
        {
            ResetToConfigDefaults();

            ApplyModifiers(equippedModules, currentMode, Data.ModifierType.Additive);
            ApplyModifiers(equippedModules, currentMode, Data.ModifierType.Multiplicative);
            ApplyModifiers(equippedModules, currentMode, Data.ModifierType.Override);
        }

        private void ResetToConfigDefaults()
        {
            _currentDamage = _config.BaseStats.Damage;
            _currentRange = _config.Range;
            _currentRpm = _config.BaseStats.Rpm;
            _currentPelletCount = _config.BaseStats.PelletCount;
            _currentMinHipSpread = _config.Accuracy.MinHipSpread;
            _currentMaxHipSpread = _config.Accuracy.MaxHipSpread;
            _currentAimSpread = _config.Accuracy.AimSpread;
            _currentSpreadPerShot = _config.Accuracy.SpreadPerShot;
            _currentSpreadDecaySpeed = _config.Accuracy.SpreadDecaySpeed;
            _currentAdsDuration = _config.BaseStats.AdsDuration;
            _currentDamageProfile = CloneDamageProfile(_config.DamageProfile);

            _currentHipSprintSpeedMultiplier = _config.SpeedModifiers.HipSprintSpeedMultiplier;
            _currentAimSpeedMultiplier = _config.SpeedModifiers.AimSpeedMultiplier;
            _currentAimSprintSpeedMultiplier = _config.SpeedModifiers.AimSprintSpeedMultiplier;
            _currentHipForwardSpeedMultiplier = _config.SpeedModifiers.HipForwardSpeedMultiplier;
            _currentAimForwardSpeedMultiplier = _config.SpeedModifiers.AimForwardSpeedMultiplier;
            _currentHipStrafeSpeedMultiplier = _config.SpeedModifiers.HipStrafeSpeedMultiplier;
            _currentAimStrafeSpeedMultiplier = _config.SpeedModifiers.AimStrafeSpeedMultiplier;
            _currentHipBackwardSpeedMultiplier = _config.SpeedModifiers.HipBackwardSpeedMultiplier;
            _currentAimBackwardSpeedMultiplier = _config.SpeedModifiers.AimBackwardSpeedMultiplier;
            _currentAdsFovCurve = _config.BaseStats.AdsFovCurve;
            _currentAdsZoomMultiplier = _config.Accuracy.AdsZoomMultiplier;
            _currentAdsFovIsFixed = false;
            _currentAdsFixedFov = 0f;

            _currentAdsSwayStableDuration = _config.AdsSway.StableDuration;
            _currentAdsSwayAmplitudeGrowthRate = _config.AdsSway.AmplitudeGrowthRate;
            _currentAdsSwayMaxAmplitude = _config.AdsSway.MaxAmplitude;
            _currentAdsSwayFrequency = _config.AdsSway.Frequency;
            _currentAdsSwayResponseSpeed = _config.AdsSway.ResponseSpeed;
        }

        private void ApplyModifiers(IReadOnlyList<Data.WeaponModule> equippedModules, Data.FireMode currentMode, Data.ModifierType type)
        {
            if (equippedModules != null)
            {
                foreach (var module in equippedModules)
                {
                    if (module == null) continue;
                    ApplyModifierSet(module.Modifiers, module.DamageProfileModifiers, type);
                }
            }

            if (currentMode != null)
            {
                ApplyModifierSet(currentMode.Modifiers, currentMode.DamageProfileModifiers, type);
            }
        }

        private void ApplyModifierSet(
            IReadOnlyList<Data.StatModifier> statModifiers,
            IReadOnlyList<Data.DamageProfilePointModifier> profileModifiers,
            Data.ModifierType type)
        {
            foreach (var mod in profileModifiers)
            {
                if (mod.Type != type) continue;
                ApplyDamageProfilePointModifier(type, mod);
            }

            foreach (var mod in statModifiers)
            {
                if (mod.Type != type) continue;
                ApplyStatModifier(type, mod);
            }
        }

        private void ApplyStatModifier(Data.ModifierType type, Data.StatModifier mod)
        {
            switch (mod.Stat)
            {
                case Data.StatType.Damage: _currentDamage = Apply(type, _currentDamage, mod.Value); break;
                case Data.StatType.Range: _currentRange = Apply(type, _currentRange, mod.Value); break;
                case Data.StatType.Rpm: _currentRpm = Apply(type, _currentRpm, mod.Value); break;
                case Data.StatType.MinHipSpread: _currentMinHipSpread = Apply(type, _currentMinHipSpread, mod.Value); break;
                case Data.StatType.MaxHipSpread: _currentMaxHipSpread = Apply(type, _currentMaxHipSpread, mod.Value); break;
                case Data.StatType.AimSpread: _currentAimSpread = Apply(type, _currentAimSpread, mod.Value); break;
                case Data.StatType.SpreadPerShot: _currentSpreadPerShot = Apply(type, _currentSpreadPerShot, mod.Value); break;
                case Data.StatType.SpreadDecaySpeed: _currentSpreadDecaySpeed = Apply(type, _currentSpreadDecaySpeed, mod.Value); break;
                case Data.StatType.AdsDuration: _currentAdsDuration = Apply(type, _currentAdsDuration, mod.Value); break;

                case Data.StatType.HipSprintSpeedMultiplier: _currentHipSprintSpeedMultiplier = Apply(type, _currentHipSprintSpeedMultiplier, mod.Value); break;
                case Data.StatType.AimSpeedMultiplier: _currentAimSpeedMultiplier = Apply(type, _currentAimSpeedMultiplier, mod.Value); break;
                case Data.StatType.AimSprintSpeedMultiplier: _currentAimSprintSpeedMultiplier = Apply(type, _currentAimSprintSpeedMultiplier, mod.Value); break;
                case Data.StatType.HipForwardSpeedMultiplier: _currentHipForwardSpeedMultiplier = Apply(type, _currentHipForwardSpeedMultiplier, mod.Value); break;
                case Data.StatType.AimForwardSpeedMultiplier: _currentAimForwardSpeedMultiplier = Apply(type, _currentAimForwardSpeedMultiplier, mod.Value); break;
                case Data.StatType.HipStrafeSpeedMultiplier: _currentHipStrafeSpeedMultiplier = Apply(type, _currentHipStrafeSpeedMultiplier, mod.Value); break;
                case Data.StatType.AimStrafeSpeedMultiplier: _currentAimStrafeSpeedMultiplier = Apply(type, _currentAimStrafeSpeedMultiplier, mod.Value); break;
                case Data.StatType.HipBackwardSpeedMultiplier: _currentHipBackwardSpeedMultiplier = Apply(type, _currentHipBackwardSpeedMultiplier, mod.Value); break;
                case Data.StatType.AimBackwardSpeedMultiplier: _currentAimBackwardSpeedMultiplier = Apply(type, _currentAimBackwardSpeedMultiplier, mod.Value); break;

                case Data.StatType.PelletCount:
                    _currentPelletCount = type switch
                    {
                        Data.ModifierType.Override => (int)mod.Value,
                        Data.ModifierType.Multiplicative => Mathf.Max(1, (int)(_currentPelletCount * mod.Value)),
                        _ => _currentPelletCount + (int)mod.Value
                    };
                    break;

                case Data.StatType.CanRunWhileAds:
                    _currentCanRunWhileAds = mod.Value > 0f;
                    break;

                case Data.StatType.CanShootWhileRunning:
                    _currentCanShootWhileRunning = mod.Value > 0f;
                    break;
                
                case Data.StatType.AdsZoomMultiplier:
                    _currentAdsZoomMultiplier = Apply(type, _currentAdsZoomMultiplier, mod.Value);
                    break;
                case Data.StatType.AdsFixedFov:
                    if (type == Data.ModifierType.Override)
                    {
                        _currentAdsFovIsFixed = true;
                        _currentAdsFixedFov = mod.Value;
                    }
                    break;
                    
                case Data.StatType.AdsFovCurve:
                    if (type == Data.ModifierType.Override)
                    {
                        // Для ссылочных типов вроде AnimationCurve имеет смысл только Override (замена)
                        // Предполагается, что в mod.ObjectValue или где-то еще у вас хранится ссылка на саму кривую
                    }
                    break;

                case Data.StatType.AdsSwayStableDuration:
                    _currentAdsSwayStableDuration = Apply(type, _currentAdsSwayStableDuration, mod.Value);
                    break;
                case Data.StatType.AdsSwayAmplitudeGrowthRate:
                    _currentAdsSwayAmplitudeGrowthRate = Apply(type, _currentAdsSwayAmplitudeGrowthRate, mod.Value);
                    break;
                case Data.StatType.AdsSwayMaxAmplitude:
                    _currentAdsSwayMaxAmplitude = Apply(type, _currentAdsSwayMaxAmplitude, mod.Value);
                    break;
                case Data.StatType.AdsSwayFrequency:
                    _currentAdsSwayFrequency = Apply(type, _currentAdsSwayFrequency, mod.Value);
                    break;
                case Data.StatType.AdsSwayResponseSpeed:
                    _currentAdsSwayResponseSpeed = Apply(type, _currentAdsSwayResponseSpeed, mod.Value);
                    break;
            }
        }

        private static float Apply(Data.ModifierType type, float current, float value)
        {
            return type switch
            {
                Data.ModifierType.Additive => current + value,
                Data.ModifierType.Multiplicative => current * value,
                Data.ModifierType.Override => value,
                _ => current
            };
        }

        private void ApplyDamageProfilePointModifier(Data.ModifierType type, Data.DamageProfilePointModifier modifier)
        {
            if (_currentDamageProfile == null)
            {
                _currentDamageProfile = Array.Empty<Data.DamagePoint>();
            }

            if (_currentDamageProfile.Length == 0)
            {
                _currentDamageProfile = new[] { new Data.DamagePoint(modifier.Distance, 1f) };
            }

            for (int i = 0; i < _currentDamageProfile.Length; i++)
            {
                if (Mathf.Abs(_currentDamageProfile[i].Distance - modifier.Distance) < 0.0001f)
                {
                    float newMultiplier = Apply(type, _currentDamageProfile[i].DamageMultiplier, modifier.Value);
                    _currentDamageProfile[i] = new Data.DamagePoint(modifier.Distance, Mathf.Clamp01(newMultiplier));
                    return;
                }
            }

            Array.Resize(ref _currentDamageProfile, _currentDamageProfile.Length + 1);
            _currentDamageProfile[_currentDamageProfile.Length - 1] = new Data.DamagePoint(modifier.Distance, Mathf.Clamp01(modifier.Value));
        }

        private static Data.DamagePoint[] CloneDamageProfile(Data.DamagePoint[] profile)
        {
            if (profile == null || profile.Length == 0)
            {
                return Array.Empty<Data.DamagePoint>();
            }

            var clone = new Data.DamagePoint[profile.Length];
            for (int i = 0; i < profile.Length; i++)
            {
                clone[i] = new Data.DamagePoint(profile[i].Distance, profile[i].DamageMultiplier);
            }

            Array.Sort(clone, (a, b) => a.Distance.CompareTo(b.Distance));
            return clone;
        }
    }
}