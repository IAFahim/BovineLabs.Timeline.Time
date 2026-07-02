using BovineLabs.Core.Authoring.EntityCommands;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Time.Data.Builders;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Time.Authoring
{
    public class WorldTimeScaleClip : DOTSClip, ITimelineClipAsset
    {
        public const float MinScale = 0.05f;

        [Tooltip(
            "Global time scale for the entire world. 0.05 = near-freeze (min), 0.1 = Slow Mo, 1 = Normal, >1 = Fast Forward.")]
        [Range(MinScale, 10f)]
        public float timeScale = 0.1f;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var safe = Mathf.Max(MinScale, timeScale);
            var builder = new WorldTimeScaleBuilder
            {
                AuthoredData = safe,
                Value = safe
            };
            var commands = new BakerCommands(context.Baker, clipEntity);
            builder.ApplyTo(ref commands);

            base.Bake(clipEntity, context);
        }
    }
}