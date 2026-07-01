using BovineLabs.Core;
using BovineLabs.Core.Collections;
using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
using BovineLabs.Reaction.Conditions;
using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Reaction.Groups;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Data.Schedular;
using BovineLabs.Timeline.EntityLinks;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    // Runs before ConditionEventWriteSystem clears the frame's ConditionEvent buffer, in the same
    // Reaction-side group ActionTimelineSystem already uses to force-set a foreign director's Timer.Time.
    [UpdateInGroup(typeof(ConditionWriteEventsGroup))]
    [UpdateBefore(typeof(ConditionEventWriteSystem))]
    public partial struct TimelineTimeJumpSystem : ISystem
    {
        private UnsafeComponentLookup<Targets> _targetsLookup;
        private UnsafeComponentLookup<EntityLinkSource> _linkSourceLookup;
        private UnsafeBufferLookup<EntityLinkEntry> _linkLookup;
        private BufferLookup<ConditionEvent> _conditionEvents;
        private ComponentLookup<EventsDirty> _eventsDirty;
        private ComponentLookup<Timer> _timers;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _targetsLookup = state.GetUnsafeComponentLookup<Targets>(true);
            _linkSourceLookup = state.GetUnsafeComponentLookup<EntityLinkSource>(true);
            _linkLookup = state.GetUnsafeBufferLookup<EntityLinkEntry>(true);
            _conditionEvents = state.GetBufferLookup<ConditionEvent>(true);
            _eventsDirty = state.GetComponentLookup<EventsDirty>(true);
            _timers = state.GetComponentLookup<Timer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _targetsLookup.Update(ref state);
            _linkSourceLookup.Update(ref state);
            _linkLookup.Update(ref state);
            _conditionEvents.Update(ref state);
            _eventsDirty.Update(ref state);
            _timers.Update(ref state);

            state.Dependency = new JumpJob
            {
                TargetsLookup = _targetsLookup,
                LinkSources = _linkSourceLookup,
                Links = _linkLookup,
                ConditionEvents = _conditionEvents,
                EventsDirty = _eventsDirty,
                Timers = _timers
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct JumpJob : IJobEntity
        {
            [ReadOnly] public UnsafeComponentLookup<Targets> TargetsLookup;
            [ReadOnly] public UnsafeComponentLookup<EntityLinkSource> LinkSources;
            [ReadOnly] public UnsafeBufferLookup<EntityLinkEntry> Links;
            [ReadOnly] public BufferLookup<ConditionEvent> ConditionEvents;
            [ReadOnly] public ComponentLookup<EventsDirty> EventsDirty;
            public ComponentLookup<Timer> Timers;

            private void Execute(in TrackBinding binding, in TimelineTimeJumpData data, in TimeTransform timeTransform, in DirectorRoot directorRoot)
            {
                if (data.Event == ConditionKey.Null || binding.Value == Entity.Null ||
                    !TargetsLookup.TryGetComponent(binding.Value, out var targets))
                {
                    return;
                }

                if (!EntityLinkResolver.TryResolve(binding.Value, targets, data.ListenOn, data.ListenLinkKey, LinkSources, Links, out var target))
                {
                    target = targets.Get(data.ListenOn, binding.Value);
                }

                // The enableable EventsDirty flag is the non-polling gate: it's only set on frames something was
                // actually written via ConditionEventWriter.Trigger, so we only touch the hashmap when it fired.
                if (target == Entity.Null || !ConditionEvents.HasBuffer(target) || !EventsDirty.HasComponent(target) ||
                    !EventsDirty.IsComponentEnabled(target))
                {
                    return;
                }

                if (!ConditionEvents[target].AsMap().TryGetValue(data.Event, out var frames) || frames == 0)
                {
                    return;
                }

                ref var timer = ref Timers.GetRefRW(directorRoot.Director).ValueRW;
                timer.Time = TimelineTimeJumpMath.ComputeTarget(timer.Time, timeTransform.Start, timeTransform.End, frames, data.FramesPerSecond);
            }
        }
    }
}
