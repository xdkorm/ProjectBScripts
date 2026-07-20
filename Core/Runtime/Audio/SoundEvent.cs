using UnityEngine;

namespace ZigdarkS.ProjectB.Core
{
    /// <summary>
    /// Звуковое событие, которое распространяется через ISoundEventBus.
    /// Подними из любого места: выстрел игрока, шаги, взрыв — враги отреагируют.
    /// </summary>
    public readonly struct SoundEvent
    {
        /// <summary>Точка в мировом пространстве, откуда исходит звук.</summary>
        public readonly Vector3 Origin;

        /// <summary>Максимальный радиус слышимости этого звука.</summary>
        public readonly float Radius;

        /// <summary>Тип звука для тонкой настройки реакции.</summary>
        public readonly SoundType Type;

        public SoundEvent(Vector3 origin, float radius, SoundType type)
        {
            Origin = origin;
            Radius = radius;
            Type   = type;
        }
    }
}
