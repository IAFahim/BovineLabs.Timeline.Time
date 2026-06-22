using NUnit.Framework;

namespace BovineLabs.Timeline.Time.Tests
{
    [TestFixture]
    public class WorldTimeScaleResolveTests
    {
        [Test]
        public void ResolveScale_IsActiveTrue_ReturnsActiveScale()
        {
            var worldScale = new WorldTimeScale { DefaultScale = 1f, ActiveScale = 0.25f, IsActive = true };
            Assert.AreEqual(0.25f, WorldTimeScaleResolve.ResolveScale(worldScale));
        }

        [Test]
        public void ResolveScale_IsActiveFalse_ReturnsDefaultScale()
        {
            var worldScale = new WorldTimeScale { DefaultScale = 1f, ActiveScale = 0.25f, IsActive = false };
            Assert.AreEqual(1f, WorldTimeScaleResolve.ResolveScale(worldScale));
        }

        [Test]
        public void ResolveFixedDeltaTime_ScaleFixedTrue_MultipliesBaseByResolvedScale()
        {
            var worldScale = new WorldTimeScale { ActiveScale = 0.1f, IsActive = true, ScaleFixedDeltaTime = true };
            Assert.AreEqual(0.002f, WorldTimeScaleResolve.ResolveFixedDeltaTime(0.02f, worldScale), 1e-7f);
        }

        [Test]
        public void ResolveFixedDeltaTime_ScaleFixedFalse_ReturnsBaseUnscaled()
        {
            var worldScale = new WorldTimeScale { ActiveScale = 0.1f, IsActive = true, ScaleFixedDeltaTime = false };
            Assert.AreEqual(0.02f, WorldTimeScaleResolve.ResolveFixedDeltaTime(0.02f, worldScale), 1e-7f);
        }

        [Test]
        public void ResolveFixedDeltaTime_FloorsAt0p0001()
        {
            var zeroBase = new WorldTimeScale { ScaleFixedDeltaTime = false };
            Assert.AreEqual(0.0001f, WorldTimeScaleResolve.ResolveFixedDeltaTime(0f, zeroBase), 1e-9f);

            var zeroScale = new WorldTimeScale { ActiveScale = 0f, IsActive = true, ScaleFixedDeltaTime = true };
            Assert.AreEqual(0.0001f, WorldTimeScaleResolve.ResolveFixedDeltaTime(0.02f, zeroScale), 1e-9f);
        }
    }
}
