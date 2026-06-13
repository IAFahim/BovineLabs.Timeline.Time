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
        [Tooltip(
            "Global time scale for the entire world. 0 = Freeze Frame, 0.1 = Slow Mo, 1 = Normal, >1 = Fast Forward. " +
            "WARNING: timeScale = 0 on a LOOPING clip never deactivates and soft-locks variable-delta GameTime with no recovery.")]
        [Range(0f, 10f)]
        public float timeScale = 0.1f;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var builder = new WorldTimeScaleBuilder
            {
                AuthoredData = timeScale,
                Value = timeScale
            };
            var commands = new BakerCommands(context.Baker, clipEntity);
            builder.ApplyTo(ref commands);

            base.Bake(clipEntity, context);
        }
    }
}