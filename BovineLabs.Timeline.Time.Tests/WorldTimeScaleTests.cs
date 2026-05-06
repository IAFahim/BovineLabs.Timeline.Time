using NUnit.Framework;

namespace BovineLabs.Timeline.Time.Tests
{
    [TestFixture]
    public class WorldTimeScaleDataTests
    {
        [Test]
        public void Default_AllFieldsZeroOrFalse()
        {
            var w = new WorldTimeScale();
            Assert.AreEqual(0f, w.DefaultScale);
            Assert.AreEqual(0f, w.ActiveScale);
            Assert.IsFalse(w.IsActive);
            Assert.IsFalse(w.ScaleFixedDeltaTime);
            Assert.AreEqual(0f, w.DefaultFixedDeltaTime);
        }

        [Test]
        public void DefaultScale_SetCorrectly()
        {
            var w = new WorldTimeScale { DefaultScale = 1.0f };
            Assert.AreEqual(1.0f, w.DefaultScale);
        }

        [Test]
        public void ActiveScale_SetCorrectly()
        {
            var w = new WorldTimeScale { ActiveScale = 0.5f };
            Assert.AreEqual(0.5f, w.ActiveScale);
        }

        [Test]
        public void IsActive_SetTrue()
        {
            var w = new WorldTimeScale { IsActive = true };
            Assert.IsTrue(w.IsActive);
        }

        [Test]
        public void IsActive_SetFalse()
        {
            var w = new WorldTimeScale { IsActive = true };
            w.IsActive = false;
            Assert.IsFalse(w.IsActive);
        }

        [Test]
        public void ScaleFixedDeltaTime_SetTrue()
        {
            var w = new WorldTimeScale { ScaleFixedDeltaTime = true };
            Assert.IsTrue(w.ScaleFixedDeltaTime);
        }

        [Test]
        public void DefaultFixedDeltaTime_SetCorrectly()
        {
            var w = new WorldTimeScale { DefaultFixedDeltaTime = 0.02f };
            Assert.AreEqual(0.02f, w.DefaultFixedDeltaTime);
        }

        [Test]
        public void AllFields_SetCorrectly()
        {
            var w = new WorldTimeScale
            {
                DefaultScale = 1.0f,
                ActiveScale = 0.25f,
                IsActive = true,
                ScaleFixedDeltaTime = true,
                DefaultFixedDeltaTime = 0.0167f
            };
            Assert.AreEqual(1.0f, w.DefaultScale);
            Assert.AreEqual(0.25f, w.ActiveScale);
            Assert.IsTrue(w.IsActive);
            Assert.IsTrue(w.ScaleFixedDeltaTime);
            Assert.AreEqual(0.0167f, w.DefaultFixedDeltaTime, 0.0001);
        }
    }
}