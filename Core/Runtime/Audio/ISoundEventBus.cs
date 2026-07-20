using System;

namespace ZigdarkS.ProjectB.Core
{
    public interface ISoundEventBus
    {
        void Raise(SoundEvent soundEvent);
        void Subscribe(Action<SoundEvent> listener);
        void Unsubscribe(Action<SoundEvent> listener);
    }
}
