using NUnit.Framework;
using Unity.IntegerTime;

namespace BovineLabs.Timeline.Time.Tests
{
    [TestFixture]
    public class TimelineTimeJumpMathTests
    {
        private static readonly DiscreteTime ClipStart = new(10f);
        private static readonly DiscreteTime ClipEnd = new(12f);

        [Test]
        public void Rewind_OvershootsClipStart_ClampsToClipStart()
        {
            // 5 frames into the clip, asked to go back 100 frames -> clamps to the clip's own start.
            var current = ClipStart + new DiscreteTime(5 / 60f);

            var result = TimelineTimeJumpMath.ComputeTarget(current, ClipStart, ClipEnd, -100, 60f);

            Assert.AreEqual(ClipStart, result);
        }

        [Test]
        public void Advance_OvershootsClipEnd_ClampsToClipEnd()
        {
            var current = ClipStart + new DiscreteTime(5 / 60f);

            var result = TimelineTimeJumpMath.ComputeTarget(current, ClipStart, ClipEnd, 500, 60f);

            Assert.AreEqual(ClipEnd, result);
        }

        [Test]
        public void Rewind_FitsWithinBounds_MovesExactlyRequestedFrames()
        {
            var current = ClipStart + new DiscreteTime(5 / 60f);
            var expected = current + new DiscreteTime(-1 / 60f);

            var result = TimelineTimeJumpMath.ComputeTarget(current, ClipStart, ClipEnd, -1, 60f);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Frames_Zero_ReturnsCurrentTimeUnchanged()
        {
            var current = ClipStart + new DiscreteTime(5 / 60f);

            var result = TimelineTimeJumpMath.ComputeTarget(current, ClipStart, ClipEnd, 0, 60f);

            Assert.AreEqual(current, result);
        }
    }
}
