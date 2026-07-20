using System;
using UnityEngine;
using ZigdarkS.ProjectB.Core;
using ZigdarkS.ProjectB.Enemy.Data;

namespace ZigdarkS.ProjectB.Enemy.Logic.Perception
{
    /// <summary>
    /// Слух одного врага. Радиус слышимости берётся из EnemyConfig по типу звука —
    /// каждый тип (шаги/выстрел/взрыв/смерть) имеет свой ползунок в инспекторе.
    /// </summary>
    public class EnemySoundPerception : IDisposable
    {
        private readonly ISoundEventBus _soundBus;
        private readonly EnemyConfig    _config;
        private readonly Action<SoundEvent> _listener;

        private Vector3 _position;

        public bool      HasHeardSound          { get; private set; }
        public Vector3   LastHeardSoundPosition  { get; private set; }
        public SoundType LastHeardSoundType      { get; private set; }

        public EnemySoundPerception(ISoundEventBus soundBus, EnemyConfig config)
        {
            _soundBus = soundBus;
            _config   = config;
            _listener = OnSoundReceived;
        }

        public void Initialize()
        {
            _soundBus.Subscribe(_listener);
        }

        public void UpdatePosition(Vector3 position)
        {
            _position = position;
        }

        /// <summary>
        /// Сбрасывает флаг после обработки звука.
        /// Вызывай в AlertState.Exit() или когда звук «использован».
        /// </summary>
        public void Consume()
        {
            HasHeardSound = false;
        }

        public void Dispose()
        {
            _soundBus.Unsubscribe(_listener);
        }

        private void OnSoundReceived(SoundEvent evt)
        {
            if (HasHeardSound) return;

            // Радиус конкретного типа звука, ограниченный общим слухом врага
            float effectiveRadius = Mathf.Min(evt.Radius, _config.GetHearRadius(evt.Type));
            float distance        = Vector3.Distance(_position, evt.Origin);

            if (distance <= effectiveRadius)
            {
                HasHeardSound          = true;
                LastHeardSoundPosition = evt.Origin;
                LastHeardSoundType     = evt.Type;
            }
        }
    }
}