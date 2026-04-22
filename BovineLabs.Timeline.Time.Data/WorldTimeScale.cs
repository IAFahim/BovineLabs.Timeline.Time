using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    public struct WorldTimeScale : IComponentData
    {
        public float DefaultScale;
        public float ActiveScale;
        public bool IsActive;
        public bool ScaleFixedDeltaTime;
        public float DefaultFixedDeltaTime;
    }
}