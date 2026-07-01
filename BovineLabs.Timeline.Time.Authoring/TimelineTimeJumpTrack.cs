using System;
using System.ComponentModel;
using BovineLabs.Reaction.Authoring.Core;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    [Serializable]
    [TrackClipType(typeof(TimelineTimeJumpClip))]
    [TrackBindingType(typeof(TargetsAuthoring))]
    [TrackColor(0.75f, 0.2f, 0.55f)]
    [DisplayName("BovineLabs/Time/Timeline Time Jump")]
    public sealed class TimelineTimeJumpTrack : DOTSTrack
    {
    }
}
