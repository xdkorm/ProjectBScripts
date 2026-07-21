using UnityEngine;
using System.Collections.Generic;
using ZigdarkS.ProjectB.Weapon.Data;

namespace ZigdarkS.ProjectB.Player.Data
{
    [CreateAssetMenu(fileName = "NewPlayerLoadout", menuName = "ProjectB/Player/Loadout")]
    public class PlayerLoadoutConfig : ScriptableObject
    {
        [SerializeField] private List<WeaponConfig> _startingWeapons = new();
        public IReadOnlyList<WeaponConfig> StartingWeapons => _startingWeapons;
    }
}