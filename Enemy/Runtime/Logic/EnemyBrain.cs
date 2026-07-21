using System;
using System.Collections.Generic;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Core.Audio;
using ZigdarkS.ProjectB.Core.Combat;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.Combat;
using ZigdarkS.ProjectB.Enemy.Logic.Navigation;
using ZigdarkS.ProjectB.Enemy.Logic.Perception;
using ZigdarkS.ProjectB.Enemy.Logic.States;
using ZigdarkS.ProjectB.Enemy.View;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public class EnemyBrain : IDisposable
    {
        private readonly EnemyContext        _context;
        private readonly EnemyStateMachine   _stateMachine;
        private readonly EnemySoundPerception _soundPerception;

        public EnemyAIState CurrentState => _stateMachine.CurrentState;

        public EnemyBrain(
            EnemyView          view,
            EnemyModel         model,
            EnemyConfig        config,
            InstanceProvider<ITargetable>     playerProvider,
            EnemyStateFactory  stateFactory,
            EnemyNavigator     navigator,
            EnemyCombatService combat,
            ISoundEventBus     soundBus,
            EnemySquadRegistry squadRegistry)
        {
            var profile = view.GetComponent<EnemyBehaviourProfile>();

            // ── Слух ──────────────────────────────────────────────────────────
            // profile.UseHearing уже сам возвращает false в Arena Mode (см. EnemyBehaviourProfile).
            EnemySoundPerception soundPerception = null;
            if (profile == null || profile.UseHearing)
            {
                soundPerception = new EnemySoundPerception(soundBus, config);
                soundPerception.Initialize();
            }
            _soundPerception = soundPerception;

            // ── Контекст ──────────────────────────────────────────────────────
            var perception = new EnemyPerception();
            _context = new EnemyContext(
                view, model, config, perception,
                navigator, combat, playerProvider,
                profile, soundPerception);

            // ── Урон: View получает данные снаружи и прокидывает их в Model.
            view.OnDamaged += model.TakeDamage;

            // ── Отряд ─────────────────────────────────────────────────────────
            if (profile == null || profile.ParticipateInSquad)
            {
                var squadMember = view.GetComponent<EnemySquadMember>();
                if (squadMember != null && squadMember.HasSquad)
                {
                    var squad = squadRegistry.GetOrCreate(squadMember.SquadId);
                    squad.Register(_context);
                    _context.Squad = squad;
                }
            }

            // ── Машина состояний ──────────────────────────────────────────────
            var states = BuildStates(stateFactory, profile);
            _stateMachine = new EnemyStateMachine(states);
            _context.IsStateAvailable = _stateMachine.IsStateRegistered;

            navigator.GenerateNewStrafeDirection(_context);
            _stateMachine.Initialize(_context);
        }

        public void Tick()
        {
            var player = _context.Player;
            if (player == null) return;

            _soundPerception?.UpdatePosition(_context.View.Position);

            _context.Perception.Update(
                _context.View, player,
                _context.Config.EyeHeight,
                _context.Profile);

            _context.Combat.UpdateTimers(_context);
            _context.CurrentThreat = ThreatAssessor.Evaluate(_context);

            _stateMachine.Update(_context);
            _context.View.TickAnimator(_stateMachine.CurrentState, _context.Model.HPPercent <= 0f);

            // ── Поворот ТОЛЬКО в боевых состояниях ────────────────────────────
            var state = _stateMachine.CurrentState;
            bool isCombatState = state == EnemyAIState.Combat
                              || state == EnemyAIState.ReloadCover
                              || state == EnemyAIState.Suppress;

            if (isCombatState && _context.Model.HPPercent > 0f)
            {
                float speed = _context.Config.CombatRotationSpeed;
                if (speed <= 0f)
                {
                    _context.View.RotateTowards(player.EyesPosition);
                }
                else
                {
                    _context.View.RotateTowardsSmoothly(player.EyesPosition, speed);
                }
            }
        }

        public void Dispose()
        {
            _context.View.OnDamaged -= _context.Model.TakeDamage;
            _soundPerception?.Dispose();
            _context.Squad?.Unregister(_context);
        }

        // ── Построение словаря состояний с учётом профиля ─────────────────────

        private static Dictionary<EnemyAIState, IEnemyState> BuildStates(
            EnemyStateFactory stateFactory, EnemyBehaviourProfile profile)
        {
            var states = new Dictionary<EnemyAIState, IEnemyState>
            {
                { EnemyAIState.Idle,   stateFactory.Create(EnemyAIState.Idle)   },
                { EnemyAIState.Combat, stateFactory.Create(EnemyAIState.Combat)  },
                { EnemyAIState.Dead,   stateFactory.Create(EnemyAIState.Dead)    },
            };

            // ВНИМАНИЕ: AllowChase/AllowSearch/AllowAlert/CanPatrol/UseHearing уже сами
            // возвращают false в Arena Mode (инкапсулировано в EnemyBehaviourProfile) —
            // здесь не нужно отдельно проверять profile.IsArenaMode.
            bool allowChase    = profile == null || profile.AllowChase;
            bool allowSearch   = profile == null || profile.AllowSearch;
            bool allowReload   = profile == null || profile.CanTakeCover;
            bool allowAlert    = profile == null || profile.AllowAlert;
            bool allowSuppress = profile == null || profile.AllowSuppress;
            bool allowPatrol   = profile == null || profile.CanPatrol;

            if (allowChase)    states[EnemyAIState.Chase]       = stateFactory.Create(EnemyAIState.Chase);
            if (allowSearch)   states[EnemyAIState.Search]      = stateFactory.Create(EnemyAIState.Search);
            if (allowReload)   states[EnemyAIState.ReloadCover] = stateFactory.Create(EnemyAIState.ReloadCover);
            if (allowAlert)    states[EnemyAIState.Alert]       = stateFactory.Create(EnemyAIState.Alert);
            if (allowSuppress) states[EnemyAIState.Suppress]    = stateFactory.Create(EnemyAIState.Suppress);
            if (allowPatrol)   states[EnemyAIState.Patrol]      = stateFactory.Create(EnemyAIState.Patrol);

            return states;
        }
    }
}