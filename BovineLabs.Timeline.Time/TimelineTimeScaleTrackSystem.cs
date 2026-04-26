using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
using BovineLabs.Core.Jobs;
using BovineLabs.Essence.Data;
using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct TimelineTimeScaleTrackSystem : ISystem
    {
        private TrackBlendImpl<float, TimelineTimeScaleAnimated> blendImpl;
        private UnsafeComponentLookup<TimelineTimeScaleMultiplier> multiplierLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            blendImpl.OnCreate(ref state);
            multiplierLookup = state.GetUnsafeComponentLookup<TimelineTimeScaleMultiplier>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            blendImpl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ResetJob().ScheduleParallel(state.Dependency);
            
            state.Dependency = new PrepareJob
            {
                Stats = SystemAPI.GetBufferLookup<Stat>(true)
            }.ScheduleParallel(state.Dependency);

            multiplierLookup.Update(ref state);
            var blendData = blendImpl.Update(ref state);

            state.Dependency = new WriteMultiplierJob
            {
                BlendData = blendData,
                MultiplierLookup = multiplierLookup
            }.ScheduleParallel(blendData, 64, state.Dependency);
        }

        [BurstCompile]
        private partial struct ResetJob : IJobEntity
        {
            private void Execute(ref TimelineTimeScaleMultiplier multiplier)
            {
                multiplier.Value = 1f;
            }
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct PrepareJob : IJobEntity
        {
            [ReadOnly] public BufferLookup<Stat> Stats;

            private void Execute(ref TimelineTimeScaleAnimated animated)
            {
                if (animated.StatKey.Value != 0 && animated.StatEntity != Entity.Null && this.Stats.TryGetBuffer(animated.StatEntity, out var statsBuffer))
                {
                    animated.Value = statsBuffer.AsMap().GetValueFloat(animated.StatKey);
                }
                else
                {
                    animated.Value = animated.AuthoredData;
                }
            }
        }

        [BurstCompile]
        private struct WriteMultiplierJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<float>>.ReadOnly BlendData;
            [NativeDisableParallelForRestriction] public UnsafeComponentLookup<TimelineTimeScaleMultiplier> MultiplierLookup;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out var entity, out var mixData);
                if (!MultiplierLookup.HasComponent(entity)) return;

                MultiplierLookup[entity] = new TimelineTimeScaleMultiplier
                {
                    Value = JobHelpers.Blend<float, FloatMixer>(ref mixData, 1f)
                };
            }
        }
    }
}