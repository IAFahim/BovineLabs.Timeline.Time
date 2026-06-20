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
    using Unity.Mathematics;

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
            this._sources = state.GetUnsafeComponentLookup<EntityLinkSource>(true);
            this._entries = state.GetUnsafeBufferLookup<EntityLinkEntry>(true);
            this._targets = state.GetComponentLookup<Targets>(true);
            this._stats = state.GetBufferLookup<Stat>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this._sources.Update(ref state);
            this._entries.Update(ref state);
            this._targets.Update(ref state);
            this._stats.Update(ref state);

            state.Dependency = new ScaleJob
            {
                Sources = this._sources,
                Entries = this._entries,
                Targets = this._targets,
                Stats = this._stats,
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
                    found = buffer.AsMap().TryGetValue(map.Stat, out var sv);
                    value = found ? sv.ValueFloat : 0f;
                }

                multiplier.Value *= math.max(StatSpeed.Resolve(map, found, value), StatSpeed.MinMultiplier);

                // Hard-floor the COMPOUNDED multiplier too: per-factor flooring keeps each factor positive
                // but a product of several factors could still dip below the safety floor. 0.05 is the
                // package's safety floor (matches TimelineTimeScaleTrackSystem), so the timeline never
                // approaches a frozen clock regardless of how many speed _sources stack.
                multiplier.Value = math.max(multiplier.Value, StatSpeed.MinMultiplier);
            }
        }
    }
}
