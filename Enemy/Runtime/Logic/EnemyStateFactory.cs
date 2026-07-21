using System;
using VContainer;
using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.States;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public class EnemyStateFactory
    {
        private readonly IObjectResolver _container;

        public EnemyStateFactory(IObjectResolver container) => _container = container;

        public IEnemyState Create(EnemyAIState stateType) => stateType switch
        {
            EnemyAIState.Idle        => _container.Resolve<IdleState>(),
            EnemyAIState.Patrol      => _container.Resolve<PatrolState>(),
            EnemyAIState.Chase       => _container.Resolve<ChaseState>(),
            EnemyAIState.Combat      => _container.Resolve<CombatState>(),
            EnemyAIState.ReloadCover => _container.Resolve<ReloadCoverState>(),
            EnemyAIState.Search      => _container.Resolve<SearchState>(),
            EnemyAIState.Alert       => _container.Resolve<AlertState>(),
            EnemyAIState.Suppress    => _container.Resolve<SuppressState>(),
            EnemyAIState.Dead        => _container.Resolve<DeadState>(),
            _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
        };
    }
}