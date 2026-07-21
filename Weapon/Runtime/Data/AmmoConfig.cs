using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    public enum AmmoType 
    { 
        Ball, 
        Buckshot, 
        ArmorPiercing,
        Explosive,
        Incendiary
    }

    [CreateAssetMenu(menuName = "ProjectB/Weapon/Ammo Config")]
    public class AmmoConfig : ScriptableObject
    {
        public AmmoType AmmoType;
        public float BaseDamageMultiplier = 1f;
        public float PenetrationPower = 10f; // одно число, работает и против стен, и против брони
    }
}