using BovineLabs.Essence.Authoring;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Playables;

namespace BovineLabs.Timeline.Time.Authoring
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelineSpeedFromStatAuthoring : MonoBehaviour
    {
        public Target ReadRootFrom = Target.Self;
        public ushort LinkKey;
        public Target Fallback = Target.Self;
        public StatSchemaObject Stat;

        [Min(0.0001f)] public float Min = 0.05f;

        public float Max = 100f;
        public float Default = 1f;

        private class TimelineSpeedFromStatBaker : Baker<TimelineSpeedFromStatAuthoring>
        {
            public override void Bake(TimelineSpeedFromStatAuthoring authoring)
            {
                DependsOn(authoring.Stat);

                var entity = GetEntity(TransformUsageFlags.None);

                if (authoring.Max < authoring.Min)
                    Debug.LogWarning(
                        $"{nameof(TimelineSpeedFromStatAuthoring)}: Max ({authoring.Max}) < Min ({authoring.Min}); clamping Max to Min.",
                        authoring);

                var max = Mathf.Max(authoring.Min, authoring.Max);
                var def = Mathf.Clamp(authoring.Default, authoring.Min, max);

                AddComponent(entity, new TimelineSpeedFromStat
                {
                    Source = new StatSource
                    {
                        Stat = authoring.Stat != null ? authoring.Stat.Key.ID : default,
                        Link = new EntityLinkRef
                        {
                            ReadRootFrom = authoring.ReadRootFrom,
                            LinkKey = authoring.LinkKey,
                        },
                    },
                    Fallback = authoring.Fallback,
                    Min = authoring.Min,
                    Max = max,
                    Default = def
                });

                AddComponent(entity, new TimelineTimeScaleMultiplier { Value = 1f });
            }
        }
    }
}