using BovineLabs.Core.EntityCommands;
using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Reaction.Data.Core;

namespace BovineLabs.Timeline.Time.Data.Builders
{
    public struct TimelineTimeJumpBuilder
    {
        public Target ListenOn;
        public ushort ListenLinkKey;
        public ConditionKey Event;
        public float FramesPerSecond;

        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent(new TimelineTimeJumpData
            {
                ListenOn = ListenOn,
                ListenLinkKey = ListenLinkKey,
                Event = Event,
                FramesPerSecond = FramesPerSecond
            });
        }
    }
}
