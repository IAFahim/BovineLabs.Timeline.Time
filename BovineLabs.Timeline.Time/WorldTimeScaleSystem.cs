using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct WorldTimeScaleSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<WorldTimeScale>()) return;

            var accum = new NativeReference<float2>(state.WorldUpdateAllocator);

            state.Dependency = new AccumulateJob
            {
                Accumulator = accum
            }.Schedule(state.Dependency);

            state.Dependency = new ApplyJob
            {
                Accumulator = accum
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct AccumulateJob : IJobEntity
        {
            public NativeReference<float2> Accumulator;

            private void Execute(in WorldTimeScaleAnimated clipData, in ClipWeight weight)
            {
                var val = Accumulator.Value;
                val.x += clipData.Value * weight.Value;
                val.y += weight.Value;
                Accumulator.Value = val;
            }
        }

        [BurstCompile]
        private partial struct ApplyJob : IJobEntity
        {
            [ReadOnly] public NativeReference<float2> Accumulator;

            private void Execute(ref WorldTimeScale worldScale)
            {
                var val = Accumulator.Value;
                if (val.y > 0f)
                {
                    worldScale.ActiveScale = val.x / val.y;
                    worldScale.IsActive = true;
                }
                else
                {
                    worldScale.IsActive = false;
                }
            }
        }
    }
}