namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public enum SquadRole
    {
        /// <summary>Нет отряда или роль не назначена.</summary>
        None,

        /// <summary>Стандартный бой: стрейф, флангирование, наступление.</summary>
        Attacker,

        /// <summary>Ведёт подавляющий огонь из укрытия пока фланкер обходит.</summary>
        Suppressor,

        /// <summary>Агрессивно заходит с фланга под прикрытием Suppressor.</summary>
        Flanker,
    }
}