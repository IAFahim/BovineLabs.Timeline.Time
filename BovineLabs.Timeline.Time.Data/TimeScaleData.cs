using BovineLabs.Timeline.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;

namespace BovineLabs.Timeline.Time
{
    public struct TimeScaleData
    {
        public float Scale;
    }

    public struct TimeScaleAnimated : IAnimatedComponent<TimeScaleData>
    {
        public TimeScaleData AuthoredData;
        [CreateProperty] public TimeScaleData Value { get; set; }
    }

    public struct ActiveTimeScale : IComponentData, IEnableableComponent
    {
        public TimeScaleData Config;
    }

    public readonly struct TimeScaleMixer : IMixer<TimeScaleData>
    {
        public TimeScaleData Lerp(in TimeScaleData a, in TimeScaleData b, in float s)
        {
            return new TimeScaleData
            {
                Scale = math.lerp(a.Scale, b.Scale, s)
            };
        }

        public TimeScaleData Add(in TimeScaleData a, in TimeScaleData b)
        {
            return new TimeScaleData
            {
                Scale = a.Scale + b.Scale
            };
        }
    }
}
