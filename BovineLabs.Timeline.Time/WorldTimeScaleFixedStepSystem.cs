using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation)]
    public partial class WorldTimeScaleFixedStepSystem : SystemBase
    {
        private FixedStepSimulationSystemGroup _fixedStep;
        private float _baseTimestep;
        private bool _captured;

        protected override void OnCreate()
        {
            this.RequireForUpdate<WorldTimeScale>();
            this._fixedStep = this.World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
            this.Enabled = this._fixedStep != null;
        }

        protected override void OnUpdate()
        {
            if (!this._captured)
            {
                this._baseTimestep = math.max(0.0001f, this._fixedStep.Timestep);
                this._captured = true;
            }

            var worldScale = SystemAPI.GetSingleton<WorldTimeScale>();
            var scale = worldScale.IsActive ? worldScale.ActiveScale : worldScale.DefaultScale;

            var target = worldScale.ScaleFixedDeltaTime ? this._baseTimestep * scale : this._baseTimestep;
            target = math.max(0.0001f, target);

            if (math.abs(this._fixedStep.Timestep - target) > 1e-6f)
            {
                this._fixedStep.Timestep = target;
            }
        }

        protected override void OnStopRunning()
        {
            this.RestoreBaseTimestep();
        }

        protected override void OnDestroy()
        {
            this.RestoreBaseTimestep();
        }

        private void RestoreBaseTimestep()
        {
            if (this._captured && this._fixedStep != null)
            {
                this._fixedStep.Timestep = this._baseTimestep;
            }
        }
    }
}
