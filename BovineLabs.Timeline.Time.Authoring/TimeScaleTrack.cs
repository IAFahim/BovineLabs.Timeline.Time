using System;
using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(TimeScaleClip))]
    [TrackColor(0.85f, 0.85f, 0.85f)]
    [TrackBindingType(typeof(TimeScaleTargetAuthoring))]
    [DisplayName("BovineLabs/Timeline/Time/Time Scale")]
    public class TimeScaleTrack : DOTSTrack
    {
    }
}
