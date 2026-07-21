using ZigdarkS.ProjectB.Enemy.Data;
using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    /// <summary>
    /// Роль в отряде: стоит за укрытием и ведёт подавляющий огонь
    /// пока напарник (Flanker) обходит с фланга.
    /// Выходит в Combat как только роль перестаёт быть Suppressor.
    /// </summary>
    public class SuppressState : IEnemyState
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

            // Вышли из роли Suppressor — возвращаемся в обычный бой
            if (context.SquadRole != SquadRole.Suppressor)
            {
                nextState = EnemyAIState.Combat;
                return;
            }

            // Патроны кончились — перезарядка важнее роли
            if (context.Model.CurrentAmmo <= 0 && !context.Model.IsReloading)
            {
                context.Combat.StartReload(context);
                nextState = context.IsStateAvailable(EnemyAIState.ReloadCover)
                    ? EnemyAIState.ReloadCover
                    : EnemyAIState.Combat;
                return;
            }

            // Двигаемся в укрытие и стреляем из него
            Vector3 playerPos = context.Player.EyesPosition;
            float   distance  = context.Perception.DistanceToPlayer;

            context.Navigator.ExecuteCoverBehavior(context, playerPos, distance);

            if (context.Perception.CanSeePlayer)
                context.Combat.TryShoot(context, context.Config.LongFireRate, playerPos);
        }

        public void Exit(EnemyContext context)
        {
            context.CoverDestination = null;
        }
    }
}