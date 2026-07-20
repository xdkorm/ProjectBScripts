using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Logic
{
    [CreateAssetMenu(menuName = "ProjectB/Weapon/Effects Config")]
    public class WeaponEffectsConfig : ScriptableObject
    {
        [Tooltip("Универсальный префаб с AudioSource + PooledAudioSource для проигрывания звуков попаданий")]
        public GameObject PooledAudioSourcePrefab;
    }
}