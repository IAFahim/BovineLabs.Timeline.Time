using BovineLabs.Essence.Authoring;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Data;
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
            context.Baker.SetComponent(clipEntity, new TrackBinding { Value = context.Timer });
            
            context.Baker.AddComponent(clipEntity, new TimelineTimeScaleAnimated 
            { 
                AuthoredData = timeScale,
                StatKey = this.stat != null ? this.stat.Key : default,
                StatEntity = context.Binding != null ? context.Binding.Target : Entity.Null
            });

            base.Bake(clipEntity, context);
        }
    }
}