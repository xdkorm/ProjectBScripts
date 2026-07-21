using UnityEngine;

namespace ZigdarkS.ProjectB.Core.Combat
{
    /// <summary>
    /// Контракт для любого объекта, который может быть целью AI.
    /// Enemy зависит только от этого интерфейса — не от конкретного PlayerView.
    /// </summary>
    public interface ITargetable
    {
        Vector3 EyesPosition { get; }
        GameObject gameObject { get; }
    }
}
