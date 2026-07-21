using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    /// <summary>
    /// Добавь на префаб врага и задай squadId.
    /// Все враги с одинаковым ID будут координироваться как отряд.
    /// Оставь пустым — враг действует автономно.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySquadMember : MonoBehaviour
    {
        [Tooltip("Уникальный ID отряда. Одинаковый у всех членов группы. Пустой = без отряда.")]
        [SerializeField] private string _squadId = "";

        public string SquadId => _squadId;
        public bool HasSquad  => !string.IsNullOrEmpty(_squadId);
    }
}