using ZigdarkS.ProjectB.Weapon.Logic;

namespace ZigdarkS.ProjectB.Player.Logic
{
    /// <summary>
    /// Считает итоговый FOV камеры при прицеливании на основе статов оружия
    /// (уже включающих модификаторы модулей) и базового FOV игрока.
    /// </summary>
    public interface IFovCalculator
    {
        float Calculate(IWeapon activeWeapon, float baseFov);
    }
}