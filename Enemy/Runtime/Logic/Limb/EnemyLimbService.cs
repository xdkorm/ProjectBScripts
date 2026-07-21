using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Logic.Combat
{
    public class EnemyLimbService
    {
        private readonly Dictionary<EnemyLimb, float> _accumulatedDamage = new();
        private readonly HashSet<EnemyLimb> _brokenLimbs = new();

        public void Register(EnemyLimb limb)
        {
            limb.OnHit += HandleLimbHit;
            _accumulatedDamage[limb] = 0f;
        }

        public void Unregister(EnemyLimb limb)
        {
            limb.OnHit -= HandleLimbHit;
            _accumulatedDamage.Remove(limb);
            _brokenLimbs.Remove(limb);
        }

        private void HandleLimbHit(EnemyLimb limb, float damage)
        {
            if (_brokenLimbs.Contains(limb)) return;

            bool isEnemyAlreadyDead = limb.MainView != null && limb.MainView.IsRagdollActive;
            float finalDamage = damage * limb.DamageSettings.DamageMultiplier;

            if (!isEnemyAlreadyDead && limb.MainView != null)
            {
                limb.MainView.TakeDamage(finalDamage);
            }

            _accumulatedDamage[limb] += finalDamage;

            bool shouldBreak = _accumulatedDamage[limb] >= limb.DamageSettings.AccumulatedDamageThreshold
                             || damage >= limb.DamageSettings.SingleHitBreakThreshold;

            if (shouldBreak)
            {
                BreakLimb(limb);
            }
        }

        private void BreakLimb(EnemyLimb limb)
        {
            _brokenLimbs.Add(limb);

            if (limb.MainView != null && !limb.MainView.IsRagdollActive && limb.DamageSettings.DamageToEnemyOnBreak > 0f)
            {
                limb.MainView.TakeDamage(limb.DamageSettings.DamageToEnemyOnBreak);
            }

            // Сюда же потом легко добавить: статистику "оторвано конечностей",
            // ачивки, звук/вибрацию геймпада на отрыв и т.п. — всё в одном месте.
        }
    }
}