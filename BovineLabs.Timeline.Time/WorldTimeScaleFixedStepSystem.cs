using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Time
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation)]
    public partial class WorldTimeScaleFixedStepSystem : SystemBase
    {
        private float _baseTimestep;
        private bool _captured;
        private FixedStepSimulationSystemGroup _fixedStep;

        protected override void OnCreate()
        {
            RequireForUpdate<WorldTimeScale>();
            _fixedStep = World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
            Enabled = _fixedStep != null;
        }

        protected override void OnUpdate()
        {
            if (!_captured)
            {
                _baseTimestep = math.max(0.0001f, _fixedStep.Timestep);
                _captured = true;
            }

            // Tolerate duplicate WorldTimeScale entities (last wins), matching WorldTimeScaleApplySystem's foreach —
            // GetSingleton would throw on a misconfiguration the sibling system silently accepts.
            var target = _baseTimestep;
            foreach (var worldScale in SystemAPI.Query<RefRO<WorldTimeScale>>())
                target = WorldTimeScaleResolve.ResolveFixedDeltaTime(_baseTimestep, worldScale.ValueRO);

            if (math.abs(_fixedStep.Timestep - target) > 1e-6f) _fixedStep.Timestep = target;
        }

        protected override void OnStopRunning()
        {
            RestoreBaseTimestep();
        }

        protected override void OnDestroy()
        {
            RestoreBaseTimestep();
        }

        private void RestoreBaseTimestep()
        {
            if (_captured && _fixedStep != null) _fixedStep.Timestep = _baseTimestep;
        }
    }
}