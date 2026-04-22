using BovineLabs.Core.Authoring.Settings;
using BovineLabs.Core.Settings;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Time.Authoring
{
    //Global Bake
    [SettingsGroup("Timeline")]
    public class WorldTimeScaleSettings : SettingsBase
    {
        [SerializeField]
        private float defaultTimeScale = 1f;

        public override void Bake(Baker<SettingsAuthoring> baker)
        {
            var entity = baker.GetEntity(TransformUsageFlags.None);
            baker.AddComponent(entity, new WorldTimeScale { Value = this.defaultTimeScale });
        }
    }
}