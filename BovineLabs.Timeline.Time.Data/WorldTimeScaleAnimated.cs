using BovineLabs.Timeline.Data;
using Unity.Properties;

namespace BovineLabs.Timeline.Time
{
    public struct WorldTimeScaleAnimated : IAnimatedComponent<float>
    {
        public float AuthoredData;
        [CreateProperty] public float Value { get; set; }
    }
}