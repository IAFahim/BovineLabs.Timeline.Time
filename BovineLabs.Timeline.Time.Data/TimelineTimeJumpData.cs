using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Timeline.Data.Schedular;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using Unity.IntegerTime;

namespace BovineLabs.Timeline.Time
{
    public struct TimelineTimeJumpData : IComponentData
    {
        public EntityLinkRef Listen;
        public ConditionKey Event;
        public float FramesPerSecond;
    }

    public static class TimelineTimeJumpMath
    {
        // Clamping into [clipStart, clipEnd] means a clip can never jump itself out of its own active range:
        // it only runs this math while already inside that interval, and Clamp() can't produce a value outside it.
        public static DiscreteTime ComputeTarget(DiscreteTime currentTime, DiscreteTime clipStart, DiscreteTime clipEnd, int frames, float framesPerSecond)
        {
            var deltaSeconds = frames / framesPerSecond;
            var bounds = new DiscreteTimeInterval(clipStart, clipEnd);
            return bounds.Clamp(currentTime + new DiscreteTime(deltaSeconds));
        }
    }
}
