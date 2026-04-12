using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class BoundsTests
    {
        // ── Constructor ──────────────────────────────────────────────────────────

        [TestMethod]
        public void Constructor_ValidArguments_SetsProperties()
        {
            var b = new Bounds(1, 2, 5, 8);
            Assert.AreEqual(1, b.Left);
            Assert.AreEqual(2, b.Top);
            Assert.AreEqual(5, b.Right);
            Assert.AreEqual(8, b.Bottom);
        }

        [TestMethod]
        public void Constructor_RightEqualToLeft_Throws()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new Bounds(3, 0, 3, 1));
        }

        [TestMethod]
        public void Constructor_RightLessThanLeft_Throws()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new Bounds(5, 0, 2, 1));
        }

        [TestMethod]
        public void Constructor_BottomEqualToTop_Throws()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new Bounds(0, 4, 1, 4));
        }

        [TestMethod]
        public void Constructor_BottomLessThanTop_Throws()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new Bounds(0, 5, 1, 2));
        }

        // ── Width / Height ───────────────────────────────────────────────────────

        [TestMethod]
        public void Width_IsRightMinusLeft()
        {
            var b = new Bounds(3, 0, 10, 1);
            Assert.AreEqual(7, b.Width);
        }

        [TestMethod]
        public void Height_IsBottomMinusTop()
        {
            var b = new Bounds(0, 2, 1, 9);
            Assert.AreEqual(7, b.Height);
        }

        // ── Intersects ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Intersects_Overlapping_ReturnsTrue()
        {
            var a = new Bounds(0, 0, 4, 4);
            var b = new Bounds(2, 2, 6, 6);
            Assert.IsTrue(a.Intersects(b));
            Assert.IsTrue(b.Intersects(a));
        }

        [TestMethod]
        public void Intersects_OneContainsOther_ReturnsTrue()
        {
            var outer = new Bounds(0, 0, 10, 10);
            var inner = new Bounds(2, 2, 5, 5);
            Assert.IsTrue(outer.Intersects(inner));
            Assert.IsTrue(inner.Intersects(outer));
        }

        [TestMethod]
        public void Intersects_SeparateHorizontally_ReturnsFalse()
        {
            var a = new Bounds(0, 0, 3, 3);
            var b = new Bounds(5, 0, 8, 3);
            Assert.IsFalse(a.Intersects(b));
            Assert.IsFalse(b.Intersects(a));
        }

        [TestMethod]
        public void Intersects_SeparateVertically_ReturnsFalse()
        {
            var a = new Bounds(0, 0, 3, 3);
            var b = new Bounds(0, 5, 3, 8);
            Assert.IsFalse(a.Intersects(b));
            Assert.IsFalse(b.Intersects(a));
        }

        [TestMethod]
        public void Intersects_TouchingEdge_ReturnsFalse()
        {
            // Right edge of a == Left edge of b — no overlap (strict inequality check)
            var a = new Bounds(0, 0, 3, 3);
            var b = new Bounds(3, 0, 6, 3);
            Assert.IsFalse(a.Intersects(b));
            Assert.IsFalse(b.Intersects(a));
        }

        [TestMethod]
        public void Intersects_TouchingCorner_ReturnsFalse()
        {
            var a = new Bounds(0, 0, 3, 3);
            var b = new Bounds(3, 3, 6, 6);
            Assert.IsFalse(a.Intersects(b));
        }

        // ── ToString ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void ToString_FormatsCorrectly()
        {
            var b = new Bounds(1, 2, 5, 8);
            Assert.AreEqual("[1,2]..[5,8]", b.ToString());
        }
    }
}
