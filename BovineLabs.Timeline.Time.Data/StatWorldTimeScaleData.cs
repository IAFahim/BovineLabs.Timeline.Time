using BovineLabs.Essence.Data;
using BovineLabs.Reaction.Data.Core;
using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    public struct StatWorldTimeScaleData : IComponentData
    {
        public Target ReadRootFrom;
        public ushort LinkKey;
        public StatKey ScaleStatKey;
    }
}
