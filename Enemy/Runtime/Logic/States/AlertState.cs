using UnityEngine;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    /// <summary>
    /// Враг услышал звук и осторожно идёт к его источнику.
    /// Переходит в Combat при виде игрока, в Idle если ничего не нашёл.
    /// </summary>
    public class AlertState : IEnemyState
    {
        private float _elapsed;

        public void Enter(EnemyContext context)
        {
            _elapsed = 0f;
            // Двигаемся медленно — осторожно
            context.View.SetStopped(false);
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            if (context.Player == null)
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            // Увидел — в бой
            if (context.Perception.CanSeePlayer)
            {
                context.SoundPerception?.Consume();
                nextState = EnemyAIState.Combat;
                return;
            }

            // Нет звука для расследования — уходим
            if (context.SoundPerception == null)
            {
                nextState = EnemyAIState.Idle;
                return;
            }

            _elapsed += Time.deltaTime;

            // Время расследования вышло
            if (_elapsed >= context.Config.AlertDuration)
            {
                context.SoundPerception.Consume();
                nextState = context.IsStateAvailable(EnemyAIState.Search)
                    ? EnemyAIState.Search
                    : EnemyAIState.Idle;
                return;
            }

            // Идём к точке звука
            Vector3 target = context.SoundPerception.LastHeardSoundPosition;
            float   dist   = Vector3.Distance(context.View.Position, target);

            if (dist <= 1.5f)
            {
                // Дошли — стоим, осматриваемся
                context.View.SetStopped(true);
            }
            else
            {
                context.Navigator.MoveTo(context, target, context.Config.StrafeSpeed * 0.6f);
            }
        }

        public void Exit(EnemyContext context)
        {
            context.View.SetStopped(false);
        }
    }
}