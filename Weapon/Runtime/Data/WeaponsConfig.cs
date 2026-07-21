using System.Collections.Generic;
using UnityEngine;
using ZigdarkS.ProjectB.Core.UI;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "ProjectB/Weapon/Config")]
    public class WeaponConfig : ScriptableObject
    {
//---------------------------------------------------------------------------------------------------//
        [Header("Weapon Identity")]
        [Tooltip("Название оружия, отображаемое в UI и списках")]
        [SerializeField] private string _weaponName = "Default Weapon";
        public string WeaponName => _weaponName;
//---------------------------------------------------------------------------------------------------//
        [Header("UI & HUD Visuals")]
        [Tooltip("Стиль прицела, используемый в HUD")]
        [SerializeField] private CrosshairStyle _crosshairStyle = CrosshairStyle.PlusShape;
        public CrosshairStyle CrosshairStyle => _crosshairStyle;
//---------------------------------------------------------------------------------------------------//
        [Header("Visuals & Effects")]
        [Tooltip("Визуальные настройки и префабы эффектов оружия")]
        [SerializeField] private WeaponVisuals _visuals = new();
        public WeaponVisuals Visuals => _visuals;
//---------------------------------------------------------------------------------------------------//
        [Header("Base Stats")]
        [Tooltip("Базовые характеристики оружия (урон, RPM, и т.д.)")]
        [SerializeField] private WeaponBaseStats _baseStats = new();
        public WeaponBaseStats BaseStats => _baseStats;
//---------------------------------------------------------------------------------------------------//
        [Header("Equip")]
        [Tooltip("Настройки экипировки: время достать/убрать оружие")]
        [SerializeField] private EquipSettings _equip = new();
        public EquipSettings Equip => _equip;
//---------------------------------------------------------------------------------------------------//
        [Header("Ammo & Reload")]
        [Tooltip("Параметры боеприпасов и перезарядки")]
        [SerializeField] private AmmoSettings _ammo = new();
        public AmmoSettings Ammo => _ammo;
//---------------------------------------------------------------------------------------------------//
        [Header("Reload Mode")]
        [Tooltip("Режим перезарядки (полный магазин, патроны по одному и т.д.)")]
        [SerializeReference] private ReloadMode _reloadMode = new FullMagazineReloadMode();
        public ReloadMode ReloadMode => _reloadMode;
//---------------------------------------------------------------------------------------------------//
        [Header("Action Cycle (болтовка/помпа)")]
        [Tooltip("Настройки цикла действий для винтовок/помп (например, зарядка патрона)")]
        [SerializeField] private ActionCycleSettings _actionCycle = new();
        public ActionCycleSettings ActionCycle => _actionCycle;
//---------------------------------------------------------------------------------------------------//
        [Header("Ballistic Transition (long-range physical bullets)")]
        [Tooltip("Настройки баллистического перехода: физические снаряды и дальность")]
        [SerializeField] private BallisticSettings _ballistic = new();
        public BallisticSettings Ballistic => _ballistic;
//---------------------------------------------------------------------------------------------------//
        [Header("Native Fire Modes")]
        [Tooltip("Список встроенных режимов стрельбы (авто, полуавто, одиночный)")]
        [SerializeReference] private List<FireMode> _fireModes = new();
        public IReadOnlyList<FireMode> FireModes => _fireModes;

        [Tooltip("Индекс режима стрельбы по умолчанию в списке режимов")]
        [SerializeField] private int _defaultModeIndex = 0;
        public int DefaultModeIndex => _defaultModeIndex;
//---------------------------------------------------------------------------------------------------//
        [Header("Installed Modules")]
        [Tooltip("Модули, установленные по умолчанию на оружие")]
        [SerializeField] private List<WeaponModule> _defaultModules = new();
        public IReadOnlyList<WeaponModule> DefaultModules => _defaultModules;
//---------------------------------------------------------------------------------------------------//
        [Header("Damage Profile")]
        [Tooltip("Кривая урона по дистанции (массив точек: дистанция -> множитель)")]
        [SerializeField] private DamagePoint[] _damageProfile = new[]
        {
            new DamagePoint(0f, 1f),
            new DamagePoint(100f, 0f)
        };
        public DamagePoint[] DamageProfile => _damageProfile;
//---------------------------------------------------------------------------------------------------//
        [Header("Accuracy")]
        [Tooltip("Параметры точности: разброс, прицеливание, прирост разброса")]        
        [SerializeField] private AccuracySettings _accuracy = new();
        public AccuracySettings Accuracy => _accuracy;
//---------------------------------------------------------------------------------------------------//
        [Header("Movement Spread Penalties")]
        [Tooltip("Штрафы к разбросу при разных типах движения и положении")]
        [SerializeField] private MovementSpreadPenalties _spreadPenalties = new();
        public MovementSpreadPenalties SpreadPenalties => _spreadPenalties;
//---------------------------------------------------------------------------------------------------//
        [Header("Movement Modifiers")]
        [Tooltip("Модификаторы скорости при прицеливании/беге/вперед/вбок/назад")]
        [SerializeField] private MovementSpeedModifiers _speedModifiers = new();
        public MovementSpeedModifiers SpeedModifiers => _speedModifiers;
//---------------------------------------------------------------------------------------------------//
        [Header("ADS Sway (покачивание камеры и оружия при удержании прицела)")]
        [Tooltip("Настройки покачивания при прицеливании (ADS)")]
        [SerializeField] private AdsSwaySettings _adsSway = new();
//---------------------------------------------------------------------------------------------------//
        public AdsSwaySettings AdsSway => _adsSway;

        private float _range = 2500f;

        public float GetDamageMultiplier(float distance) => DamageProfileHelper.GetDamageMultiplier(_damageProfile, distance);
        public float Range => _range;

        private float GetMaxRaycastRange()
        {
            if (_damageProfile == null || _damageProfile.Length == 0) return 100f;

            float maxDistance = 0f;
            foreach (var point in _damageProfile)
            {
                if (point.Distance > maxDistance) maxDistance = point.Distance;
            }

            return Mathf.Max(0f, maxDistance);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ballistic.HitscanRange > _range)
            {
                Debug.LogError($"{_weaponName} has HitscanRange more than max range!");
            }
        }
