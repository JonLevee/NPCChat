using System.Drawing;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class MovementStateTests
    {
        [TestMethod]
        public void IsMoving_EmptyPath_ReturnsFalse()
        {
            var state = new MovementState();
            Assert.IsFalse(state.IsMoving);
        }

        [TestMethod]
        public void IsMoving_WithPathPoint_ReturnsTrue()
        {
            var state = new MovementState();
            state.Path.Enqueue(new Point(1, 1));
            Assert.IsTrue(state.IsMoving);
        }

        [TestMethod]
        public void ClearMovement_ResetsPath()
        {
            var state = new MovementState();
            state.Path.Enqueue(new Point(1, 1));
            state.Path.Enqueue(new Point(2, 2));
            state.ClearMovement();
            Assert.IsFalse(state.IsMoving);
            Assert.IsEmpty(state.Path);
        }

        [TestMethod]
        public void ClearMovement_ResetsFinalTarget()
        {
            var state = new MovementState();
            state.FinalTarget = new Point(5, 5);
            state.ClearMovement();
            Assert.IsNull(state.FinalTarget);
        }

        [TestMethod]
        public void ClearMovement_ResetsStepAccumulator()
        {
            var state = new MovementState();
            state.StepAccumulator = 0.75f;
            state.ClearMovement();
            Assert.AreEqual(0f, state.StepAccumulator);
        }

        [TestMethod]
        public void FinalTarget_DefaultIsNull()
        {
            var state = new MovementState();
            Assert.IsNull(state.FinalTarget);
        }

        [TestMethod]
        public void Facing_DefaultIsSouth()
        {
            var state = new MovementState();
            Assert.AreEqual(Direction8.S, state.Facing);
        }
    }
}
