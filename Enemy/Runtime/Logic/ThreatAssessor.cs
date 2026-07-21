namespace ZigdarkS.ProjectB.Enemy.Logic
{
    /// <summary>
    /// Оценивает уровень угрозы на основе текущего состояния врага.
    /// Используется в CombatState, чтобы выбирать поведение (агрессия vs защита).
    /// </summary>
    public static class ThreatAssessor
    {
        public static ThreatLevel Evaluate(EnemyContext context)
        {
            float hp    = context.Model.HPPercent;
            var   cfg   = context.Config;

            if (hp <= cfg.ThreatCriticalHPThreshold)
                return ThreatLevel.Critical;

            if (hp <= cfg.ThreatHighHPThreshold)
                return ThreatLevel.High;

            // Патроны кончились и ещё не начали перезарядку — высокий приоритет укрыться
            if (context.Model.CurrentAmmo == 0)
                return ThreatLevel.High;

            return ThreatLevel.Normal;
        }
    }
}