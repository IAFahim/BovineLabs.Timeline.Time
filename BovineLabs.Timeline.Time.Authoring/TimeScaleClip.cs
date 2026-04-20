using BovineLabs.Timeline.Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    public class TimeScaleClip : DOTSClip, ITimelineClipAsset
    {
        [Tooltip("Global time scale during this clip. 0 = Freeze Frame, 0.1 = Slow Mo, 1 = Normal, >1 = Fast Forward.")]
        [Range(0f, 10f)]
        public float timeScale = 0.1f;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new TimeScaleAnimated
            {
                AuthoredData = new TimeScaleData
                {
                    Scale = timeScale
                }
            });

            base.Bake(clipEntity, context);
        }
    }
}
