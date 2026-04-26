using System;
using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(TimelineTimeScaleClip))]
    [TrackColor(0.2f, 0.8f, 0.4f)]
    [DisplayName("BovineLabs/Time/Timeline Time Scale")]
    public class TimelineTimeScaleTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
            context.Baker.AddComponent(context.Target, new TimelineTimeScaleMultiplier { Value = 1f });
            base.Bake(context);
        }
    }
}
