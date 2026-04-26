using BovineLabs.Timeline.Data.Schedular;
using BovineLabs.Timeline.Schedular;
using Unity.Burst;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(ScheduleSystemGroup))]
    [UpdateAfter(typeof(ClockUpdateSystem))]
    [UpdateBefore(typeof(TimerUpdateSystem))]
    public partial struct TimelineTimeScaleApplySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ApplyTimeScaleJob().ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct ApplyTimeScaleJob : IJobEntity
        {
            private void Execute(ref ClockData clock, in TimelineTimeScaleMultiplier multiplier)
            {
                if (multiplier.Value != 1f)
                {
                    clock.DeltaTime *= (double)multiplier.Value;
                    clock.Scale *= (double)multiplier.Value;
                }
            }
        }
    }
}
