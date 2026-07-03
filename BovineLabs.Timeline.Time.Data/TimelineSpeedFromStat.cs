using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Time
{
    public struct TimelineSpeedFromStat : IComponentData
    {
        public StatSource Source;
        public Target Fallback;
        public float Min;
        public float Max;
        public float Default;
    }

    public static class StatSpeed
    {
        public const float MinMultiplier = 0.05f;

        public static float Resolve(in TimelineSpeedFromStat config, bool found, float stat)
        {
            return found ? math.clamp(stat, config.Min, config.Max) : config.Default;
        }

        public static float Floor(float value)
        {
            return math.max(value, MinMultiplier);
        }

        public static float Apply(float incoming, in TimelineSpeedFromStat config, bool found, float stat)
        {
            return Floor(incoming * Floor(Resolve(config, found, stat)));
        }
    }
}