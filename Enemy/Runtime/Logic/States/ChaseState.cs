using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public class ChaseState : IEnemyState
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

            if (context.Model.IsReloading)
            {
                nextState = EnemyAIState.ReloadCover;
                return;
            }

            if (context.Perception.CanSeePlayer)
            {
                nextState = EnemyAIState.Combat;
                return;
            }

            if (context.Perception.TimeSinceLastSeen >= context.Config.LostSightSearchDelay)
            {
                nextState = EnemyAIState.Search;
                return;
            }

            if (!context.Perception.IsPlayerInAggroRange(context.Config.AggroRange))
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            Vector3 target = context.Perception.LastKnownPlayerPosition ?? context.Player.EyesPosition;
            context.Navigator.MoveTo(context, target, context.Config.StrafeSpeed);
        }

        public void Exit(EnemyContext context)
        {
        }
    }
}
