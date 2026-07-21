using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.States
{
    public interface IEnemyState
    {
        void Enter(EnemyContext context);
        void Update(EnemyContext context, ref EnemyAIState nextState);
        void Exit(EnemyContext context);
    }
}