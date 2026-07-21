using System;
using ZigdarkS.ProjectB.Core.Combat;

namespace ZigdarkS.ProjectB.Player.Data
{
    public class PlayerModel : IDamageable
    {
        private readonly Health _health;

        public float CurrentHp => _health.CurrentHP;
        public float HPPercent => _health.Percent;
        public bool IsDead => _health.IsDead;

        public event Action<float> OnHPChanged;

        public PlayerModel(PlayerConfig config)
        {
            _health = new Health(config.Health.MaxHP);
            _health.OnChanged += HandleHealthChanged;
        }

        public void TakeDamage(float damageAmount)
        {
            _health.TakeDamage(damageAmount);
            OnHPChanged?.Invoke(_health.CurrentHP);
        }

        private void HandleHealthChanged(float percent)
        {
            OnHPChanged?.Invoke(percent);
        }
    }
}