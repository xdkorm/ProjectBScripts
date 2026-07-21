using System;

namespace ZigdarkS.ProjectB.Core.Audio
{
    public interface ISoundEventBus
    {
        void Raise(SoundEvent soundEvent);
        void Subscribe(Action<SoundEvent> listener);
        void Unsubscribe(Action<SoundEvent> listener);
    }
}
