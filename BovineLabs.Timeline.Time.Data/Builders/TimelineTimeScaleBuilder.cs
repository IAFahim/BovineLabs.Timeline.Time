using BovineLabs.Core.EntityCommands;
using BovineLabs.Essence.Data;
using Unity.Entities;

namespace BovineLabs.Timeline.Time.Data.Builders
{
    public struct TimelineTimeScaleBuilder
    {
        public float AuthoredData;
        public StatKey StatKey;
        public Entity StatEntity;

        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent(new TimelineTimeScaleAnimated
            {
                AuthoredData = AuthoredData,
                StatKey = StatKey,
                StatEntity = StatEntity
            });
        }
    }
}