using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
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
        private UnsafeComponentLookup<HitStopState> hitStopsLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            hitStopsLookup = state.GetUnsafeComponentLookup<HitStopState>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            hitStopsLookup.Update(ref state);

            state.Dependency = new ApplyTimeScaleJob
            {
                HitStops = hitStopsLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct ApplyTimeScaleJob : IJobEntity
        {
            [ReadOnly] public UnsafeComponentLookup<HitStopState> HitStops;

            private void Execute(Entity entity, ref ClockData clock, in TimelineTimeScaleMultiplier multiplier)
            {
                var timeScale = multiplier.Value;

                if (HitStops.TryGetComponent(entity, out var hitStop) &&
                    HitStops.IsComponentEnabled(entity) &&
                    hitStop.RemainingTime > 0f)
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