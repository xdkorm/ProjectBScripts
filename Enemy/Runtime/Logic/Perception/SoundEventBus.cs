using System;
using ZigdarkS.ProjectB.Core.Audio;

namespace ZigdarkS.ProjectB.Enemy.Logic.Perception
{
    /// <summary>
    /// Зарегистрируй как Singleton в VContainer:
    ///   builder.Register&lt;ISoundEventBus, SoundEventBus&gt;(Lifetime.Singleton);
    ///
    /// Откуда вызывать Raise():
    ///   - PlayerShooting.OnShoot → Raise(new SoundEvent(pos, 40f, SoundType.Gunshot))
    ///   - PlayerMovement.OnFootstep → Raise(new SoundEvent(pos, 8f, SoundType.Footstep))
    ///   - EnemyCombatService.TryShoot → уже интегрировано
    /// </summary>
    public class SoundEventBus : ISoundEventBus
    {
        private event Action<SoundEvent> _onSoundRaised;

        public void Raise(SoundEvent soundEvent)
        {
            _onSoundRaised?.Invoke(soundEvent);
        }

        public void Subscribe(Action<SoundEvent> listener)
        {
            _onSoundRaised += listener;
        }

        public void Unsubscribe(Action<SoundEvent> listener)
        {
            _onSoundRaised -= listener;
        }
    }
}