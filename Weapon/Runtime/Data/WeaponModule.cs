using System.Collections.Generic;
using UnityEngine;

namespace ZigdarkS.ProjectB.Weapon.Data
{
    [CreateAssetMenu(fileName = "Mod_NewModule", menuName = "ProjectB/Weapon/Module")]
    public class WeaponModule : ScriptableObject
    {
        [SerializeField] private string _moduleName;
        
        [Tooltip("Список характеристик, которые изменяет этот модуль")]
        [SerializeField] private List<StatModifier> _modifiers = new();

        [Header("Damage Profile Modifiers")]
        [Tooltip("Точечные модификаторы профиля урона по дистанции")]
        [SerializeField] private List<DamageProfilePointModifier> _damageProfileModifiers = new();

        [Header("Visuals")]
        [SerializeField] private GameObject _visualPrefab;

        public string ModuleName => _moduleName;
        public IReadOnlyList<StatModifier> Modifiers => _modifiers;
        public IReadOnlyList<DamageProfilePointModifier> DamageProfileModifiers => _damageProfileModifiers;
        public GameObject VisualPrefab => _visualPrefab;
    }
}