using Unity.Entities;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class WorldTimeScaleApplySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingleton<WorldTimeScale>(out var worldScale))
            {
                float targetScale = worldScale.IsActive ? worldScale.ActiveScale : worldScale.DefaultScale;
                
                if (UnityEngine.Mathf.Abs(UnityEngine.Time.timeScale - targetScale) > 0.001f)
                {
                    UnityEngine.Time.timeScale = targetScale;
                    
                    if (worldScale.ScaleFixedDeltaTime)
                    {
                        UnityEngine.Time.fixedDeltaTime = worldScale.DefaultFixedDeltaTime * targetScale;
                    }
                }
            }
        }
    }
}