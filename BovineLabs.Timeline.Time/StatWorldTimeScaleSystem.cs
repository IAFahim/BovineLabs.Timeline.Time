using BovineLabs.Core.Extensions;
using BovineLabs.Essence.Data;
using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(WorldTimeScaleSystem))]
    public partial struct StatWorldTimeScaleSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new UpdateScaleJob
            {
                Stats = SystemAPI.GetBufferLookup<Stat>(true)
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct UpdateScaleJob : IJobEntity
        {
            [ReadOnly] public BufferLookup<Stat> Stats;

            private void Execute(in TrackBinding binding, in StatWorldTimeScaleData config, ref WorldTimeScaleAnimated animated)
            {
                if (binding.Value == Entity.Null)
                {
                    return;
                }

                if (this.Stats.TryGetBuffer(binding.Value, out var statsBuffer))
                {
                    animated.Value = statsBuffer.AsMap().GetValueFloat(config.ScaleStatKey);
                }
            }
        }
    }
}
