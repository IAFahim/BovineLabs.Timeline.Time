using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class WorldTimeScaleApplySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingleton<WorldTimeScale>(out var worldScale))
            {
                var targetScale = worldScale.IsActive ? worldScale.ActiveScale : worldScale.DefaultScale;

                if (Mathf.Abs(UnityEngine.Time.timeScale - targetScale) > 0.001f)
                {
                    UnityEngine.Time.timeScale = targetScale;

                    if (worldScale.ScaleFixedDeltaTime)
                        UnityEngine.Time.fixedDeltaTime = Mathf.Max(0.0001f,
                            worldScale.DefaultFixedDeltaTime * targetScale);
                }
            }
        }
    }
}