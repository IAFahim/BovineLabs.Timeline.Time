namespace BovineLabs.Timeline.Time
{
    using BovineLabs.Essence.Data;
    using BovineLabs.Reaction.Data.Core;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct TimelineSpeedFromStat : IComponentData
    {
        public Target ReadRootFrom;
        public ushort LinkKey;
        public Target Fallback;
        public StatKey Stat;
        public float Min;
        public float Max;
        public float Default;
    }

    public static class StatSpeed
    {
        public const float MinMultiplier = 0.05f;

        public static float Resolve(in TimelineSpeedFromStat map, bool found, float stat)
        {
            return found ? math.clamp(stat, map.Min, map.Max) : map.Default;
        }
    }
}
