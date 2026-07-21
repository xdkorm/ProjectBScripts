using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.Navigation;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    /// <summary>
    /// Враг обходит точки патрулирования.
    /// Переходит в Alert при звуке, в Chase при виде игрока.
    /// Если пути нет — ведёт себя как Idle.
    /// </summary>
    public class PatrolState : IEnemyState
    {
        private EnemyPatrolPath _path;
        private int   _currentWaypoint;
        private bool  _forward = true;
        private float _waitTimer;
        private bool  _waiting;

        public void Enter(EnemyContext context)
        {
            _path = context.View.GetComponent<EnemyPatrolPath>();
            _waiting   = false;
            _waitTimer = 0f;

            if (_path != null && _path.HasPath)
                context.View.SetStopped(false);
            else
                context.View.SetStopped(true); // нет пути — стоим как Idle
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            if (context.Player == null) return;

            // ── Реакция на игрока ─────────────────────────────────────────────
            if (context.Perception.CanSeePlayer)
            {
                nextState = context.IsStateAvailable(EnemyAIState.Chase)
                    ? EnemyAIState.Chase
                    : EnemyAIState.Combat;
                return;
            }

            if (context.Perception.IsPlayerInAggroRange(context.Config.AggroRange))
            {
                nextState = context.IsStateAvailable(EnemyAIState.Chase)
                    ? EnemyAIState.Chase
                    : EnemyAIState.Combat;
                return;
            }

            // ── Реакция на звук ───────────────────────────────────────────────
            if (context.SoundPerception != null
                && context.SoundPerception.HasHeardSound
                && context.IsStateAvailable(EnemyAIState.Alert))
            {
                nextState = EnemyAIState.Alert;
                return;
            }

            // ── Нет пути — просто стоим ───────────────────────────────────────
            if (_path == null || !_path.HasPath) return;

            // ── Ожидание на точке ─────────────────────────────────────────────
            if (_waiting)
            {
                _waitTimer -= Time.deltaTime;
                context.View.SetStopped(true);

                // Поворачиваемся к следующей точке во время ожидания
                if (_waitTimer <= 0f)
                {
                    _waiting = false;
                    context.View.SetStopped(false);
                    _currentWaypoint = _path.GetNextIndex(_currentWaypoint, ref _forward);
                }
                return;
            }

            // ── Движение к текущей точке ──────────────────────────────────────
            Vector3 target   = _path.GetWaypoint(_currentWaypoint);
            float   distance = Vector3.Distance(context.View.Position, target);

            context.Navigator.MoveTo(context, target, context.Config.StrafeSpeed * 0.6f);

            if (distance <= 0.8f)
            {
                _waiting   = true;
                _waitTimer = _path.WaitTime;
            }
        }

        public void Exit(EnemyContext context)
        {
            context.View.SetStopped(false);
        }
    }
}