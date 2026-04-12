using NPCChat.Core.DialogueClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class CooldownTrackerTests
    {
        private const string Speaker = "npc1";

        private static DialoguePoolEntry Entry(
            string id,
            float selfSeconds = 0f,
            string? group = null,
            float groupSeconds = 0f)
            => TestEntryFactory.MakeEntry(id, cooldown: new CooldownSpec
            {
                SelfSeconds  = selfSeconds,
                Group        = group,
                GroupSeconds = groupSeconds
            });

        // ── No cooldown ───────────────────────────────────────────────────────────

        [TestMethod]
        public void IsOffCooldown_NoCooldownSpec_AlwaysTrue()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1");
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, entry, 1000));
            tracker.MarkUsed(Speaker, entry, 1000);
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, entry, 1000));
        }

        // ── Self cooldown ─────────────────────────────────────────────────────────

        [TestMethod]
        public void IsOffCooldown_SelfCooldown_BeforeMarkUsed_ReturnsTrue()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", selfSeconds: 30f);
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, entry, 0));
        }

        [TestMethod]
        public void IsOffCooldown_SelfCooldown_JustUsed_ReturnsFalse()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", selfSeconds: 30f);
            tracker.MarkUsed(Speaker, entry, 1000);
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, entry, 1005));
        }

        [TestMethod]
        public void IsOffCooldown_SelfCooldown_Expired_ReturnsTrue()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", selfSeconds: 30f);
            tracker.MarkUsed(Speaker, entry, 1000);
            // 30 seconds later — exactly at boundary
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, entry, 1030));
        }

        [TestMethod]
        public void IsOffCooldown_SelfCooldown_NotExpiredYet_ReturnsFalse()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", selfSeconds: 30f);
            tracker.MarkUsed(Speaker, entry, 1000);
            // Only 29 seconds later
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, entry, 1029));
        }

        [TestMethod]
        public void IsOffCooldown_SelfCooldown_DifferentSpeaker_IndependentCooldowns()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", selfSeconds: 30f);
            tracker.MarkUsed("npc1", entry, 1000);
            // npc2 has not used this entry — should be off cooldown
            Assert.IsTrue(tracker.IsOffCooldown("npc2", entry, 1005));
        }

        // ── Group cooldown ────────────────────────────────────────────────────────

        [TestMethod]
        public void IsOffCooldown_GroupCooldown_BeforeMarkUsed_ReturnsTrue()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", group: "greetings", groupSeconds: 60f);
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, entry, 0));
        }

        [TestMethod]
        public void IsOffCooldown_GroupCooldown_JustUsed_ReturnsFalse()
        {
            var tracker = new CooldownTracker();
            var e1 = Entry("e1", group: "greetings", groupSeconds: 60f);
            var e2 = Entry("e2", group: "greetings", groupSeconds: 60f);
            tracker.MarkUsed(Speaker, e1, 1000);
            // e2 is in same group — should be blocked
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, e2, 1005));
        }

        [TestMethod]
        public void IsOffCooldown_GroupCooldown_Expired_ReturnsTrue()
        {
            var tracker = new CooldownTracker();
            var e1 = Entry("e1", group: "greetings", groupSeconds: 60f);
            var e2 = Entry("e2", group: "greetings", groupSeconds: 60f);
            tracker.MarkUsed(Speaker, e1, 1000);
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, e2, 1060));
        }

        [TestMethod]
        public void IsOffCooldown_GroupCooldown_DifferentGroup_NotBlocked()
        {
            var tracker = new CooldownTracker();
            var e1 = Entry("e1", group: "greetings", groupSeconds: 60f);
            var e2 = Entry("e2", group: "farewells", groupSeconds: 60f);
            tracker.MarkUsed(Speaker, e1, 1000);
            // Different group — e2 should not be blocked
            Assert.IsTrue(tracker.IsOffCooldown(Speaker, e2, 1005));
        }

        [TestMethod]
        public void MarkUsed_GroupCooldown_AllGroupMembersBlocked()
        {
            var tracker = new CooldownTracker();
            var e1 = Entry("e1", group: "greetings", groupSeconds: 60f);
            var e2 = Entry("e2", group: "greetings", groupSeconds: 60f);
            var e3 = Entry("e3", group: "greetings", groupSeconds: 60f);
            tracker.MarkUsed(Speaker, e1, 1000);
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, e2, 1005));
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, e3, 1005));
        }

        // ── Both cooldowns ────────────────────────────────────────────────────────

        [TestMethod]
        public void MarkUsed_BothCooldowns_BothRecorded()
        {
            var tracker = new CooldownTracker();
            var entry   = Entry("e1", selfSeconds: 10f, group: "greetings", groupSeconds: 30f);
            tracker.MarkUsed(Speaker, entry, 1000);

            // Same entry blocked by self cooldown
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, entry, 1005));

            // Another entry in group also blocked by group cooldown
            var e2 = Entry("e2", group: "greetings", groupSeconds: 30f);
            Assert.IsFalse(tracker.IsOffCooldown(Speaker, e2, 1005));
        }
    }
}
