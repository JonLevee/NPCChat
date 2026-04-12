using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class ActorComponentPerceptionTests
    {
        private static Bounds At(int x, int y) => new Bounds(x, y, x + 1, y + 1);

        // Centers for 1x1 bounds at (x,y): center = ((x + x+1) / 2) = x  (integer division)

        [TestMethod]
        public void CanPerceive_SamePosition_ReturnsTrue()
        {
            var actor = new ActorComponent { PerceptionRange = 8f };
            Assert.IsTrue(actor.CanPerceive(At(0, 0), At(0, 0)));
        }

        [TestMethod]
        public void CanPerceive_TargetWithinRange_ReturnsTrue()
        {
            var actor = new ActorComponent { PerceptionRange = 8f };
            Assert.IsTrue(actor.CanPerceive(At(0, 0), At(5, 0)));
        }

        [TestMethod]
        public void CanPerceive_TargetExactlyAtRangeEdge_ReturnsTrue()
        {
            var actor = new ActorComponent { PerceptionRange = 8f };
            // Center of At(0,0) = (0,0); center of At(8,0) = (8,0); dx=8 == range
            Assert.IsTrue(actor.CanPerceive(At(0, 0), At(8, 0)));
        }

        [TestMethod]
        public void CanPerceive_TargetBeyondRange_ReturnsFalse()
        {
            var actor = new ActorComponent { PerceptionRange = 8f };
            // dx = 9 > 8
            Assert.IsFalse(actor.CanPerceive(At(0, 0), At(9, 0)));
        }

        [TestMethod]
        public void CanPerceive_DiagonalUsesChebyshev_NotManhattan()
        {
            var actor = new ActorComponent { PerceptionRange = 8f };
            // Chebyshev distance to (8,8) = max(8,8) = 8 → within range
            // Manhattan would be 16, so this distinguishes the two metrics
            Assert.IsTrue(actor.CanPerceive(At(0, 0), At(8, 8)));
        }

        [TestMethod]
        public void CanPerceive_DiagonalBeyondRange_ReturnsFalse()
        {
            var actor = new ActorComponent { PerceptionRange = 8f };
            // Chebyshev distance to (9,9) = max(9,9) = 9 → outside
            Assert.IsFalse(actor.CanPerceive(At(0, 0), At(9, 9)));
        }

        [TestMethod]
        public void CanPerceive_CustomRange_Respected()
        {
            var actor = new ActorComponent { PerceptionRange = 3f };
            Assert.IsTrue(actor.CanPerceive(At(0, 0), At(3, 0)));
            Assert.IsFalse(actor.CanPerceive(At(0, 0), At(4, 0)));
        }
    }
}
