using System;
using System.Collections;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class BurstFireBehaviour : IFireBehaviour
    {
        public void Execute(WeaponInstance weapon, FireMode mode, Func<Vector3> getOrigin, Func<Vector3> getDirection, Func<bool> holdCondition)
        {
            if (mode is not BurstFireMode burstMode)
            {
                Debug.LogError($"BurstFireBehaviour получил режим типа {mode?.GetType().Name}, ожидался BurstFireMode.");
                return;
            }

            float baseFireInterval = 60f / Mathf.Max(1f, weapon.CurrentRpm);

            if (weapon.View != null)
            {
                weapon.RegisterBurstCoroutine(
                    weapon.View.StartCoroutine(BurstFireRoutine(weapon, burstMode, getOrigin, getDirection, baseFireInterval, holdCondition))
                );
            }
        }

        private IEnumerator BurstFireRoutine(
            WeaponInstance weapon,
            BurstFireMode burstMode,
            Func<Vector3> getOrigin,
            Func<Vector3> getDirection,
            float baseFireInterval,
            Func<bool> holdCondition)
        {
            int burstShotCount = burstMode.BurstShotCount;
            float burstDelay = burstMode.BurstDelay;
            bool requireHoldToContinue = burstMode.RequireHoldToContinue;

            float maxBurstDuration = (burstShotCount - 1) * baseFireInterval + burstDelay;
            weapon.SetNextTimeToFire(Time.time + maxBurstDuration);

            for (int i = 0; i < burstShotCount; i++)
            {
                if (requireHoldToContinue && holdCondition != null && !holdCondition())
                {
                    weapon.SetNextTimeToFire(Time.time + burstDelay);
                    yield break;
                }

                // Каждый выстрел очереди берёт АКТУАЛЬную позицию/направление камеры на момент выстрела,
                // а не замороженные значения из момента нажатия кнопки.
                weapon.ExecuteSingleShot(getOrigin(), getDirection());

                if (i == burstShotCount - 1) break;

                yield return new WaitForSeconds(baseFireInterval);
            }
        }
    }
}