using BovineLabs.HitStop.Data;
using BovineLabs.Timeline.Data.Schedular;
using BovineLabs.Timeline.Schedular;
using Unity.Burst;
using Unity.Collections;
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
            state.Dependency = new ApplyTimeScaleJob
            {
                HitStops = SystemAPI.GetComponentLookup<HitStopState>(true)
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct ApplyTimeScaleJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<HitStopState> HitStops;

            private void Execute(Entity entity, ref ClockData clock, in TimelineTimeScaleMultiplier multiplier)
            {
                var timeScale = multiplier.Value;

                if (HitStops.HasComponent(entity) &&
                    HitStops.IsComponentEnabled(entity) &&
                    HitStops[entity].RemainingTime > 0f)
                    timeScale = 0.0001f;

                if (timeScale != 1f)
                {
                    clock.DeltaTime *= (double)timeScale;
                    clock.Scale *= timeScale;
                }
            }
        }
    }
}