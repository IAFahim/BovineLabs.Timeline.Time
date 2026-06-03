using System;
using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(WorldTimeScaleClip))]
    [TrackColor(0.92f, 0.92f, 0.92f)]
    [DisplayName("BovineLabs/Time/World Time Scale")]
    public class WorldTimeScaleTrack : DOTSTrack
    {
    }
}