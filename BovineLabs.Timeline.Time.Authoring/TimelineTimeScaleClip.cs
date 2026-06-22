using BovineLabs.Core.Authoring.EntityCommands;
using BovineLabs.Essence.Authoring;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Time.Data.Builders;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    public class TimelineTimeScaleClip : DOTSClip, ITimelineClipAsset
    {
        [Tooltip("Time scale multiplier for this timeline instance. 0.5 = Half speed.")]
        public float timeScale = 0.5f;

        [Tooltip("Optional stat to override the time scale. If set, timeScale field is ignored.")]
        public StatSchemaObject stat;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.DependsOn(stat);

            var statEntity = context.Binding != null ? context.Binding.Target : Entity.Null;
            if (stat != null && statEntity == Entity.Null)
            {
                Debug.LogWarning(
                    $"TimelineTimeScaleClip has a stat override ('{stat.name}') but its track is not bound to a StatAuthoring. " +
                    "The stat override will be ignored and the timeline will run at the static timeScale value. " +
                    "Bind the TimelineTimeScaleTrack to a StatAuthoring object to use the stat override.",
                    stat);
            }

            var builder = new TimelineTimeScaleBuilder
            {
                AuthoredData = timeScale,
                StatKey = stat != null ? stat.Key : default,
                StatEntity = statEntity
            };
            var commands = new BakerCommands(context.Baker, clipEntity);
            builder.ApplyTo(ref commands);

            base.Bake(clipEntity, context);
        }
    }
}