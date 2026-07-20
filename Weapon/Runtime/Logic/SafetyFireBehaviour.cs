using System;
using UnityEngine;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    public class SafetyFireBehaviour : IFireBehaviour
    {
        public void Execute(WeaponInstance weapon, FireMode mode, Func<Vector3> getOrigin, Func<Vector3> getDirection, Func<bool> holdCondition)
        {
            Debug.Log("Клац! Оружие на предохранителе.");
        }
    }
}