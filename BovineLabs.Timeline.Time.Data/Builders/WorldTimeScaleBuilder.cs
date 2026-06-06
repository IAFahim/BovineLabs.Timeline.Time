using BovineLabs.Core.EntityCommands;
using Unity.Entities;

namespace BovineLabs.Timeline.Time.Data.Builders
{
    public struct WorldTimeScaleBuilder
    {
        public float AuthoredData;
        public float Value;

        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent(new WorldTimeScaleAnimated
            {
                AuthoredData = AuthoredData,
                Value = Value
            });
        }
    }
}
