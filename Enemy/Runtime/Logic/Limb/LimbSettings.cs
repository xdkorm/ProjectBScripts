using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Logic.Combat
{
    [System.Serializable]
    public class LimbDamageSettings
    {
        [Tooltip("Множитель урона для данного лимба.\nДля головы можно ставить 0.\nДля критичных частей 1.5 или 2.")]
        public float DamageMultiplier = 1f;

        [Tooltip("Сколько накопленного урона нужно получить, чтобы лимб оторвался.")]
        public float AccumulatedDamageThreshold = 50f;

        [Tooltip("Сколько урона нужно получить за один хит, чтобы лимб оторвался мгновенно.")]
        public float SingleHitBreakThreshold = 100f;

        [Tooltip("Сколько дополнительного урона наносится врагу целиком при отрыве лимба.")]
        public float DamageToEnemyOnBreak = 0f;
    }

    [System.Serializable]
    public class LimbGoreSettings
    {
        [Tooltip("Нужно ли схлопывать исходный лимб (масштаб в ноль) при разрыве?")]
        public bool HideOriginalLimb = true;

        [Tooltip("Появляется ли оторванная часть? Отвечает за спавн оторванной части на месте лимба.")]
        public bool SpawnSeveredLimb = true;

        [Tooltip("Список того, что нужно уничтожить (Destroy) после разрыва.")]
        public GameObject[] ObjectsToDestroyOnBreak;

        [Tooltip("Список префабов оторванной части, которые заспавнятся на месте лимба.")]
        public GameObject[] SeveredLimbPrefabs;
    }
}