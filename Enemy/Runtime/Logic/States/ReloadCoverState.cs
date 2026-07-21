using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public class ReloadCoverState : IEnemyState
    {
        public void Enter(EnemyContext context)
        {
            context.CoverDestination = null;
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            if (context.Player == null)
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            if (!context.Model.IsReloading)
            {
                nextState = context.Perception.CanSeePlayer
                    ? EnemyAIState.Combat
                    : EnemyAIState.Chase;
                return;
            }

            context.Navigator.ExecuteCoverBehavior(
                context,
                context.Player.EyesPosition,
                context.Perception.DistanceToPlayer);
        }

        public void Exit(EnemyContext context)
        {
            context.CoverDestination = null;
        }
    }
}
