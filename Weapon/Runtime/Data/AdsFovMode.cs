namespace ZigdarkS.ProjectB.Weapon.Data
{
    public enum AdsFovMode
    {
        ScalesWithPlayerFov, // обычное оружие: множитель от текущего FOV игрока
        FixedFov             // оптика: конкретный фиксированный FOV независимо от настроек игрока
    }
}