using BovineLabs.Core.ConfigVars;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BovineLabs.Timeline.Time
{
    [Configurable]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class TimeScaleApplySystem : SystemBase
    {
        private bool _wasOverriddenByTimeline;
        private float _previousTimeScale = 1f;

        protected override void OnUpdate()
        {
            bool hasActiveTimeScale = false;
            float minTimeScale = float.MaxValue;

            foreach (var activeScale in SystemAPI.Query<RefRO<ActiveTimeScale>>())
            {
                hasActiveTimeScale = true;
                minTimeScale = math.min(minTimeScale, activeScale.ValueRO.Config.Scale);
            }

            if (hasActiveTimeScale)
            {
                if (!_wasOverriddenByTimeline)
                {
                    _previousTimeScale = UnityEngine.Time.timeScale;
                    _wasOverriddenByTimeline = true;
                }

                UnityEngine.Time.timeScale = math.max(0f, minTimeScale);
            }
            else if (_wasOverriddenByTimeline)
            {
                UnityEngine.Time.timeScale = _previousTimeScale;
                _wasOverriddenByTimeline = false;
            }
        }
    }
}
