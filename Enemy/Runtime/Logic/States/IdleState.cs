using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.Navigation;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public class IdleState : IEnemyState
    {
        public void Enter(EnemyContext context)
        {
            context.View.SetStopped(true);
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            if (context.Player == null) return;

            // Слух → Alert
            if (context.SoundPerception != null
                && context.SoundPerception.HasHeardSound
                && context.IsStateAvailable(EnemyAIState.Alert))
            {
                nextState = EnemyAIState.Alert;
                return;
            }

            // Увидел / услышал шаги → Chase или Combat
            if (context.Perception.IsPlayerInAggroRange(context.Config.AggroRange))
            {
                nextState = context.IsStateAvailable(EnemyAIState.Chase)
                    ? EnemyAIState.Chase
                    : EnemyAIState.Combat;
                return;
            }

            // Есть маршрут → Patrol
            var path = context.View.GetComponent<EnemyPatrolPath>();
            if (path != null && path.HasPath && context.IsStateAvailable(EnemyAIState.Patrol))
            {
                nextState = EnemyAIState.Patrol;
            }
        }

        public void Exit(EnemyContext context)
        {
            context.View.SetStopped(false);
        }
    }
}