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
        /// <summary>
        /// Lower bound for the baked world time scale. A true zero soft-locks variable-delta GameTime with no recovery
        /// (especially on a looping clip that never deactivates), so the baked value is floored to this minimum.
        /// </summary>
        public const float MinScale = 0.05f;

        [Tooltip(
            "Global time scale for the entire world. 0 = Freeze Frame, 0.1 = Slow Mo, 1 = Normal, >1 = Fast Forward. " +
            "WARNING: timeScale = 0 on a LOOPING clip never deactivates and soft-locks variable-delta GameTime with no recovery.")]
        [Range(0f, 10f)]
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