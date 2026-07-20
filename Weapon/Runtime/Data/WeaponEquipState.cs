namespace ZigdarkS.ProjectB.Weapon.Data
{
    public enum WeaponEquipState
    {
        Ready,       // можно стрелять/целиться/перезаряжаться
        Holstering,  // убираем текущее
        Drawing      // достаём новое
    }
}