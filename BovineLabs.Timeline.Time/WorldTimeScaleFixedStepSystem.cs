using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation)]
    public partial class WorldTimeScaleFixedStepSystem : SystemBase
    {
        private FixedStepSimulationSystemGroup fixedStep;
        private float baseTimestep;
        private bool captured;

        protected override void OnCreate()
        {
            this.RequireForUpdate<WorldTimeScale>();
            this.fixedStep = this.World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
            this.Enabled = this.fixedStep != null;
        }

        protected override void OnUpdate()
        {
            if (!this.captured)
            {
                this.baseTimestep = math.max(0.0001f, this.fixedStep.Timestep);
                this.captured = true;
            }

            var worldScale = SystemAPI.GetSingleton<WorldTimeScale>();
            var scale = worldScale.IsActive ? worldScale.ActiveScale : worldScale.DefaultScale;

            var target = worldScale.ScaleFixedDeltaTime ? this.baseTimestep * scale : this.baseTimestep;
            target = math.max(0.0001f, target);

            if (math.abs(this.fixedStep.Timestep - target) > 1e-6f)
            {
                this.fixedStep.Timestep = target;
            }
        }
    }
}
