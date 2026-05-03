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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldTimeScale>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mix = new NativeReference<MixData<float>>(state.WorldUpdateAllocator)
            {
                Value = new MixData<float> { Weights = float4.zero }
            };

            state.Dependency = new AccumulateJob
            {
                Mix = mix
            }.Schedule(state.Dependency);

            state.Dependency = new ApplyJob
            {
                Mix = mix
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct AccumulateJob : IJobEntity
        {
            public NativeReference<MixData<float>> Mix;

            private void Execute(in WorldTimeScaleAnimated clipData, in ClipWeight weight)
            {
                var mix = Mix.Value;
                AddWeighted(ref mix, clipData.Value, weight.Value);
                Mix.Value = mix;
            }

            private static void AddWeighted(ref MixData<float> mix, float value, float weight)
            {
                if (weight <= math.EPSILON)
                {
                    return;
                }

                if (weight > mix.Weights.x)
                {
                    mix.Weights = mix.Weights.xxyz;
                    mix.Weights.x = weight;
                    mix.Value4 = mix.Value3;
                    mix.Value3 = mix.Value2;
                    mix.Value2 = mix.Value1;
                    mix.Value1 = value;
                }
                else if (weight > mix.Weights.y)
                {
                    mix.Weights = mix.Weights.xxyz;
                    mix.Weights.y = weight;
                    mix.Value4 = mix.Value3;
                    mix.Value3 = mix.Value2;
                    mix.Value2 = value;
                }
                else if (weight > mix.Weights.z)
                {
                    mix.Weights = mix.Weights.xyyz;
                    mix.Weights.z = weight;
                    mix.Value4 = mix.Value3;
                    mix.Value3 = value;
                }
                else if (weight > mix.Weights.w)
                {
                    mix.Weights.w = weight;
                    mix.Value4 = value;
                }
            }
        }

        [BurstCompile]
        private partial struct ApplyJob : IJobEntity
        {
            [ReadOnly] public NativeReference<MixData<float>> Mix;

            private void Execute(ref WorldTimeScale worldScale)
            {
                var mix = Mix.Value;
                worldScale.ActiveScale = JobHelpers.Blend<float, FloatMixer>(ref mix, worldScale.DefaultScale);
                worldScale.IsActive = mix.Weights.x > math.EPSILON;
            }
        }
    }
}