namespace BovineLabs.Timeline.Time
{
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

    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateAfter(typeof(TimelineTimeScaleTrackSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TimelineSpeedFromStatSystem : ISystem
    {
        private UnsafeComponentLookup<EntityLinkSource> sources;
        private UnsafeBufferLookup<EntityLinkEntry> entries;
        private ComponentLookup<Targets> targets;
        private BufferLookup<Stat> stats;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.sources = state.GetUnsafeComponentLookup<EntityLinkSource>(true);
            this.entries = state.GetUnsafeBufferLookup<EntityLinkEntry>(true);
            this.targets = state.GetComponentLookup<Targets>(true);
            this.stats = state.GetBufferLookup<Stat>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.sources.Update(ref state);
            this.entries.Update(ref state);
            this.targets.Update(ref state);
            this.stats.Update(ref state);

            state.Dependency = new ScaleJob
            {
                Sources = this.sources,
                Entries = this.entries,
                Targets = this.targets,
                Stats = this.stats,
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

            private void Execute(Entity entity, in TimelineSpeedFromStat map, ref TimelineTimeScaleMultiplier multiplier)
            {
                var resolvedTargets = this.Targets.TryGetComponent(entity, out var t) ? t : default;

                var statEntity =
                    EntityLinkResolver.TryResolve(entity, resolvedTargets, map.ReadRootFrom, map.LinkKey, this.Sources, this.Entries, out var linked)
                    && linked != Entity.Null
                        ? linked
                        : resolvedTargets.Get(map.Fallback, entity);

                var found = false;
                var value = 0f;
                if (statEntity != Entity.Null && this.Stats.TryGetBuffer(statEntity, out var buffer))
                {
                    found = true;
                    value = buffer.GetValueFloat(map.Stat);
                }

                multiplier.Value *= StatSpeed.Resolve(map, found, value);
            }
        }
    }
}
