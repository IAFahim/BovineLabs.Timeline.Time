using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.Time.Authoring
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct TimeScaleBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var binding in SystemAPI.Query<RefRO<TrackBinding>>().WithAll<TimeScaleAnimated>()
                         .WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab))
            {
                var target = binding.ValueRO.Value;
                if (target != Entity.Null && !SystemAPI.HasComponent<ActiveTimeScale>(target))
                {
                    ecb.AddComponent<ActiveTimeScale>(target);
                    ecb.SetComponentEnabled<ActiveTimeScale>(target, false);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
