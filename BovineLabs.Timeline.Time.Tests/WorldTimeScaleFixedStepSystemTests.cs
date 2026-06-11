using BovineLabs.Testing;
using NUnit.Framework;
using Unity.Entities;

namespace BovineLabs.Timeline.Time.Tests
{
    public class WorldTimeScaleFixedStepSystemTests : ECSTestsFixture
    {
        [Test]
        public void SlowMo_ScalesFixedStepTimestepByScale()
        {
            var group = CreateFixedStep(0.02f);
            SetWorldScale(new WorldTimeScale
                { DefaultScale = 1f, ActiveScale = 0.1f, IsActive = true, ScaleFixedDeltaTime = true });

            RunSystem();

            Assert.AreEqual(0.002f, group.Timestep, 1e-6f);
        }

        [Test]
        public void NormalSpeed_LeavesTimestepAtBase()
        {
            var group = CreateFixedStep(0.02f);
            SetWorldScale(new WorldTimeScale
                { DefaultScale = 1f, ActiveScale = 1f, IsActive = false, ScaleFixedDeltaTime = true });

            RunSystem();

            Assert.AreEqual(0.02f, group.Timestep, 1e-6f);
        }

        [Test]
        public void ScaleFixedDeltaTimeDisabled_LeavesTimestepAtBase()
        {
            var group = CreateFixedStep(0.02f);
            SetWorldScale(new WorldTimeScale
                { DefaultScale = 1f, ActiveScale = 0.1f, IsActive = true, ScaleFixedDeltaTime = false });

            RunSystem();

            Assert.AreEqual(0.02f, group.Timestep, 1e-6f);
        }

        [Test]
        public void SlowMoEnds_RestoresBaseTimestep()
        {
            var group = CreateFixedStep(0.02f);
            var entity = Manager.CreateEntity(typeof(WorldTimeScale));

            Manager.SetComponentData(entity, new WorldTimeScale
                { DefaultScale = 1f, ActiveScale = 0.25f, IsActive = true, ScaleFixedDeltaTime = true });
            RunSystem();
            Assert.AreEqual(0.005f, group.Timestep, 1e-6f);

            Manager.SetComponentData(entity, new WorldTimeScale
                { DefaultScale = 1f, ActiveScale = 0.25f, IsActive = false, ScaleFixedDeltaTime = true });
            RunSystem();
            Assert.AreEqual(0.02f, group.Timestep, 1e-6f);
        }

        private FixedStepSimulationSystemGroup CreateFixedStep(float timestep)
        {
            var group = World.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();
            group.Timestep = timestep;
            return group;
        }

        private void SetWorldScale(in WorldTimeScale worldScale)
        {
            var entity = Manager.CreateEntity(typeof(WorldTimeScale));
            Manager.SetComponentData(entity, worldScale);
        }

        private void RunSystem()
        {
            World.GetOrCreateSystemManaged<WorldTimeScaleFixedStepSystem>().Update();
        }
    }
}
