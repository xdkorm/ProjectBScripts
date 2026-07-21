using System.Collections.Generic;
using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.States;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public class EnemyStateMachine
    {
        private readonly Dictionary<EnemyAIState, IEnemyState> _states;
        private IEnemyState   _currentStateLogic;
        private EnemyAIState  _currentStateEnum;

        public EnemyAIState CurrentState => _currentStateEnum;

        public EnemyStateMachine(Dictionary<EnemyAIState, IEnemyState> states)
        {
            _states = states;
        }

        /// <summary>
        /// Проверяет, зарегистрировано ли состояние.
        /// Устанавливай в EnemyContext.IsStateAvailable = этот метод.
        /// </summary>
        public bool IsStateRegistered(EnemyAIState state)
            => _states.ContainsKey(state);

        public void Initialize(EnemyContext context)
        {
            // ТЗ Arena Mode: "It spawns directly into the Combat State" — минуя Idle/Patrol.
            bool startInCombat = context.Profile != null
                              && context.Profile.IsArenaMode
                              && _states.ContainsKey(EnemyAIState.Combat);

            _currentStateEnum  = startInCombat ? EnemyAIState.Combat : EnemyAIState.Idle;
            _currentStateLogic = _states[_currentStateEnum];
            _currentStateLogic.Enter(context);
        }

        public void Update(EnemyContext context)
        {
            // Смерть имеет наивысший приоритет
            if (context.Model.HPPercent <= 0f && _currentStateEnum != EnemyAIState.Dead)
            {
                TransitionTo(EnemyAIState.Dead, context);
                return;
            }

            EnemyAIState nextState = _currentStateEnum;
            _currentStateLogic.Update(context, ref nextState);

            if (nextState != _currentStateEnum)
                TransitionTo(nextState, context);
        }

        // ── Приватное ────────────────────────────────────────

        private void TransitionTo(EnemyAIState target, EnemyContext context)
        {
            if (!_states.ContainsKey(target))
            {
                // Состояние выключено через профиль — предупреждение в редакторе, игнорируем
                Debug.LogWarning($"[EnemyStateMachine] State '{target}' is not registered. " +
                                 $"Transition ignored. Check EnemyBehaviourProfile on '{context.View.name}'.");
                return;
            }

            _currentStateLogic.Exit(context);
            _currentStateEnum  = target;
            _currentStateLogic = _states[target];
            _currentStateLogic.Enter(context);
        }
    }
}