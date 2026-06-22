using Unity.Mathematics;

namespace BovineLabs.Timeline.Time
{
    public static class WorldTimeScaleResolve
    {
        public const float MinFixedDeltaTime = 0.0001f;

        public static float ResolveScale(in WorldTimeScale worldScale)
        {
            return worldScale.IsActive ? worldScale.ActiveScale : worldScale.DefaultScale;
        }

        public static float ResolveFixedDeltaTime(float baseFixedDeltaTime, in WorldTimeScale worldScale)
        {
            var floored = math.max(MinFixedDeltaTime, baseFixedDeltaTime);
            var scaled = worldScale.ScaleFixedDeltaTime ? floored * ResolveScale(worldScale) : floored;
            return math.max(MinFixedDeltaTime, scaled);
        }
    }
}
