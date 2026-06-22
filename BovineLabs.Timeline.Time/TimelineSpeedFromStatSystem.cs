using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
using BovineLabs.Essence.Data;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.EntityLinks;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateAfter(typeof(TimelineTimeScaleTrackSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TimelineSpeedFromStatSystem : ISystem
    {
        private UnsafeComponentLookup<EntityLinkSource> _sources;
        private UnsafeBufferLookup<EntityLinkEntry> _entries;
        private ComponentLookup<Targets> _targets;
        private BufferLookup<Stat> _stats;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _sources = state.GetUnsafeComponentLookup<EntityLinkSource>(true);
            _entries = state.GetUnsafeBufferLookup<EntityLinkEntry>(true);
            _targets = state.GetComponentLookup<Targets>(true);
            _stats = state.GetBufferLookup<Stat>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _sources.Update(ref state);
            _entries.Update(ref state);
            _targets.Update(ref state);
            _stats.Update(ref state);

            state.Dependency = new ScaleJob
            {
                Sources = _sources,
                Entries = _entries,
                Targets = _targets,
                Stats = _stats
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(TimelineActive))]
        private partial struct ScaleJob : IJobEntity
        {
            [ReadOnly] public UnsafeComponentLookup<EntityLinkSource> Sources;
            [ReadOnly] public UnsafeBufferLookup<EntityLinkEntry> Entries;
            [ReadOnly] public ComponentLookup<Targets> Targets;
            [ReadOnly] public BufferLookup<Stat> Stats;

            private void Execute(Entity entity, in TimelineSpeedFromStat config,
                ref TimelineTimeScaleMultiplier multiplier)
            {
                var targets = Targets.TryGetComponent(entity, out var t) ? t : default;

                var statEntity =
                    EntityLinkResolver.TryResolve(entity, targets, config.ReadRootFrom, config.LinkKey, Sources,
                        Entries, out var linked)
                    && linked != Entity.Null
                        ? linked
                        : targets.Get(config.Fallback, entity);

                var found = false;
                var value = 0f;
                if (statEntity != Entity.Null && Stats.TryGetBuffer(statEntity, out var buffer))
                {
                    found = buffer.AsMap().TryGetValue(config.Stat, out var statValue);
                    value = found ? statValue.ValueFloat : 0f;
                }

                multiplier.Value = StatSpeed.Apply(multiplier.Value, config, found, value);
            }
        }
    }
}