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
    [Unity.Entities.WorldSystemFilter(Unity.Entities.WorldSystemFilterFlags.LocalSimulation | Unity.Entities.WorldSystemFilterFlags.ClientSimulation | Unity.Entities.WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TimelineTimeScaleApplySystem : ISystem
    {
        private ComponentLookup<HitStopState> hitStopsLookup;
        private ComponentLookup<HitStopRemainingTime> remainingLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            hitStopsLookup = state.GetComponentLookup<HitStopState>(true);
            remainingLookup = state.GetComponentLookup<HitStopRemainingTime>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            hitStopsLookup.Update(ref state);
            remainingLookup.Update(ref state);

            state.Dependency = new ApplyTimeScaleJob
            {
                HitStops = hitStopsLookup,
                Remaining = remainingLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct ApplyTimeScaleJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<HitStopState> HitStops;
            [ReadOnly] public ComponentLookup<HitStopRemainingTime> Remaining;

            private void Execute(Entity entity, ref ClockData clock, in TimelineTimeScaleMultiplier multiplier)
            {
                var timeScale = multiplier.Value;

                if (HitStops.TryGetComponent(entity, out var hitStop) &&
                    HitStops.IsComponentEnabled(entity) &&
                    Remaining.TryGetComponent(entity, out var remaining) &&
                    remaining.Value > 0f)
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