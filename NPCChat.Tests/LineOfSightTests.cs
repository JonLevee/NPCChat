using System.Drawing;
using NPCChat.Core.AIClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class LineOfSightTests
    {
        private static readonly List<Bounds> NoObstacles = [];

        private static Bounds Block(int x, int y, int w = 1, int h = 1)
            => new Bounds(x, y, x + w, y + h);

        // ── No obstacles ─────────────────────────────────────────────────────────

        [TestMethod]
        public void Check_NoObstacles_ReturnsTrue()
        {
            Assert.IsTrue(LineOfSight.Check(new Point(0, 0), new Point(10, 5), NoObstacles));
        }

        [TestMethod]
        public void Check_SamePoint_ReturnsTrue()
        {
            var obs = new List<Bounds> { Block(3, 3) };
            Assert.IsTrue(LineOfSight.Check(new Point(0, 0), new Point(0, 0), obs));
        }

        [TestMethod]
        public void Check_VeryShortDistance_ReturnsTrue()
        {
            // len < 0.5f — early-out path
            var obs = new List<Bounds> { Block(5, 5) };
            Assert.IsTrue(LineOfSight.Check(new Point(1, 1), new Point(1, 1), obs));
        }

        // ── Clear line ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Check_ClearHorizontalLine_ReturnsTrue()
        {
            var obs = new List<Bounds> { Block(0, 5) }; // obstacle off to the side
            Assert.IsTrue(LineOfSight.Check(new Point(0, 0), new Point(10, 0), obs));
        }

        [TestMethod]
        public void Check_ClearDiagonalLine_ReturnsTrue()
        {
            Assert.IsTrue(LineOfSight.Check(new Point(0, 0), new Point(5, 5), NoObstacles));
        }

        // ── Blocked line ─────────────────────────────────────────────────────────

        [TestMethod]
        public void Check_WallInPath_ReturnsFalse()
        {
            // Obstacle at x=5, blocking a horizontal line from (0,0) to (10,0)
            var obs = new List<Bounds> { Block(5, 0) };
            Assert.IsFalse(LineOfSight.Check(new Point(0, 0), new Point(10, 0), obs));
        }

        [TestMethod]
        public void Check_WallInDiagonalPath_ReturnsFalse()
        {
            // 5x5 wall in the middle of a diagonal path
            var obs = new List<Bounds> { Block(5, 5, 3, 3) };
            Assert.IsFalse(LineOfSight.Check(new Point(0, 0), new Point(12, 12), obs));
        }

        [TestMethod]
        public void Check_MultipleObstacles_OneBlocks_ReturnsFalse()
        {
            var obs = new List<Bounds>
            {
                Block(100, 100), // far away — irrelevant
                Block(5, 0)      // directly in path
            };
            Assert.IsFalse(LineOfSight.Check(new Point(0, 0), new Point(10, 0), obs));
        }

        [TestMethod]
        public void Check_ObstacleBesidePath_DoesNotBlock()
        {
            // Obstacle is at y=2, path travels along y=0
            var obs = new List<Bounds> { Block(5, 2) };
            Assert.IsTrue(LineOfSight.Check(new Point(0, 0), new Point(10, 0), obs));
        }
    }
}
