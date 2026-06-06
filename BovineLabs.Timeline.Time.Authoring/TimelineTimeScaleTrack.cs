using System;
using System.ComponentModel;
using BovineLabs.Core.Authoring.EntityCommands;
using BovineLabs.Essence.Authoring;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Time.Data.Builders;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(TimelineTimeScaleClip))]
    [TrackColor(0.20f, 0.75f, 0.45f)]
    [DisplayName("BovineLabs/Time/Timeline Time Scale")]
    [TrackBindingType(typeof(StatAuthoring))]
    public class TimelineTimeScaleTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
            var builder = new TimelineTimeScaleTrackBuilder();
            var commands = new BakerCommands(context.Baker, context.Timer);
            builder.ApplyTo(ref commands);
            base.Bake(context);
        }
    }
}