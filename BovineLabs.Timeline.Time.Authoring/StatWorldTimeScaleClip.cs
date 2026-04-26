using BovineLabs.Essence.Authoring;
using BovineLabs.Essence.Data;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.EntityLinks.Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    public class StatWorldTimeScaleClip : DOTSClip, ITimelineClipAsset
    {
        public Target readRootFrom = Target.Owner;
        public EntityLinkSchema link;
        public StatSchemaObject scaleStat;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            if (!EntityLinkAuthoringUtility.TryGetKey(this.link, out ushort linkKey))
            {
                linkKey = 0;
            }

            context.Baker.AddComponent(clipEntity, new StatWorldTimeScaleData
            {
                ReadRootFrom = this.readRootFrom,
                LinkKey = linkKey,
                ScaleStatKey = this.scaleStat != null ? this.scaleStat.Key : default
            });

            context.Baker.AddComponent(clipEntity, new WorldTimeScaleAnimated { Value = 1f });

            base.Bake(clipEntity, context);
        }
    }
}
