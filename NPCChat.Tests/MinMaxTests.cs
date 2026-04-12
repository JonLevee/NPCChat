using NPCChat.Core.DialogueClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class MinMaxTests
    {
        // ── No limits ────────────────────────────────────────────────────────────

        [TestMethod]
        public void Contains_NoLimits_AlwaysReturnsTrue()
        {
            var mm = new MinMax(0, false, 0, false);
            Assert.IsTrue(mm.Contains(-999f));
            Assert.IsTrue(mm.Contains(0f));
            Assert.IsTrue(mm.Contains(999f));
        }

        // ── Min only ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void Contains_MinOnly_BelowMin_ReturnsFalse()
        {
            var mm = new MinMax(0.5f, true, 0, false);
            Assert.IsFalse(mm.Contains(0.4f));
        }

        [TestMethod]
        public void Contains_MinOnly_AtMin_ReturnsTrue()
        {
            var mm = new MinMax(0.5f, true, 0, false);
            Assert.IsTrue(mm.Contains(0.5f));
        }

        [TestMethod]
        public void Contains_MinOnly_AboveMin_ReturnsTrue()
        {
            var mm = new MinMax(0.5f, true, 0, false);
            Assert.IsTrue(mm.Contains(0.9f));
        }

        // ── Max only ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void Contains_MaxOnly_AboveMax_ReturnsFalse()
        {
            var mm = new MinMax(0, false, 0.5f, true);
            Assert.IsFalse(mm.Contains(0.6f));
        }

        [TestMethod]
        public void Contains_MaxOnly_AtMax_ReturnsTrue()
        {
            var mm = new MinMax(0, false, 0.5f, true);
            Assert.IsTrue(mm.Contains(0.5f));
        }

        [TestMethod]
        public void Contains_MaxOnly_BelowMax_ReturnsTrue()
        {
            var mm = new MinMax(0, false, 0.5f, true);
            Assert.IsTrue(mm.Contains(0.1f));
        }

        // ── Both limits ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Contains_BothLimits_WithinRange_ReturnsTrue()
        {
            var mm = new MinMax(0.2f, true, 0.8f, true);
            Assert.IsTrue(mm.Contains(0.5f));
        }

        [TestMethod]
        public void Contains_BothLimits_AtMinEdge_ReturnsTrue()
        {
            var mm = new MinMax(0.2f, true, 0.8f, true);
            Assert.IsTrue(mm.Contains(0.2f));
        }

        [TestMethod]
        public void Contains_BothLimits_AtMaxEdge_ReturnsTrue()
        {
            var mm = new MinMax(0.2f, true, 0.8f, true);
            Assert.IsTrue(mm.Contains(0.8f));
        }

        [TestMethod]
        public void Contains_BothLimits_BelowMin_ReturnsFalse()
        {
            var mm = new MinMax(0.2f, true, 0.8f, true);
            Assert.IsFalse(mm.Contains(0.1f));
        }

        [TestMethod]
        public void Contains_BothLimits_AboveMax_ReturnsFalse()
        {
            var mm = new MinMax(0.2f, true, 0.8f, true);
            Assert.IsFalse(mm.Contains(0.9f));
        }
    }
}
