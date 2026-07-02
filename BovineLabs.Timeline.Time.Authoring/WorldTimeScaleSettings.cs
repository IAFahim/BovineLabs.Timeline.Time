using BovineLabs.Core.Authoring.Settings;
using BovineLabs.Core.Settings;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Time.Authoring
{
    [SettingsGroup("Timeline")]
    public class WorldTimeScaleSettings : SettingsBase
    {
        [SerializeField] private float defaultTimeScale = 1f;

        [Header("Physics Optimization")]
        [Tooltip("Enable this so your physics don't get choppy during slow-mo. It smoothly downscales fixedDeltaTime.")]
        [SerializeField]
        private bool scaleFixedDeltaTime = true;

        [Tooltip("Base classic fixedDeltaTime (seconds). Guarded against 0/negative which would drive physics to ~10kHz.")]
        [Min(0.001f)]
        [SerializeField] private float defaultFixedDeltaTime = 0.02f;

        public override void Bake(Baker<SettingsAuthoring> baker)
        {
            var entity = baker.GetEntity(TransformUsageFlags.None);

            var safeScale = Mathf.Max(WorldTimeScaleClip.MinScale, defaultTimeScale);

            baker.AddComponent(entity, new WorldTimeScale
            {
                DefaultScale = safeScale,
                ActiveScale = safeScale,
                IsActive = false,
                ScaleFixedDeltaTime = scaleFixedDeltaTime,
                DefaultFixedDeltaTime = Mathf.Max(WorldTimeScaleResolve.MinFixedDeltaTime, defaultFixedDeltaTime)
            });
        }
    }
}