namespace ZigdarkS.ProjectB.Enemy.Logic
{
    public enum ThreatLevel
    {
        /// <summary>Всё хорошо — наступаем, атакуем агрессивно.</summary>
        Normal,

        /// <summary>HP низкий или патроны почти кончились — осторожнее.</summary>
        High,

        /// <summary>Критически мало HP — в укрытие, стреляем редко.</summary>
        Critical,
    }
}