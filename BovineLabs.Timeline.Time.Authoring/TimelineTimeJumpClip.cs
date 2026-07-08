using BovineLabs.Core.Authoring.EntityCommands;
using BovineLabs.Reaction.Authoring.Conditions;
using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.EntityLinks.Authoring;
using BovineLabs.Timeline.Time.Data.Builders;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    public sealed class TimelineTimeJumpClip : DOTSClip, ITimelineClipAsset
    {
        [Tooltip("Optional link key; re-routes from the resolved listen target to its linked entity.")]
        public EntityLinkSchema listenLink;

        [Tooltip("Which entity carries the incoming scrub event: the bound actor (Self) or one of its Targets slots.")]
        public Target listenOn = Target.Self;

        [Tooltip("The condition event whose signed value is the number of frames to jump: negative rewinds, positive advances.")]
        public ConditionEventObject scrubEvent;

        [Tooltip("Frames-per-second used to convert the event's frame count to seconds.")]
        public float framesPerSecond = 60f;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var builder = new TimelineTimeJumpBuilder
            {
                Listen = EntityLinkAuthoringUtility.BakeRef(context.Baker, listenLink, listenOn),
                Event = scrubEvent ? new ConditionKey(scrubEvent.Key) : ConditionKey.Null,
                FramesPerSecond = framesPerSecond < 1f ? 1f : framesPerSecond
            };
            var commands = new BakerCommands(context.Baker, clipEntity);
            builder.ApplyTo(ref commands);

            base.Bake(clipEntity, context);
        }
    }
}
