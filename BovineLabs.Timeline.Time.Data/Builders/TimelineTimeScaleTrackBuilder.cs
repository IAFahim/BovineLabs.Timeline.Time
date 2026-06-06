using BovineLabs.Core.EntityCommands;

namespace BovineLabs.Timeline.Time.Data.Builders
{
    public struct TimelineTimeScaleTrackBuilder
    {
        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent(new TimelineTimeScaleMultiplier { Value = 1f });
        }
    }
}