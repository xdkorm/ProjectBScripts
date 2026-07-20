using UnityEngine;

namespace ZigdarkS.ProjectB.Core.Combat
{
    // Описывает "сопротивление" конкретного куска материи — что для стены, что для бронепластины
    [System.Serializable]
    public struct MaterialResistance
    {
        public float Thickness;         // условная толщина
        public float Density;           // насколько сильно материал гасит пробивную силу за единицу толщины
        public float DamageAbsorption;  // 0..1, сколько урона гасится независимо от пробития
    }

    public readonly struct PenetrationResult
    {
        public readonly bool Penetrated;
        public readonly float ResidualDamage;           // сколько урона проходит дальше
        public readonly float ResidualPenetrationPower;  // сколько "пробивной силы" осталось для следующей преграды

        public PenetrationResult(bool penetrated, float residualDamage, float residualPower)
        {
            Penetrated = penetrated;
            ResidualDamage = residualDamage;
            ResidualPenetrationPower = residualPower;
        }
    }

    public static class PenetrationMath
    {
        public static PenetrationResult Resolve(float incomingDamage, float penetrationPower, MaterialResistance resistance)
        {
            float powerCost = resistance.Thickness * resistance.Density;
            float residualPower = penetrationPower - powerCost;

            float damageAfterAbsorption = incomingDamage * (1f - resistance.DamageAbsorption);

            if (residualPower <= 0f)
                return new PenetrationResult(false, 0f, 0f);

            // чем "впритык" пробили — тем больше урона теряется на преодоление преграды
            float efficiency = Mathf.Clamp01(residualPower / Mathf.Max(0.001f, penetrationPower));
            return new PenetrationResult(true, damageAfterAbsorption * efficiency, residualPower);
        }
    }
}