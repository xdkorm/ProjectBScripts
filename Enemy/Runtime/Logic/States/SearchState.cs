using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public class SearchState : IEnemyState
    {
        public void Enter(EnemyContext context)
        {
            context.SearchElapsed = 0f;
            context.CoverDestination = null;
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            if (context.Player == null)
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            if (context.Perception.CanSeePlayer)
            {
                nextState = EnemyAIState.Combat;
                return;
            }

            context.SearchElapsed += Time.deltaTime;

            if (context.Perception.LastKnownPlayerPosition == null ||
                context.SearchElapsed >= context.Config.SearchDuration)
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            Vector3 searchTarget = context.Perception.LastKnownPlayerPosition.Value;
            float distanceToTarget = Vector3.Distance(context.View.Position, searchTarget);

            if (distanceToTarget <= 1.5f)
            {
                context.View.SetStopped(true);
                return;
            }

            context.Navigator.MoveTo(context, searchTarget, context.Config.StrafeSpeed * 0.8f);
        }

        public void Exit(EnemyContext context)
        {
            context.View.SetStopped(false);
        }
    }
}
