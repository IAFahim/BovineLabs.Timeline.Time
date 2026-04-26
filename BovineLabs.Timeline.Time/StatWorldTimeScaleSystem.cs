using BovineLabs.Essence.Data;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.EntityLinks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using EntityLink = BovineLabs.Timeline.EntityLinks.Data.EntityLink;
using EntityLinkSource = BovineLabs.Timeline.EntityLinks.Data.EntityLinkSource;

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
                TargetsLookup = SystemAPI.GetComponentLookup<Targets>(true),
                TargetsCustoms = SystemAPI.GetComponentLookup<TargetsCustom>(true),
                Sources = SystemAPI.GetComponentLookup<EntityLinkSource>(true),
                Links = SystemAPI.GetBufferLookup<EntityLink>(true),
                Stats = SystemAPI.GetBufferLookup<Stat>(true)
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct UpdateScaleJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Targets> TargetsLookup;
            [ReadOnly] public ComponentLookup<TargetsCustom> TargetsCustoms;
            [ReadOnly] public ComponentLookup<EntityLinkSource> Sources;
            [ReadOnly] public BufferLookup<EntityLink> Links;
            [ReadOnly] public BufferLookup<Stat> Stats;

            private void Execute(in TrackBinding binding, in StatWorldTimeScaleData config,
                ref WorldTimeScaleAnimated animated)
            {
                if (binding.Value == Entity.Null ||
                    !TargetsLookup.TryGetComponent(binding.Value, out var targets)) return;

                if (!EntityLinkResolver.TryResolve(
                        binding.Value,
                        targets,
                        config.ReadRootFrom,
                        config.LinkKey,
                        TargetsCustoms,
                        Sources,
                        Links,
                        out var target))
                    return;

                if (Stats.TryGetBuffer(target, out var statsBuffer))
                    animated.Value = statsBuffer.AsMap().GetValueFloat(config.ScaleStatKey);
            }
        }
    }
}