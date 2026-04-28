using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct WorldTimeScaleSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<WorldTimeScale>(out var worldScale)) return;

            var totalWeight = 0f;
            var blendedScale = 0f;

            foreach (var (clipData, weight) in SystemAPI.Query<RefRO<WorldTimeScaleAnimated>, RefRO<ClipWeight>>()
                         .WithAll<ClipActive>())
            {
                blendedScale += clipData.ValueRO.Value * weight.ValueRO.Value;
                totalWeight += weight.ValueRO.Value;
            }

            if (totalWeight > 0f)
            {
                worldScale.ValueRW.ActiveScale = blendedScale / totalWeight;
                worldScale.ValueRW.IsActive = true;
            }
            else
            {
                worldScale.ValueRW.IsActive = false;
            }
        }
    }
}