#endif
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class WeaponVisuals
    {
        [Tooltip("Префаб визуальной модели оружия")]
        [SerializeField] private GameObject _modelPrefab;
        public GameObject ModelPrefab => _modelPrefab;
        [Tooltip("Оверрайд анимаций для этого оружия")]
        [SerializeField] private AnimatorOverrideController _animatorOverride;
        public AnimatorOverrideController AnimatorOverride => _animatorOverride;
        [Tooltip("Префаб эффекта вспышки дульного пламени")]
        [SerializeField] private ParticleSystem _muzzleFlashPrefab;
        public ParticleSystem MuzzleFlashPrefab => _muzzleFlashPrefab;
        [Tooltip("Префаб эффекта попадания/импакта")]
        [SerializeField] private GameObject _impactEffectPrefab;
        public GameObject ImpactEffectPrefab => _impactEffectPrefab;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class WeaponBaseStats
    {
        [Tooltip("Базовый урон за выстрел")]
        [SerializeField] [Min(0f)] private float _damage = 10f;
        public float Damage => _damage;
        [Tooltip("Количество дроби/пуль за один выстрел (для помп/шота)")]
        [SerializeField] [Min(1)] private int _pelletCount = 1;
        public int PelletCount => _pelletCount;
        [Tooltip("Скорострельность в выстрелах в минуту (RPM)")]
        [SerializeField] [Min(0f)] private float _rpm = 600f;
        public float Rpm => _rpm;
        [Tooltip("Сила отдачи/отбрасывания цели")]
        [SerializeField] [Min(0f)] private float _knockbackForce = 0f;
        public float KnockbackForce => _knockbackForce;
        [Tooltip("Задержка после бега до возможности выстрела")]
        [SerializeField] [Min(0f)] private float _sprintToFireDelay = 0.2f;
        public float SprintToFireDelay => _sprintToFireDelay;
        [Tooltip("Длительность перехода в прицеливание (ADS)")]
        [SerializeField] [Min(0f)] private float _adsDuration = 0.25f;
        public float AdsDuration => _adsDuration;

        [Tooltip("Кривая изменения поля зрения при прицеливании")]
        [SerializeField] private AnimationCurve _adsFovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve AdsFovCurve => _adsFovCurve;

        public float FireInterval => 60f / Mathf.Max(1f, Rpm);
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class EquipSettings
    {
        [SerializeField] [Min(0f)] private float _drawDuration = 0.3f;
        public float DrawDuration => _drawDuration;
        [SerializeField] [Min(0f)] private float _holsterDuration = 0.3f;
        public float HolsterDuration => _holsterDuration;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class AmmoSettings
    {
        [Header("Capacity")]
        [Tooltip("Вместимость магазина (патронов в одном магазине)")]
        [SerializeField] [Min(1)] private int _magazineSize = 30;
        public int MagazineSize => _magazineSize;
        [Tooltip("Максимальный запас боеприпасов в резерве")]
        [SerializeField] [Min(0)] private int _maxReserveAmmo = 90;
        public int MaxReserveAmmo => _maxReserveAmmo;
        [Tooltip("Начальный запас боеприпасов в резерве при старте")]
        [SerializeField] [Min(0)] private int _startReserveAmmo = 60;
        public int StartReserveAmmo => _startReserveAmmo;

        [Header("Chamber (для помповых/болтовых/трубчатых)")]
        [Tooltip("Используется ли отдельная камера (chamber) для патрона")]
        [SerializeField] private bool _usesChamberSlot = false;
        public bool UsesChamberSlot => _usesChamberSlot;
        [Tooltip("Политика заполнения камеры: сначала или после трубки")]
        [SerializeField] private ChamberFillPolicy _chamberFillPolicy = ChamberFillPolicy.ChamberAfterTube;
        public ChamberFillPolicy ChamberFillPolicy => _chamberFillPolicy;

        [Header("Reload")]
        [Tooltip("Оружие перезаряжается сменой магазина (true) или по-патронно (false)")]
        [SerializeField] private bool _isMagazineFed = true;
        public bool IsMagazineFed => _isMagazineFed;
        [Tooltip("Разрешена ли перезарядка при полном магазине")]
        [SerializeField] private bool _canReloadWithFullMag = false;
        public bool CanReloadWithFullMag => _canReloadWithFullMag;
        [Tooltip("Задержка перед авто-перезарядкой, сек")]
        [SerializeField] [Min(0f)] private float _autoReloadDelay = 0.3f;
        public float AutoReloadDelay => _autoReloadDelay;

        [Header("Ejected Magazine")]
        [Tooltip("Выбрасывается ли магазин при перезарядке")]
        [SerializeField] private bool _dropsMagazineOnReload = false;
        public bool DropsMagazineOnReload => _dropsMagazineOnReload;
        [Tooltip("Префаб выбрасываемого магазина")]
        [SerializeField] private GameObject _magazinePrefab;
        public GameObject MagazinePrefab => _magazinePrefab;
        [Tooltip("Замена слота/суффикса для падения магазина (опционально)")]
        [SerializeField] private Transform _magazineDropSocketOverride;
        public Transform MagazineDropSocketOverride => _magazineDropSocketOverride;

        [Header("Shell Casings")]
        [Tooltip("Выпадают ли гильзы при выстреле")]
        [SerializeField] private bool _ejectsCasingOnFire = true;
        public bool EjectsCasingOnFire => _ejectsCasingOnFire;
        [Tooltip("Префаб гильзы для эффекта выброса")]
        [SerializeField] private GameObject _casingPrefab;
        public GameObject CasingPrefab => _casingPrefab;
        [Tooltip("Сила выброса гильзы")]
        [SerializeField] [Min(0f)] private float _casingEjectForce = 1.5f;
        public float CasingEjectForce => _casingEjectForce;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    public enum ChamberFillPolicy
    {
        ChamberFirst,
        ChamberAfterTube
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class BallisticSettings
    {
        [Tooltip("Включает переход от hitscan к физическим снарядам на дальних дистанциях")]
        [SerializeField] private bool _usesBallisticTransition = true;
        public bool UsesBallisticTransition => _usesBallisticTransition;

        [Tooltip("Дальность hitscan в метрах перед переходом на физические снаряды")]
        [SerializeField] [Min(0f)] private float _hitscanRange = 35f;
        public float HitscanRange => _hitscanRange;

        [Tooltip("Скорость полёта физического снаряда (ед/сек)")]
        [SerializeField] [Min(0f)] private float _projectileSpeed = 750f;
        public float ProjectileSpeed => _projectileSpeed;

        [Tooltip("Мультипликатор гравитации для физического снаряда")]
        [SerializeField] [Min(0f)] private float _projectileGravityScale = 0.05f;
        public float ProjectileGravityScale => _projectileGravityScale;

        [Tooltip("Визуальный префаб для физического снаряда")]
        [SerializeField] private GameObject _projectileVisualPrefab;
        public GameObject ProjectileVisualPrefab => _projectileVisualPrefab;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class AccuracySettings
    {
        [Tooltip("Множитель зума при прицеливании (ADS)")]
        [SerializeField] [Range(0.5f, 1f)] private float _adsZoomMultiplier = 0.8f;
        public float AdsZoomMultiplier => _adsZoomMultiplier;

        [Tooltip("Минимальный разброс прицела в бедре (hip)")]
        [SerializeField] [Min(0f)] private float _minHipSpread = 0.02f;
        public float MinHipSpread => _minHipSpread;

        [Tooltip("Максимальный разброс в бедре (hip)")]
        [SerializeField] [Min(0f)] private float _maxHipSpread = 0.08f;
        public float MaxHipSpread => _maxHipSpread;
        
        [Tooltip("Разброс при прицеливании (ADS)")]
        [SerializeField] [Min(0f)] private float _aimSpread = 0f;
        public float AimSpread => _aimSpread;

        [Tooltip("Прирост разброса за выстрел")]
        [SerializeField] [Min(0f)] private float _spreadPerShot = 0.01f;
        public float SpreadPerShot => _spreadPerShot;
        
        [Tooltip("Скорость уменьшения разброса со временем")]
        [SerializeField] [Min(0f)] private float _spreadDecaySpeed = 0.05f;
        public float SpreadDecaySpeed => _spreadDecaySpeed;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class AdsSwaySettings
    {
        [Tooltip("Сколько секунд после входа в ADS прицел остаётся стабильным, без покачивания")]
        [SerializeField] [Min(0f)] private float _stableDuration = 0.2f;
        public float StableDuration => _stableDuration;

        [Tooltip("Скорость нарастания амплитуды после stable-порога (град/сек)")]
        [SerializeField] [Min(0f)] private float _amplitudeGrowthRate = 0.15f;
        public float AmplitudeGrowthRate => _amplitudeGrowthRate;

        [Tooltip("Максимальная амплитуда покачивания, градусы")]
        [SerializeField] [Min(0f)] private float _maxAmplitude = 1.5f;
        public float MaxAmplitude => _maxAmplitude;

        [Tooltip("Частота колебаний по фигуре восемь, циклов в секунду")]
        [SerializeField] [Min(0f)] private float _frequency = 1.2f;
        public float Frequency => _frequency;

        [Tooltip("Скорость нарастания/затухания амплитуды при входе/выходе из ADS")]
        [SerializeField] [Min(0f)] private float _responseSpeed = 4f;
        public float ResponseSpeed => _responseSpeed;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class MovementSpreadPenalties
    {
        [Tooltip("Множитель прироста разброса при движении в беде")]
        [SerializeField] [Min(0f)] private float _hipMoveMultiplier = 0.003f;
        public float HipMoveMultiplier => _hipMoveMultiplier;
        [Tooltip("Множитель прироста разброса при движении в ADS")]
        [SerializeField] [Min(0f)] private float _adsMoveMultiplier = 0f;
        public float AdsMoveMultiplier => _adsMoveMultiplier;
        [Tooltip("Множитель прироста разброса при движении в присяде (hip)")]
        [SerializeField] [Min(0f)] private float _hipCrouchMoveMultiplier = 0.003f;
        public float HipCrouchMoveMultiplier => _hipCrouchMoveMultiplier;
        [Tooltip("Множитель прироста разброса при движении в присяде (ADS)")]
        [SerializeField] [Min(0f)] private float _adsCrouchMoveMultiplier = 0f;
        public float AdsCrouchMoveMultiplier => _adsCrouchMoveMultiplier;
        [Tooltip("Штраф к разбросу при полёте/находясь в воздухе (hip)")]
        [SerializeField] [Min(0f)] private float _hipAirbornePenalty = 0.03f;
        public float HipAirbornePenalty => _hipAirbornePenalty;
        [Tooltip("Штраф к разбросу при полёте/находясь в воздухе (ADS)")]
        [SerializeField] [Min(0f)] private float _adsAirbornePenalty = 0f;
        public float AdsAirbornePenalty => _adsAirbornePenalty;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
    [System.Serializable]
    public class MovementSpeedModifiers
    {
        [Tooltip("Множитель скорости при прицеливании")]
        [SerializeField] [Min(0f)] private float _aimSpeedMultiplier = 0.7f;
        public float AimSpeedMultiplier => _aimSpeedMultiplier;

        [Tooltip("Множитель скорости бега в беде")]
        [SerializeField] [Min(0f)] private float _hipSprintSpeedMultiplier = 1.0f;
        public float HipSprintSpeedMultiplier => _hipSprintSpeedMultiplier;
        [Tooltip("Множитель скорости бега при прицеливании")]
        [SerializeField] [Min(0f)] private float _aimSprintSpeedMultiplier = 1.0f;
        public float AimSprintSpeedMultiplier => _aimSprintSpeedMultiplier;

        [Tooltip("Множитель скорости при движении вперёд (hip)")]
        [SerializeField] [Min(0f)] private float _hipForwardSpeedMultiplier = 1.0f;
        public float HipForwardSpeedMultiplier => _hipForwardSpeedMultiplier;
        [Tooltip("Множитель скорости при движении вперёд (ADS)")]
        [SerializeField] [Min(0f)] private float _aimForwardSpeedMultiplier = 1.0f;
        public float AimForwardSpeedMultiplier => _aimForwardSpeedMultiplier;

        [Tooltip("Множитель скорости при боковом движении (hip)")]
        [SerializeField] [Min(0f)] private float _hipStrafeSpeedMultiplier = 1.0f;
        public float HipStrafeSpeedMultiplier => _hipStrafeSpeedMultiplier;
        [Tooltip("Множитель скорости при боковом движении (ADS)")]
        [SerializeField] [Min(0f)] private float _aimStrafeSpeedMultiplier = 1.0f;
        public float AimStrafeSpeedMultiplier => _aimStrafeSpeedMultiplier;

        [Tooltip("Множитель скорости при движении назад (hip)")]
        [SerializeField] [Min(0f)] private float _hipBackwardSpeedMultiplier = 1.0f;
        public float HipBackwardSpeedMultiplier => _hipBackwardSpeedMultiplier;
        [Tooltip("Множитель скорости при движении назад (ADS)")]
        [SerializeField] [Min(0f)] private float _aimBackwardSpeedMultiplier = 1.0f;
        public float AimBackwardSpeedMultiplier => _aimBackwardSpeedMultiplier;
    }
//---------------------------------------------------------------------------------------------------//
///////////////////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------------//
}