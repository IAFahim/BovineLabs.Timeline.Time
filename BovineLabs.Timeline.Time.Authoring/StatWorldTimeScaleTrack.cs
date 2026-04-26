using System;
using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(StatWorldTimeScaleClip))]
    [TrackColor(0.85f, 0.95f, 0.95f)]
    [DisplayName("BovineLabs/Timeline/Time/Stat World Time Scale")]
    public class StatWorldTimeScaleTrack : DOTSTrack
    {
    }
}
