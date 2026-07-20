using System;

namespace ZigdarkS.ProjectB.Core.Combat
{
    public class Health
    {
        public event Action<float> OnChanged;

        public float MaxHP { get; private set; }
        public float CurrentHP { get; private set; }
        public float Percent => CurrentHP / MaxHP;
        
        // Свойство для проверки, жив ли персонаж
        public bool IsDead => CurrentHP <= 0;

        public Health(float maxHp)
        {
            MaxHP = maxHp;
            CurrentHP = maxHp;
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0 || IsDead) return;

            ApplyChange(-damage);
        }

        public void Heal(float amount)
        {
            if (amount <= 0 || IsDead) return;

            ApplyChange(amount);
        }

        private void ApplyChange(float value)
        {
            CurrentHP = Math.Clamp(CurrentHP + value, 0, MaxHP);
            OnChanged?.Invoke(Percent);
        }
    }
}