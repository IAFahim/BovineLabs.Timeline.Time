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
            // Only ever subdivide the fixed step (slow-mo), never enlarge it: a fast-forward scale (>1) would
            // grow the physics step and tunnel colliders / destabilize constraints. min(1, scale) keeps
            // slow-mo identical while fast-forward runs more sub-steps at the base step.
            var scaled = worldScale.ScaleFixedDeltaTime ? floored * math.min(1f, ResolveScale(worldScale)) : floored;
            return math.max(MinFixedDeltaTime, scaled);
        }
    }
}
