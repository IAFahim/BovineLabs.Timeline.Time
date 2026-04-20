using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Time.Authoring
{
    [DisallowMultipleComponent]
    [AddComponentMenu("BovineLabs/Timeline/Time Scale Target")]
    public class TimeScaleTargetAuthoring : MonoBehaviour
    {
        private class Baker : Baker<TimeScaleTargetAuthoring>
        {
            public override void Bake(TimeScaleTargetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<TimeScaleTarget>(entity);
            }
        }
    }
}
