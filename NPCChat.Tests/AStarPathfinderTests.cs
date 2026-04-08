using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.PathfindingClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class AStarPathfinderTests
    {
        // Helper: build a PathGrid from a set of blocked rectangles.
        // Mover is 1x1 for simplicity unless specified.
        private static PathGrid BuildGrid(IEnumerable<Bounds> obstacles, int moverW = 1, int moverH = 1)
        {
            return new PathGridBuilder(new List<Bounds>(obstacles), moverW, moverH).Build();
        }

        // ── source == target ────────────────────────────────────────────────

        [TestMethod]
        public void FindPath_SameSourceAndTarget_ReturnsEmptyList()
        {
            var grid = BuildGrid([]);
            var result = AStarPathfinder.FindPath(grid, new Point(5, 5), new Point(5, 5), 10000);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        // ── straight cardinal path ──────────────────────────────────────────

        [TestMethod]
        public void FindPath_OpenMap_StraightHorizontalPath()
        {
            var grid = BuildGrid([]);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 0), new Point(5, 0), 10000);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            // Path must begin adjacent to source and end at target.
            Assert.AreEqual(new Point(5, 0), result[result.Count - 1]);
        }

        [TestMethod]
        public void FindPath_OpenMap_StraightVerticalPath()
        {
            var grid = BuildGrid([]);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 0), new Point(0, 4), 10000);
            Assert.IsNotNull(result);
            Assert.AreEqual(new Point(0, 4), result[result.Count - 1]);
        }

        // ── diagonal path ───────────────────────────────────────────────────

        [TestMethod]
        public void FindPath_OpenMap_DiagonalPath_UsesFewerStepsThanCardinal()
        {
            var grid = BuildGrid([]);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 0), new Point(3, 3), 10000);
            Assert.IsNotNull(result);
            // Diagonal should reach (3,3) in exactly 3 steps (pure diagonal).
            Assert.HasCount(3, result);
            Assert.AreEqual(new Point(3, 3), result[result.Count - 1]);
        }

        // ── wall avoidance ──────────────────────────────────────────────────

        [TestMethod]
        public void FindPath_WallBlocking_PathGoesAround()
        {
            // Vertical wall at x=2 from y=0 to y=4 (5 tiles tall, 1 wide).
            var wall = new Bounds(2, 0, 3, 5);
            var grid = BuildGrid([wall]);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 2), new Point(4, 2), 10000);
            Assert.IsNotNull(result, "Path should exist going around the wall.");
            Assert.AreEqual(new Point(4, 2), result[result.Count - 1]);
            // No step should land inside the wall.
            foreach (var step in result)
                Assert.IsFalse(wall.Intersects(new Bounds(step.X, step.Y, step.X + 1, step.Y + 1)),
                    $"Step {step} is inside the wall.");
        }

        // ── impassable target ───────────────────────────────────────────────

        [TestMethod]
        public void FindPath_ImpassableTarget_ReturnsNull()
        {
            var block = new Bounds(5, 5, 6, 6);
            var grid = BuildGrid([block]);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 0), new Point(5, 5), 10000);
            Assert.IsNull(result, "Should return null when target tile is blocked.");
        }

        // ── unreachable target ──────────────────────────────────────────────

        [TestMethod]
        public void FindPath_EnclosedTarget_ReturnsNull()
        {
            // Surround target (3,3) with a solid 5x5 box (1-tile border around it).
            var obstacles = new List<Bounds>
            {
                new Bounds(2, 2, 5, 3), // top
                new Bounds(2, 4, 5, 5), // bottom
                new Bounds(2, 2, 3, 5), // left
                new Bounds(4, 2, 5, 5), // right
            };
            var grid = BuildGrid(obstacles);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 0), new Point(3, 3), 10000);
            Assert.IsNull(result, "Should return null when target is completely enclosed.");
        }

        // ── iteration limit ─────────────────────────────────────────────────

        [TestMethod]
        public void FindPath_IterationLimitExceeded_ReturnsNull()
        {
            // Long open path but very small iteration budget.
            var grid = BuildGrid([]);
            var result = AStarPathfinder.FindPath(grid, new Point(0, 0), new Point(100, 100), maxIterations: 1);
            Assert.IsNull(result, "Should return null when iteration budget is exhausted.");
        }

        // ── path continuity ─────────────────────────────────────────────────

        [TestMethod]
        public void FindPath_PathSteps_AreEachAdjacent()
        {
            var grid = BuildGrid([]);
            var source = new Point(0, 0);
            var target = new Point(6, 4);
            var result = AStarPathfinder.FindPath(grid, source, target, 10000);
            Assert.IsNotNull(result);

            var prev = source;
            foreach (var step in result)
            {
                int dx = Math.Abs(step.X - prev.X);
                int dy = Math.Abs(step.Y - prev.Y);
                Assert.IsTrue(dx <= 1 && dy <= 1 && (dx + dy) > 0,
                    $"Step {step} is not adjacent to previous {prev}.");
                prev = step;
            }
        }
    }

    /// <summary>
    /// Allows tests to construct a PathGrid without going through WorldData.
    /// </summary>
    internal sealed class PathGridBuilder
    {
        private readonly List<Bounds> _obstacles;
        private readonly int _moverWidth;
        private readonly int _moverHeight;

        internal PathGridBuilder(List<Bounds> obstacles, int moverWidth, int moverHeight)
        {
            _obstacles = obstacles;
            _moverWidth = moverWidth;
            _moverHeight = moverHeight;
        }

        internal PathGrid Build() => new PathGrid(_obstacles, _moverWidth, _moverHeight);
    }
}
