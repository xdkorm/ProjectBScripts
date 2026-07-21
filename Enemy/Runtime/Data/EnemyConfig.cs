using UnityEngine;
using ZigdarkS.ProjectB.Core.Audio;

namespace ZigdarkS.ProjectB.Enemy.Data
{
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "ProjectB/Enemy/Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Здоровье")]
        [SerializeField] private float _maxHP = 50f;

        [Header("Восприятие — зрение")]
        [SerializeField] private float _aggroRange = 40f;
        [SerializeField] private float _eyeHeight = 0.5f;
        [SerializeField] private float _lostSightSearchDelay = 3f;
        [SerializeField] private float _searchDuration = 6f;

        [Header("Восприятие — слух (радиусы слышимости)")]
        [Tooltip("Максимальный общий радиус слуха врага")]
        [SerializeField] [Range(0f, 100f)] private float _hearingRange = 30f;

        [Tooltip("Радиус слышимости шагов игрока")]
        [SerializeField] [Range(0f, 30f)] private float _footstepHearRadius = 8f;

        [Tooltip("Радиус слышимости выстрела (игрока или другого врага)")]
        [SerializeField] [Range(0f, 100f)] private float _gunshotHearRadius = 40f;

        [Tooltip("Радиус слышимости взрыва")]
        [SerializeField] [Range(0f, 150f)] private float _explosionHearRadius = 80f;

        [Tooltip("Радиус слышимости звука перезарядки")]
        [SerializeField] [Range(0f, 20f)] private float _reloadHearRadius = 5f;

        [Tooltip("Радиус слышимости звука смерти (крик/падение)")]
        [SerializeField] [Range(0f, 60f)] private float _deathHearRadius = 25f;

        [Tooltip("Время расследования звука в секундах")]
        [SerializeField] [Range(1f, 15f)] private float _alertDuration = 5f;

        [Header("Дистанции боя")]
        [SerializeField] private float _closeRange = 10f;
        [SerializeField] private float _midRangeMin = 14f;
        [SerializeField] private float _midRangeMax = 22f;

        [Header("Стрельба")]
        [SerializeField] private float _longFireRate = 3f;
        [SerializeField] private float _crazyFireRate = 0.2f;
        [SerializeField] private float _spreadAmount = 0.05f;
        [SerializeField] private float _crazySpreadMultiplier = 1.8f;
        [SerializeField] private float _damage = 8f;
        [SerializeField] private float _shootRange = 100f;
        [SerializeField] private int   _maxAmmo = 8;
        [SerializeField] private float _reloadTime = 4f;
        [SerializeField] private float _laserDisplayTime = 0.04f;

        [Header("Движение")]
        [SerializeField] private float _strafeSpeed = 4f;
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _closeRetreatSpeed = 7.5f;
        [SerializeField] private float _coverSearchRadius = 6f;
        [SerializeField] private float _coverNavSampleRadius = 3f;
        [SerializeField] private float _coverRaycastRange = 25f;

        [Header("Поворот")]
        [Tooltip("Плавность поворота в бою (градусов/сек). 0 = мгновенный")]
        [SerializeField] [Range(0f, 720f)] private float _combatRotationSpeed = 180f;

        [Header("Оценка угрозы")]
        [SerializeField] [Range(0f, 1f)] private float _threatHighHPThreshold = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float _threatCriticalHPThreshold = 0.25f;

        // ── Свойства ───────────────────────────────────────────────────────
        public float MaxHP                    => _maxHP;
        public float AggroRange               => _aggroRange;
        public float EyeHeight                => _eyeHeight;
        public float LostSightSearchDelay     => _lostSightSearchDelay;
        public float SearchDuration           => _searchDuration;
        public float HearingRange             => _hearingRange;
        public float FootstepHearRadius       => _footstepHearRadius;
        public float GunshotHearRadius        => _gunshotHearRadius;
        public float ExplosionHearRadius      => _explosionHearRadius;
        public float ReloadHearRadius         => _reloadHearRadius;
        public float DeathHearRadius          => _deathHearRadius;
        public float AlertDuration            => _alertDuration;
        public float CloseRange               => _closeRange;
        public float MidRangeMin              => _midRangeMin;
        public float MidRangeMax              => _midRangeMax;
        public float LongFireRate             => _longFireRate;
        public float CrazyFireRate            => _crazyFireRate;
        public float SpreadAmount             => _spreadAmount;
        public float CrazySpreadMultiplier    => _crazySpreadMultiplier;
        public float Damage                   => _damage;
        public float ShootRange               => _shootRange;
        public int   MaxAmmo                  => _maxAmmo;
        public float ReloadTime               => _reloadTime;
        public float LaserDisplayTime         => _laserDisplayTime;
        public float StrafeSpeed              => _strafeSpeed;
        public float PatrolSpeed              => _patrolSpeed;
        public float CloseRetreatSpeed        => _closeRetreatSpeed;
        public float CoverSearchRadius        => _coverSearchRadius;
        public float CoverNavSampleRadius     => _coverNavSampleRadius;
        public float CoverRaycastRange        => _coverRaycastRange;
        public float CombatRotationSpeed      => _combatRotationSpeed;
        public float ThreatHighHPThreshold    => _threatHighHPThreshold;
        public float ThreatCriticalHPThreshold => _threatCriticalHPThreshold;

        /// <summary>
        /// Возвращает эффективный радиус слышимости конкретного типа звука.
        /// Ограничен общим HearingRange врага — так можно делать "глухих" врагов.
        /// </summary>
        public float GetHearRadius(SoundType type)
        {
            float raw = type switch
            {
                SoundType.Footstep  => _footstepHearRadius,
                SoundType.Gunshot   => _gunshotHearRadius,
                SoundType.Explosion => _explosionHearRadius,
                SoundType.Reload    => _reloadHearRadius,
                SoundType.Death     => _deathHearRadius,
                _                   => _gunshotHearRadius,
            };
            return Mathf.Min(raw, _hearingRange);
        }
    }
}