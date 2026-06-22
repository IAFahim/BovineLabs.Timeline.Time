using BovineLabs.Core.Iterators;
using BovineLabs.Essence.Data;
using BovineLabs.Testing;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;

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
            Assert.AreEqual(default(StatKey), a.StatKey);
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

    [TestFixture]
    public class StatSpeedResolveTests
    {
        [Test]
        public void Resolve_NotFound_ReturnsDefaultNotZero()
        {
            var map = new TimelineSpeedFromStat { Min = StatSpeed.MinMultiplier, Max = 100f, Default = 1f };

            var result = StatSpeed.Resolve(map, false, 0f);

            Assert.AreEqual(1f, result);
            Assert.AreNotEqual(0f, result);
        }

        [Test]
        public void Resolve_FoundZero_ClampsToMinNotZero()
        {
            var map = new TimelineSpeedFromStat { Min = StatSpeed.MinMultiplier, Max = 100f, Default = 1f };

            var result = StatSpeed.Resolve(map, true, 0f);

            Assert.AreEqual(StatSpeed.MinMultiplier, result);
            Assert.AreNotEqual(0f, result);
        }
    }

    [TestFixture]
    public class StatSpeedFloorTests
    {
        [Test]
        public void Floor_BelowMin_ReturnsMin()
        {
            Assert.AreEqual(StatSpeed.MinMultiplier, StatSpeed.Floor(0f));
        }

        [Test]
        public void Floor_AboveMin_ReturnsInput()
        {
            Assert.AreEqual(2f, StatSpeed.Floor(2f));
        }

        [Test]
        public void Floor_NaN_ReturnsMin()
        {
            Assert.AreEqual(StatSpeed.MinMultiplier, StatSpeed.Floor(float.NaN));
        }
    }

    [TestFixture]
    public class StatSpeedApplyTests
    {
        [Test]
        public void Apply_NotFound_UsesDefaultThenFloors()
        {
            var config = new TimelineSpeedFromStat { Min = StatSpeed.MinMultiplier, Max = 100f, Default = 2f };

            var result = StatSpeed.Apply(1f, config, false, 0f);

            Assert.AreEqual(2f, result);
        }

        [Test]
        public void Apply_FoundClampedAndFloored()
        {
            var config = new TimelineSpeedFromStat { Min = StatSpeed.MinMultiplier, Max = 100f, Default = 1f };

            var result = StatSpeed.Apply(1f, config, true, 0f);

            Assert.AreEqual(StatSpeed.MinMultiplier, result);
        }

        [Test]
        public void Apply_IncomingBelowMin_FlooredByOuterMax()
        {
            var config = new TimelineSpeedFromStat { Min = StatSpeed.MinMultiplier, Max = 100f, Default = 1f };

            var result = StatSpeed.Apply(0.01f, config, true, 1f);

            Assert.AreEqual(StatSpeed.MinMultiplier, result);
        }

        [Test]
        public void Apply_CompoundMultiply_StaysAtOrAboveMin()
        {
            var config = new TimelineSpeedFromStat { Min = StatSpeed.MinMultiplier, Max = 100f, Default = 1f };

            var result = StatSpeed.Apply(0.5f, config, true, 0.5f);

            Assert.AreEqual(0.25f, result);
            Assert.GreaterOrEqual(result, StatSpeed.MinMultiplier);
        }
    }

    [TestFixture]
    public class StatMissingKeyMultiplierTests : ECSTestsFixture
    {
        private const ushort PresentKey = 1;
        private const ushort AbsentKey = 2;

        [Test]
        public void SpeedFromStat_BufferPresentKeyAbsent_ResolvesToDefaultNotZero()
        {
            var statBuffer = CreateStatBuffer();
            var map = new TimelineSpeedFromStat
                { Stat = AbsentKey, Min = StatSpeed.MinMultiplier, Max = 100f, Default = 1f };

            var found = statBuffer.AsMap().TryGetValue(map.Stat, out var sv);
            var value = found ? sv.ValueFloat : 0f;
            var multiplier = StatSpeed.Resolve(map, found, value);

            Assert.IsFalse(found);
            Assert.AreEqual(1f, multiplier);
            Assert.AreNotEqual(0f, multiplier);
        }

        [Test]
        public void TimeScaleTrack_BufferPresentKeyAbsent_FallsBackPositiveNotZero()
        {
            var statBuffer = CreateStatBuffer();
            var animated = new TimelineTimeScaleAnimated { AuthoredData = 1f, StatKey = AbsentKey };

            var read = statBuffer.AsMap().GetValueFloat(animated.StatKey, animated.AuthoredData);
            var multiplier = math.max(read, StatSpeed.MinMultiplier);

            Assert.AreEqual(1f, multiplier);
            Assert.AreNotEqual(0f, multiplier);
        }

        [Test]
        public void TimeScaleTrack_AuthoredZero_FlooredToMinNotZero()
        {
            var statBuffer = CreateStatBuffer();
            var animated = new TimelineTimeScaleAnimated { AuthoredData = 0f, StatKey = AbsentKey };

            var read = statBuffer.AsMap().GetValueFloat(animated.StatKey, animated.AuthoredData);
            var multiplier = math.max(read, StatSpeed.MinMultiplier);

            Assert.AreEqual(StatSpeed.MinMultiplier, multiplier);
            Assert.AreNotEqual(0f, multiplier);
        }

        private DynamicBuffer<Stat> CreateStatBuffer()
        {
            var entity = Manager.CreateEntity(typeof(Stat));
            var buffer = Manager.GetBuffer<Stat>(entity)
                .InitializeHashMap<Stat, StatKey, StatValue>(0, 64);

            var map = buffer.AsMap();
            map.Add(PresentKey, new StatValue { Added = 100, Multi = 1f });

            return Manager.GetBuffer<Stat>(entity);
        }
    }
}