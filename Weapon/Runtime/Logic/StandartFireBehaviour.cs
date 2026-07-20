using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    /// <summary>Общее поведение для Single и Auto — единственная разница между ними (IsAutomatic)
    /// учитывается выше, в PlayerShootingSystem, для решения "стрелять ли при удержании".</summary> 
    public class StandardFireBehaviour : IFireBehaviour
    {
        public void Execute(WeaponInstance weapon, FireMode mode, Func<Vector3> getOrigin, Func<Vector3> getDirection, Func<bool> holdCondition)
        {
            weapon.ExecuteSingleShot(getOrigin(), getDirection());

            float fireInterval = 60f / Mathf.Max(1f, weapon.CurrentRpm);
            weapon.SetNextTimeToFire(Time.time + fireInterval);
        }
    }
}