using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Enemy.View;

namespace ZigdarkS.ProjectB.Enemy.Logic.Combat
{
    public class EnemyCombatService
    {
        private readonly ISoundEventBus _soundBus;
        private readonly RaycastHit[]   _shootHits = new RaycastHit[32];

        public EnemyCombatService(ISoundEventBus soundBus)
        {
            _soundBus = soundBus;
        }

        public void UpdateTimers(EnemyContext context)
        {
            if (context.Model.IsReloading)
            {
                context.ReloadTimer -= Time.deltaTime;
                if (context.ReloadTimer <= 0f)
                {
                    context.Model.CurrentAmmo = context.Model.Config.MaxAmmo;
                    context.Model.IsReloading = false;
                }
            }

            if (context.IsLaserActive)
            {
                context.LaserHideTimer -= Time.deltaTime;
                if (context.LaserHideTimer <= 0f)
                {
                    context.View.HideLaser();
                    context.IsLaserActive = false;
                }
            }
        }

        public void StartReload(EnemyContext context)
        {
            context.Model.IsReloading = true;
            context.ReloadTimer       = context.Config.ReloadTime;
            context.CoverDestination  = null;

            // ── Анимация перезарядки ──────────────────────────────────────────
            context.View.OnReloadAnimation();
        }

        public void TryShoot(EnemyContext context, float fireRate, Vector3 playerPos)
        {
            if (Time.time < context.NextFireTime || context.Model.IsReloading) return;

            var config           = context.Config;
            context.NextFireTime = Time.time + fireRate;
            context.Model.CurrentAmmo--;

            Vector3 startPos = context.View.Position + Vector3.up * config.EyeHeight;
            float   spread   = fireRate <= config.CrazyFireRate + 0.01f
                                ? config.SpreadAmount * config.CrazySpreadMultiplier
                                : config.SpreadAmount;

            Vector3 randomSpread = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread));

            Vector3 shootDir  = ((playerPos - startPos).normalized + randomSpread).normalized;
            Vector3 targetPos = startPos + shootDir * config.ShootRange;

            int hitCount = Physics.RaycastNonAlloc(startPos, shootDir, _shootHits, config.ShootRange);
            RaycastHit? closestHit = null;
            float minDist          = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _shootHits[i];
                if (hit.transform.TryGetComponent<EnemyView>(out _)) continue;
                if (hit.distance < minDist) { minDist = hit.distance; closestHit = hit; }
            }

            if (closestHit.HasValue)
            {
                var hit = closestHit.Value;
                targetPos = hit.point;
                if (hit.transform.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(config.Damage);
            }

            context.View.ShowLaser(startPos, targetPos);
            context.IsLaserActive  = true;
            context.LaserHideTimer = config.LaserDisplayTime;

            // ── Анимация выстрела ─────────────────────────────────────────────
            context.View.OnShootAnimation();

            // ── Звук выстрела для других врагов ──────────────────────────────
            _soundBus.Raise(new SoundEvent(context.View.Position, config.GunshotHearRadius, SoundType.Gunshot));
        }

        public float GetFireRateForDistance(EnemyContext context, float distance)
            => distance <= context.Config.CloseRange
                ? context.Config.CrazyFireRate
                : context.Config.LongFireRate;
    }
}