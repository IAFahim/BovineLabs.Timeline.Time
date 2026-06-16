namespace BovineLabs.Timeline.Time.Authoring
{
    using BovineLabs.Essence.Authoring;
    using BovineLabs.Reaction.Data.Core;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Playables;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelineSpeedFromStatAuthoring : MonoBehaviour
    {
        public Target ReadRootFrom = Target.Self;
        public ushort LinkKey;
        public Target Fallback = Target.Self;
        public StatSchemaObject Stat;

        [Min(0.0001f)]
        public float Min = 0.05f;

        public float Max = 100f;
        public float Default = 1f;

        private class TimelineSpeedFromStatBaker : Baker<TimelineSpeedFromStatAuthoring>
        {
            public override void Bake(TimelineSpeedFromStatAuthoring authoring)
            {
                this.DependsOn(authoring.Stat);

                var entity = this.GetEntity(TransformUsageFlags.None);

                this.AddComponent(entity, new TimelineSpeedFromStat
                {
                    ReadRootFrom = authoring.ReadRootFrom,
                    LinkKey = authoring.LinkKey,
                    Fallback = authoring.Fallback,
                    Stat = authoring.Stat != null ? authoring.Stat.Key : default,
                    Min = authoring.Min,
                    Max = authoring.Max,
                    Default = authoring.Default,
                });

                this.AddComponent(entity, new TimelineTimeScaleMultiplier { Value = 1f });
            }
        }
    }
}
