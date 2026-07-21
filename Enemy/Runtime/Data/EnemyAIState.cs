namespace ZigdarkS.ProjectB.Enemy.Data
{
    public enum EnemyAIState
    {
        Idle,
        Patrol,       // обход точек маршрута
        Chase,
        Combat,
        ReloadCover,
        Search,
        Alert,        // расследует звук
        Suppress,     // подавляющий огонь (роль в отряде)
        Dead,
    }
}