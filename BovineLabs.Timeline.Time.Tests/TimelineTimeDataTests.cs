using NUnit.Framework;
using Unity.Entities;

namespace BovineLabs.Timeline.Time.Tests
{
    [TestFixture]
    public class TimelineTimeScaleMultiplierTests
    {
        [Test]
        public void Default_ValueIsZero()
        {
            var m = new TimelineTimeScaleMultiplier();
            Assert.AreEqual(0f, m.Value);
        }

        [Test]
        public void Value_SetCorrectly()
        {
            var m = new TimelineTimeScaleMultiplier { Value = 2.5f };
            Assert.AreEqual(2.5f, m.Value);
        }
    }

    [TestFixture]
    public class TimelineTimeScaleAnimatedTests
    {
        [Test]
        public void Default_AuthoredDataIsZero()
        {
            var a = new TimelineTimeScaleAnimated();
            Assert.AreEqual(0f, a.AuthoredData);
        }

        [Test]
        public void Default_ValueIsZero()
        {
            var a = new TimelineTimeScaleAnimated();
            Assert.AreEqual(0f, a.Value);
        }

        [Test]
        public void Default_StatKeyIsDefault()
        {
            var a = new TimelineTimeScaleAnimated();
            Assert.AreEqual(default, a.StatKey);
        }

        [Test]
        public void Default_StatEntityIsNull()
        {
            var a = new TimelineTimeScaleAnimated();
            Assert.AreEqual(Entity.Null, a.StatEntity);
        }

        [Test]
        public void AuthoredData_SetCorrectly()
        {
            var a = new TimelineTimeScaleAnimated { AuthoredData = 3.7f };
            Assert.AreEqual(3.7f, a.AuthoredData);
        }

        [Test]
        public void Value_SetCorrectly()
        {
            var a = new TimelineTimeScaleAnimated { Value = 4.2f };
            Assert.AreEqual(4.2f, a.Value);
        }

        [Test]
        public void Value_PropertyRoundTrip()
        {
            var a = new TimelineTimeScaleAnimated { Value = 1.5f };
            a.Value = 2.5f;
            Assert.AreEqual(2.5f, a.Value);
        }
    }

    [TestFixture]
    public class WorldTimeScaleAnimatedTests
    {
        [Test]
        public void Default_AuthoredDataIsZero()
        {
            var a = new WorldTimeScaleAnimated();
            Assert.AreEqual(0f, a.AuthoredData);
        }

        [Test]
        public void Default_ValueIsZero()
        {
            var a = new WorldTimeScaleAnimated();
            Assert.AreEqual(0f, a.Value);
        }

        [Test]
        public void AuthoredData_SetCorrectly()
        {
            var a = new WorldTimeScaleAnimated { AuthoredData = 0.5f };
            Assert.AreEqual(0.5f, a.AuthoredData);
        }

        [Test]
        public void Value_SetCorrectly()
        {
            var a = new WorldTimeScaleAnimated { Value = 1.0f };
            Assert.AreEqual(1.0f, a.Value);
        }

        [Test]
        public void Value_PropertyRoundTrip()
        {
            var a = new WorldTimeScaleAnimated { Value = 0.0f };
            a.Value = 3.0f;
            Assert.AreEqual(3.0f, a.Value);
        }
    }
}
