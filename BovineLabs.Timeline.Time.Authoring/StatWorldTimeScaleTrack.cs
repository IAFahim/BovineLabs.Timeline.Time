using System;
using System.ComponentModel;
using BovineLabs.Essence.Authoring;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(StatWorldTimeScaleClip))]
    [TrackColor(0.85f, 0.95f, 0.95f)]
    [DisplayName("BovineLabs/Time/Stat World Time Scale")]
    [TrackBindingType(typeof(StatAuthoring))]
    public class StatWorldTimeScaleTrack : DOTSTrack
    {
    }
}