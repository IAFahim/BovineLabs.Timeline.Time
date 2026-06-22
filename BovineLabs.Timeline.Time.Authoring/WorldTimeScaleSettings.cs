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

        [SerializeField] private float defaultFixedDeltaTime = 0.02f;

        public override void Bake(Baker<SettingsAuthoring> baker)
        {
            var entity = baker.GetEntity(TransformUsageFlags.None);

            // Floor the baked scale the same way WorldTimeScaleClip does: a true zero (or negative) DefaultScale soft-locks
            // variable-delta GameTime with no recovery (and a negative value throws from UnityEngine.Time.timeScale every frame),
            // because IsActive=false makes both apply systems fall back to DefaultScale unconditionally.
            var safeScale = Mathf.Max(WorldTimeScaleClip.MinScale, defaultTimeScale);

            baker.AddComponent(entity, new WorldTimeScale
            {
                DefaultScale = safeScale,
                ActiveScale = safeScale,
                IsActive = false,
                ScaleFixedDeltaTime = scaleFixedDeltaTime,
                DefaultFixedDeltaTime = defaultFixedDeltaTime
            });
        }
    }
}