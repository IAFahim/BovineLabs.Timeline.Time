using System;
using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(TimeScaleClip))]
    [TrackColor(0.85f, 0.85f, 0.85f)]
    [DisplayName("BovineLabs/Timeline/Time/Time Scale")]
    public class TimeScaleTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
            context.Target = context.TrackEntity;
            base.Bake(context);
        }
    }
}
