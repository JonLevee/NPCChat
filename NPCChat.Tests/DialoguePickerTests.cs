using NPCChat.Core.DialogueClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class DialoguePickerTests
    {
        private const string Speaker = "npc1";

        private static readonly float[] NeutralMood  = [];
        private static readonly IReadOnlyDictionary<string, float> NoStats =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        private readonly DialoguePicker _picker = new();
        private readonly CooldownTracker _cooldowns = new();

        private static DialoguePoolNode Pool(params DialoguePoolEntry[] entries)
            => TestEntryFactory.MakePool(entries);

        // ── All entries blocked ──────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_EmptyPool_ReturnsNull()
        {
            var node = Pool();
            var result = _picker.PickOne(node, NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNull(result);
        }

        // ── Single eligible entry ────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_SingleEntry_ReturnsIt()
        {
            var entry  = TestEntryFactory.MakeEntry("e1", "Hello");
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual("e1", result.Id);
        }

        // ── Mood affinity ────────────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_MoodAffinity_EntryBelowMinAffinity_Filtered()
        {
            // NPC mood: [1,0] (purely axis-0 oriented)
            // Entry mood: [0,1] (purely axis-1) → dot product = 0, below minAffinity=0.5
            var entry = TestEntryFactory.MakeEntry("e1",
                moodVector: new float[] { 0f, 1f },
                minAffinity: 0.5f);

            // Use a fallback-free scenario by also blocking fallback with same minAffinity
            // (fallback ignores mood, so we need weight=0 to also block fallback)
            var entryZeroWeight = TestEntryFactory.MakeEntry("e1",
                moodVector: new float[] { 0f, 1f },
                minAffinity: 0.5f,
                weight: 0f);

            var npcMood = new float[] { 1f, 0f };
            var result  = _picker.PickOne(Pool(entryZeroWeight), npcMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PickOne_MoodAffinity_EntryAboveMin_Selected()
        {
            // NPC mood [1,0], entry mood [1,0] → dot=1, well above minAffinity=0.5
            var entry  = TestEntryFactory.MakeEntry("e1",
                moodVector: new float[] { 1f, 0f },
                minAffinity: 0.5f);
            var npcMood = new float[] { 1f, 0f };

            var result = _picker.PickOne(Pool(entry), npcMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PickOne_EmptyMoodVectors_Returns05Affinity_PassesZeroMinAffinity()
        {
            // Both empty → Affinity01 returns 0.5, which passes minAffinity=0
            var entry  = TestEntryFactory.MakeEntry("e1", moodVector: [], minAffinity: 0f);
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
        }

        // ── Mood fallback ────────────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_AllEntriesFailMoodGate_FallsBackToWeightOnly()
        {
            // Entry has minAffinity=0.9, but affinity with perpendicular mood is ~0
            // Fallback (weight-only) should still pick it if weight > 0
            var entry   = TestEntryFactory.MakeEntry("e1",
                weight: 1f,
                moodVector: new float[] { 0f, 1f },
                minAffinity: 0.9f);
            var npcMood = new float[] { 1f, 0f };  // perpendicular

            var result = _picker.PickOne(Pool(entry), npcMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
            Assert.AreEqual("e1", result.Id);
        }

        // ── Hard gates: Requires ─────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_Requires_StatMissing_Filtered()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                requires: new Dictionary<string, MinMax>
                {
                    ["strength"] = new MinMax(0.5f, true, 1f, true)
                });
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PickOne_Requires_StatInRange_Passes()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                requires: new Dictionary<string, MinMax>
                {
                    ["strength"] = new MinMax(0.5f, true, 1f, true)
                });
            var stats = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["strength"] = 0.8f
            };
            var result = _picker.PickOne(Pool(entry), NeutralMood, stats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PickOne_Requires_StatOutOfRange_Filtered()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                requires: new Dictionary<string, MinMax>
                {
                    ["strength"] = new MinMax(0.5f, true, 1f, true)
                });
            var stats = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["strength"] = 0.3f
            };
            var result = _picker.PickOne(Pool(entry), NeutralMood, stats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNull(result);
        }

        // ── Hard gates: Forbids ──────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_Forbids_StatInForbiddenRange_Filtered()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                forbids: new Dictionary<string, MinMax>
                {
                    ["hostile"] = new MinMax(0.8f, true, 1f, true)
                });
            var stats = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["hostile"] = 0.9f
            };
            var result = _picker.PickOne(Pool(entry), NeutralMood, stats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PickOne_Forbids_StatNotInForbiddenRange_Passes()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                forbids: new Dictionary<string, MinMax>
                {
                    ["hostile"] = new MinMax(0.8f, true, 1f, true)
                });
            var stats = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["hostile"] = 0.3f
            };
            var result = _picker.PickOne(Pool(entry), NeutralMood, stats, null,
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
        }

        // ── Intent filtering ─────────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_IntentFilter_EntryHasNoTags_Passes()
        {
            // Entry with no intent tags matches any desired intent
            var entry  = TestEntryFactory.MakeEntry("e1", intent: []);
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, "shop",
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PickOne_IntentFilter_EntryMatchesDesired_Passes()
        {
            var entry  = TestEntryFactory.MakeEntry("e1", intent: ["shop", "trade"]);
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, "shop",
                Speaker, _cooldowns, 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PickOne_IntentFilter_EntryMismatches_Filtered()
        {
            var entry  = TestEntryFactory.MakeEntry("e1", intent: ["quest"]);
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, "shop",
                Speaker, _cooldowns, 0);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PickOne_IntentFilter_NullDesired_AllPassThrough()
        {
            var e1 = TestEntryFactory.MakeEntry("e1", intent: ["shop"]);
            var e2 = TestEntryFactory.MakeEntry("e2", intent: ["quest"]);
            var e3 = TestEntryFactory.MakeEntry("e3", intent: []);
            // null desired intent → PassIntent always returns true
            var result = _picker.PickOne(Pool(e1, e2, e3), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0, rng: new Random(1));
            Assert.IsNotNull(result);
        }

        // ── Cooldown filtering ───────────────────────────────────────────────────

        [TestMethod]
        public void PickOne_SelfCooldown_BlocksEntry()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                cooldown: new CooldownSpec { SelfSeconds = 60f });
            _cooldowns.MarkUsed(Speaker, entry, 1000);
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 1010);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PickOne_SelfCooldown_Expired_Passes()
        {
            var entry = TestEntryFactory.MakeEntry("e1",
                cooldown: new CooldownSpec { SelfSeconds = 60f });
            _cooldowns.MarkUsed(Speaker, entry, 1000);
            var result = _picker.PickOne(Pool(entry), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 1060);
            Assert.IsNotNull(result);
        }

        // ── Deterministic selection ──────────────────────────────────────────────

        [TestMethod]
        public void PickOne_SeededRng_IsDeterministic()
        {
            var e1 = TestEntryFactory.MakeEntry("e1", "Hello",  weight: 1f);
            var e2 = TestEntryFactory.MakeEntry("e2", "Howdy",  weight: 1f);
            var e3 = TestEntryFactory.MakeEntry("e3", "Greetings", weight: 1f);
            var pool = Pool(e1, e2, e3);

            var r1 = _picker.PickOne(pool, NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0, rng: new Random(42));
            var r2 = _picker.PickOne(pool, NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0, rng: new Random(42));

            Assert.IsNotNull(r1);
            Assert.AreEqual(r1!.Id, r2!.Id);
        }

        [TestMethod]
        public void PickOne_TopN_LimitsConsideredEntries()
        {
            // 10 entries, topN=2 — should still return something
            var entries = Enumerable.Range(0, 10)
                .Select(i => TestEntryFactory.MakeEntry($"e{i}", weight: 1f))
                .ToArray();
            var result = _picker.PickOne(Pool(entries), NeutralMood, NoStats, null,
                Speaker, _cooldowns, 0, topN: 2, rng: new Random(1));
            Assert.IsNotNull(result);
        }
    }
}
