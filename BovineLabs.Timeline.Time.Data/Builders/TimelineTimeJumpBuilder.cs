using BovineLabs.Core.EntityCommands;
using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Timeline.EntityLinks.Data;

namespace BovineLabs.Timeline.Time.Data.Builders
{
    public struct TimelineTimeJumpBuilder
    {
        public EntityLinkRef Listen;
        public ConditionKey Event;
        public float FramesPerSecond;

        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent(new TimelineTimeJumpData
            {
                Listen = Listen,
                Event = Event,
                FramesPerSecond = FramesPerSecond
            });
        }
    }
}
