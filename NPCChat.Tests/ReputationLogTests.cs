using NPCChat.Core.FactionClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class ReputationLogTests
    {
        private static FactionDef DefaultFaction(string id = "guards")
            => new FactionDef
            {
                Id               = id,
                FriendlyThreshold = 50,
                HostileThreshold  = -25
            };

        // ── GetReputation ────────────────────────────────────────────────────────

        [TestMethod]
        public void GetReputation_UnknownFaction_ReturnsZero()
        {
            var log = new ReputationLog();
            Assert.AreEqual(0, log.GetReputation("unknown"));
        }

        [TestMethod]
        public void GetReputation_KnownFaction_ReturnsCurrentScore()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", 100);
            Assert.AreEqual(100, log.GetReputation("guards"));
        }

        // ── AddReputation ────────────────────────────────────────────────────────

        [TestMethod]
        public void AddReputation_Positive_IncrementsScore()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", 50);
            log.AddReputation("guards", 30);
            Assert.AreEqual(80, log.GetReputation("guards"));
        }

        [TestMethod]
        public void AddReputation_Negative_DecrementsScore()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", 100);
            log.AddReputation("guards", -40);
            Assert.AreEqual(60, log.GetReputation("guards"));
        }

        [TestMethod]
        public void AddReputation_ClampsAtPositiveMax()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", 1500);
            Assert.AreEqual(1000, log.GetReputation("guards"));
        }

        [TestMethod]
        public void AddReputation_ClampsAtNegativeMin()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", -2000);
            Assert.AreEqual(-1000, log.GetReputation("guards"));
        }

        [TestMethod]
        public void AddReputation_ExactlyAtMax_StaysAtMax()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", 1000);
            Assert.AreEqual(1000, log.GetReputation("guards"));
        }

        [TestMethod]
        public void AddReputation_ExactlyAtMin_StaysAtMin()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", -1000);
            Assert.AreEqual(-1000, log.GetReputation("guards"));
        }

        // ── GetTier ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void GetTier_Exalted_AtThreshold()
        {
            var log = new ReputationLog();
            var def = DefaultFaction();
            log.AddReputation("guards", 300);
            Assert.AreEqual(ReputationTier.Exalted, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Revered_AtThreshold()
        {
            var log = new ReputationLog();
            var def = DefaultFaction();
            log.AddReputation("guards", 200);
            Assert.AreEqual(ReputationTier.Revered, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Honored_AtThreshold()
        {
            var log = new ReputationLog();
            var def = DefaultFaction();
            log.AddReputation("guards", 100);
            Assert.AreEqual(ReputationTier.Honored, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Friendly_AtThreshold()
        {
            var log = new ReputationLog();
            var def = DefaultFaction(); // FriendlyThreshold = 50
            log.AddReputation("guards", 50);
            Assert.AreEqual(ReputationTier.Friendly, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Neutral_AtZeroScore()
        {
            var log = new ReputationLog();
            var def = DefaultFaction();
            // score 0: >= HostileThreshold(-25) and < FriendlyThreshold(50) and >= 0
            Assert.AreEqual(ReputationTier.Neutral, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Unfriendly_NegativeButAboveHostile()
        {
            var log = new ReputationLog();
            var def = DefaultFaction(); // HostileThreshold = -25
            log.AddReputation("guards", -10);
            // score -10: >= HostileThreshold(-25), < 0 → Unfriendly
            Assert.AreEqual(ReputationTier.Unfriendly, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Hostile_AtThreshold()
        {
            // HostileThreshold = -25: score >= -25 is Unfriendly; score < -25 is Hostile
            var log = new ReputationLog();
            var def = DefaultFaction();
            log.AddReputation("guards", -26);
            Assert.AreEqual(ReputationTier.Hostile, log.GetTier(def));
        }

        [TestMethod]
        public void GetTier_Hostile_BelowThreshold()
        {
            var log = new ReputationLog();
            var def = DefaultFaction();
            log.AddReputation("guards", -100);
            Assert.AreEqual(ReputationTier.Hostile, log.GetTier(def));
        }

        // ── AllScores ────────────────────────────────────────────────────────────

        [TestMethod]
        public void AllScores_ReflectsAllFactions()
        {
            var log = new ReputationLog();
            log.AddReputation("guards", 100);
            log.AddReputation("thieves", -50);
            Assert.AreEqual(2, log.AllScores.Count);
            Assert.AreEqual(100, log.AllScores["guards"]);
            Assert.AreEqual(-50, log.AllScores["thieves"]);
        }

        // ── Case insensitivity ───────────────────────────────────────────────────

        [TestMethod]
        public void AddReputation_CaseInsensitive_SameEntry()
        {
            var log = new ReputationLog();
            log.AddReputation("Guards", 50);
            log.AddReputation("GUARDS", 50);
            Assert.AreEqual(100, log.GetReputation("guards"));
        }

        [TestMethod]
        public void GetReputation_CaseInsensitive()
        {
            var log = new ReputationLog();
            log.AddReputation("Guards", 75);
            Assert.AreEqual(75, log.GetReputation("GUARDS"));
        }
    }
}
