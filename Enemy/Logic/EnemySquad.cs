using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    /// <summary>
    /// Управляет тактической координацией группы врагов.
    /// Переназначает роли каждые несколько секунд: один подавляет, другой заходит с фланга.
    /// </summary>
    public class EnemySquad
    {
        public string Id { get; }

        private readonly List<EnemyContext> _members     = new();
        private float _roleReassignCooldown               = 5f;
        private float _lastAssignTime                     = -999f;

        public EnemySquad(string id)
        {
            Id = id;
        }

        // ── Регистрация членов ───────────────────────────────────

        public void Register(EnemyContext context)
        {
            if (!_members.Contains(context))
                _members.Add(context);
        }

        public void Unregister(EnemyContext context)
        {
            _members.Remove(context);
            // Переназначить роли оставшимся
            if (_members.Count > 0)
                AssignRoles();
        }

        // ── Роли ────────────────────────────────────────────────

        /// <summary>
        /// Вызывай из CombatState.Update().
        /// Переназначает роли с кулдауном, чтобы не дёргать каждый тик.
        /// </summary>
        public void TryAssignRoles()
        {
            if (Time.time - _lastAssignTime < _roleReassignCooldown) return;
            _lastAssignTime = Time.time;
            AssignRoles();
        }

        private void AssignRoles()
        {
            var alive = _members
                .Where(m => !m.Model.IsDead)
                .OrderByDescending(m => m.Model.HPPercent)
                .ToList();

            // Сбросить всех в Attacker
            foreach (var m in alive)
                m.SquadRole = SquadRole.Attacker;

            if (alive.Count < 2) return;

            // При 2+ членах: самый здоровый — Flanker, второй — Suppressor
            alive[0].SquadRole = SquadRole.Flanker;
            alive[1].SquadRole = SquadRole.Suppressor;
        }

        public int AliveCount => _members.Count(m => !m.Model.IsDead);
    }
}