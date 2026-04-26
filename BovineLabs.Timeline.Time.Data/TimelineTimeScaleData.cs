using BovineLabs.Essence.Data;
using BovineLabs.Timeline.Data;
using Unity.Entities;
using Unity.Properties;

namespace BovineLabs.Timeline.Time
{
    public struct TimelineTimeScaleMultiplier : IComponentData
    {
        public float Value;
    }

    public struct TimelineTimeScaleAnimated : IAnimatedComponent<float>
    {
        public float AuthoredData;
        public StatKey StatKey;
        [CreateProperty] public float Value { get; set; }
    }
}
