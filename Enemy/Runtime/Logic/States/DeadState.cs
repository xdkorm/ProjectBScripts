using ZigdarkS.ProjectB.Enemy.Data;
using ZigdarkS.ProjectB.Enemy.Logic.Combat;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public class DeadState : IEnemyState
    {
        public void Enter(EnemyContext context)
        {
            context.View.HideLaser();
            context.View.SetStopped(true);

            DeathDirection direction = DeathDirectionCalculator.Calculate(
                context.View.Forward,
                context.View.Right,
                context.Model.LastHitDirection);

            // Важно: выставляем ДО того как в этом же тике EnemyBrain.Tick() вызовет
            // TickAnimator(..., isDead: true) — тогда Any State -> Death в контроллере
            // сразу свернёт в нужный под-стейт (Front/Back/Left/Right).
            context.View.SetDeathDirection(direction);

            // Рэгдолл здесь НЕ включается напрямую.
            // Его включает Animation Event (EnemyAnimator.Anim_ActivateRagdoll),
            // проставленный на нужном кадре клипа смерти в Animator Controller —
            // так тело обмякает синхронно с анимацией, а не телепортируется в рэгдолл мгновенно.
        }

        public void Update(EnemyContext context, ref EnemyAIState nextState)
        {
            // В смерти делать нечего, ждём Animation Event из клипа.
        }

        public void Exit(EnemyContext context)
        {
        }
    }
}