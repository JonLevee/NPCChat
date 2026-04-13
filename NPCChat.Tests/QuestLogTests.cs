using NPCChat.Core.QuestClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class QuestLogTests
    {
        private static QuestDef Def(string id) => new QuestDef { Id = id, Name = id };

        // ── Initial state ────────────────────────────────────────────────────────

        [TestMethod]
        public void ActiveQuests_InitiallyEmpty()
        {
            var log = new QuestLog();
            Assert.IsEmpty(log.ActiveQuests);
        }

        [TestMethod]
        public void HasActiveQuest_NeverStarted_ReturnsFalse()
        {
            var log = new QuestLog();
            Assert.IsFalse(log.HasActiveQuest("quest1"));
        }

        [TestMethod]
        public void HasCompletedQuest_NeverStarted_ReturnsFalse()
        {
            var log = new QuestLog();
            Assert.IsFalse(log.HasCompletedQuest("quest1"));
        }

        [TestMethod]
        public void CanStartQuest_NeverStarted_ReturnsTrue()
        {
            var log = new QuestLog();
            Assert.IsTrue(log.CanStartQuest("quest1"));
        }

        // ── StartQuest ───────────────────────────────────────────────────────────

        [TestMethod]
        public void StartQuest_AddsToActiveList()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            Assert.HasCount(1, log.ActiveQuests);
            Assert.IsTrue(log.HasActiveQuest("q1"));
        }

        [TestMethod]
        public void StartQuest_AlreadyActive_IsIdempotent()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.StartQuest(Def("q1"));
            Assert.HasCount(1, log.ActiveQuests);
        }

        [TestMethod]
        public void StartQuest_AlreadyCompleted_IsNoOp()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.TryComplete("q1");
            log.StartQuest(Def("q1"));         // should be ignored
            Assert.IsEmpty(log.ActiveQuests);
            Assert.IsTrue(log.HasCompletedQuest("q1"));
        }

        [TestMethod]
        public void CanStartQuest_AlreadyActive_ReturnsFalse()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            Assert.IsFalse(log.CanStartQuest("q1"));
        }

        [TestMethod]
        public void CanStartQuest_AlreadyCompleted_ReturnsFalse()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.TryComplete("q1");
            Assert.IsFalse(log.CanStartQuest("q1"));
        }

        // ── TryComplete ──────────────────────────────────────────────────────────

        [TestMethod]
        public void TryComplete_ActiveQuest_ReturnsTrue()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            Assert.IsTrue(log.TryComplete("q1"));
        }

        [TestMethod]
        public void TryComplete_ActiveQuest_RemovesFromActive()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.TryComplete("q1");
            Assert.IsEmpty(log.ActiveQuests);
            Assert.IsFalse(log.HasActiveQuest("q1"));
        }

        [TestMethod]
        public void TryComplete_ActiveQuest_AddsToCompleted()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.TryComplete("q1");
            Assert.IsTrue(log.HasCompletedQuest("q1"));
        }

        [TestMethod]
        public void TryComplete_InactiveQuest_ReturnsFalse()
        {
            var log = new QuestLog();
            Assert.IsFalse(log.TryComplete("q1"));
        }

        [TestMethod]
        public void TryComplete_AlreadyCompleted_ReturnsFalse()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.TryComplete("q1");
            Assert.IsFalse(log.TryComplete("q1"));
        }

        // ── Case insensitivity ───────────────────────────────────────────────────

        [TestMethod]
        public void HasActiveQuest_CaseInsensitive()
        {
            var log = new QuestLog();
            log.StartQuest(Def("FindSword"));
            Assert.IsTrue(log.HasActiveQuest("findsword"));
            Assert.IsTrue(log.HasActiveQuest("FINDSWORD"));
        }

        [TestMethod]
        public void TryComplete_CaseInsensitive()
        {
            var log = new QuestLog();
            log.StartQuest(Def("FindSword"));
            Assert.IsTrue(log.TryComplete("FINDSWORD"));
            Assert.IsTrue(log.HasCompletedQuest("findsword"));
        }

        // ── Multiple quests ──────────────────────────────────────────────────────

        [TestMethod]
        public void ActiveQuests_MultipleQuests_AllPresent()
        {
            var log = new QuestLog();
            log.StartQuest(Def("q1"));
            log.StartQuest(Def("q2"));
            log.StartQuest(Def("q3"));
            Assert.HasCount(3, log.ActiveQuests);
        }
    }
}
