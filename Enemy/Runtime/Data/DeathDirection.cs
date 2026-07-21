namespace ZigdarkS.ProjectB.Enemy.Data
{
    /// <summary>
    /// Направление, с которого враг получил смертельный удар, относительно его forward.
    /// Значение int этого enum пробрасывается в Animator как параметр "DeathDirection"
    /// и используется для выбора клипа Death From Front/Back/Left/Right.
    /// </summary>
    public enum DeathDirection
    {
        Front = 0,
        Back  = 1,
        Left  = 2,
        Right = 3
    }
}