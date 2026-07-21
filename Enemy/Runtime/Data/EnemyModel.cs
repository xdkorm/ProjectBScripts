using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core.Combat;

namespace ZigdarkS.ProjectB.Enemy.Data
{
    public class EnemyModel
    {
        private readonly Health _health;
        private readonly EnemyConfig _config;

        public EnemyConfig Config => _config;
        public int CurrentAmmo { get; set; }
        public bool IsReloading { get; set; }

        public float CurrentHP => _health.CurrentHP;
        public float HPPercent => _health.Percent;
        public bool IsDead => _health.IsDead;

        /// <summary>
        /// Направление последнего полученного удара (в т.ч. смертельного).
        /// DeadState читает это, чтобы вычислить DeathDirection.
        /// </summary>
        public Vector3 LastHitDirection { get; private set; }

        public event Action<float> OnHPChanged;
        public event Action<Vector3> OnDied;

        public EnemyModel(EnemyConfig config)
        {
            _config = config;
            _health = new Health(config.MaxHP);
            CurrentAmmo = config.MaxAmmo;
        }

        public void TakeDamage(float damage, Vector3 hitDirection)
        {
            if (_health.IsDead) return;

            LastHitDirection = hitDirection;
            _health.TakeDamage(damage);
            OnHPChanged?.Invoke(_health.CurrentHP);

            if (_health.IsDead)
            {
                OnDied?.Invoke(hitDirection);
            }
        }
    }
}
