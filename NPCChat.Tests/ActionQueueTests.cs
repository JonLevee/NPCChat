using NPCChat.Core.BehaviorClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class ActionQueueTests
    {
        private static StubTask T(int priority) => new StubTask(priority);

        // ── Basic operations ─────────────────────────────────────────────────────

        [TestMethod]
        public void IsEmpty_NewQueue_IsTrue()
        {
            var q = new ActionQueue();
            Assert.IsTrue(q.IsEmpty);
        }

        [TestMethod]
        public void Enqueue_SingleTask_IsEmptyFalse()
        {
            var q = new ActionQueue();
            q.Enqueue(T(5));
            Assert.IsFalse(q.IsEmpty);
        }

        [TestMethod]
        public void TryPeekHighest_SingleTask_ReturnsThatTask()
        {
            var q = new ActionQueue();
            var task = T(5);
            q.Enqueue(task);
            Assert.AreSame(task, q.TryPeekHighest());
        }

        [TestMethod]
        public void TryPeekHighest_DoesNotRemoveTask()
        {
            var q = new ActionQueue();
            q.Enqueue(T(5));
            q.TryPeekHighest();
            Assert.IsFalse(q.IsEmpty);
        }

        [TestMethod]
        public void TryPeekHighest_EmptyQueue_ReturnsNull()
        {
            var q = new ActionQueue();
            Assert.IsNull(q.TryPeekHighest());
        }

        [TestMethod]
        public void Dequeue_EmptyQueue_ReturnsNull()
        {
            var q = new ActionQueue();
            Assert.IsNull(q.Dequeue());
        }

        [TestMethod]
        public void Dequeue_SingleTask_ReturnsItAndBecomesEmpty()
        {
            var q = new ActionQueue();
            var task = T(5);
            q.Enqueue(task);
            var result = q.Dequeue();
            Assert.AreSame(task, result);
            Assert.IsTrue(q.IsEmpty);
        }

        // ── Priority ordering ────────────────────────────────────────────────────

        [TestMethod]
        public void Enqueue_HigherPriorityAdded_PeekReturnsHigher()
        {
            var q = new ActionQueue();
            var low  = T(1);
            var high = T(10);
            q.Enqueue(low);
            q.Enqueue(high);
            Assert.AreSame(high, q.TryPeekHighest());
        }

        [TestMethod]
        public void Enqueue_LowerPriorityAdded_PeekStillReturnsOriginal()
        {
            var q = new ActionQueue();
            var high = T(10);
            var low  = T(1);
            q.Enqueue(high);
            q.Enqueue(low);
            Assert.AreSame(high, q.TryPeekHighest());
        }

        [TestMethod]
        public void Dequeue_ReturnsTasksInPriorityDescendingOrder()
        {
            var q = new ActionQueue();
            var t1 = T(1);
            var t5 = T(5);
            var t9 = T(9);
            q.Enqueue(t1);
            q.Enqueue(t9);
            q.Enqueue(t5);

            Assert.AreSame(t9, q.Dequeue());
            Assert.AreSame(t5, q.Dequeue());
            Assert.AreSame(t1, q.Dequeue());
            Assert.IsNull(q.Dequeue());
        }

        [TestMethod]
        public void Enqueue_SamePriority_BothPresent()
        {
            var q = new ActionQueue();
            var a = T(5);
            var b = T(5);
            q.Enqueue(a);
            q.Enqueue(b);
            // Both should dequeue — order between equal-priority tasks is not specified
            var first  = q.Dequeue();
            var second = q.Dequeue();
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.IsTrue(q.IsEmpty);
        }

        // ── Clear ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Clear_RemovesAllTasks()
        {
            var q = new ActionQueue();
            q.Enqueue(T(1));
            q.Enqueue(T(2));
            q.Enqueue(T(3));
            q.Clear();
            Assert.IsTrue(q.IsEmpty);
            Assert.IsNull(q.TryPeekHighest());
        }
    }
}
