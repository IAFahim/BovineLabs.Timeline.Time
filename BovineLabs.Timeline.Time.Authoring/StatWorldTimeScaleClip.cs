using BovineLabs.Essence.Authoring;
using BovineLabs.Timeline.Authoring;
using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    public class StatWorldTimeScaleClip : DOTSClip, ITimelineClipAsset
    {
        public StatSchemaObject scaleStat;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new StatWorldTimeScaleData
            {
                ScaleStatKey = this.scaleStat != null ? this.scaleStat.Key : default
            });

            context.Baker.AddComponent(clipEntity, new WorldTimeScaleAnimated { Value = 1f });

            base.Bake(clipEntity, context);
        }
    }
}
