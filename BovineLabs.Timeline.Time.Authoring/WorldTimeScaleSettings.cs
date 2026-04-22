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
        [SerializeField] private bool scaleFixedDeltaTime = true;
        [SerializeField] private float defaultFixedDeltaTime = 0.02f;

        public override void Bake(Baker<SettingsAuthoring> baker)
        {
            var entity = baker.GetEntity(TransformUsageFlags.None);
            baker.AddComponent(entity, new WorldTimeScale 
            { 
                DefaultScale = this.defaultTimeScale,
                ActiveScale = this.defaultTimeScale,
                IsActive = false,
                ScaleFixedDeltaTime = this.scaleFixedDeltaTime,
                DefaultFixedDeltaTime = this.defaultFixedDeltaTime
            });
        }
    }
}