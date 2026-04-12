using NPCChat.Core.CombatClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class CombatStatsTests
    {
        // ── Construction ─────────────────────────────────────────────────────────

        [TestMethod]
        public void Constructor_SetsCurrentHpToMaxHp()
        {
            var cs = new CombatStats(maxHp: 100);
            Assert.AreEqual(100, cs.CurrentHp);
            Assert.AreEqual(100, cs.MaxHp);
        }

        [TestMethod]
        public void Constructor_ZeroMaxHp_Throws()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CombatStats(maxHp: 0));
        }

        [TestMethod]
        public void Constructor_NegativeMaxHp_Throws()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new CombatStats(maxHp: -1));
        }

        // ── IsDead ───────────────────────────────────────────────────────────────

        [TestMethod]
        public void IsDead_FullHealth_ReturnsFalse()
        {
            var cs = new CombatStats(maxHp: 100);
            Assert.IsFalse(cs.IsDead);
        }

        [TestMethod]
        public void IsDead_ZeroHp_ReturnsTrue()
        {
            var cs = new CombatStats(maxHp: 100);
            cs.TakeDamage(100);
            Assert.IsTrue(cs.IsDead);
        }

        // ── TakeDamage ───────────────────────────────────────────────────────────

        [TestMethod]
        public void TakeDamage_ReducesHp()
        {
            var cs = new CombatStats(maxHp: 100);
            cs.TakeDamage(30);
            Assert.AreEqual(70, cs.CurrentHp);
        }

        [TestMethod]
        public void TakeDamage_ClampsAtZero()
        {
            var cs = new CombatStats(maxHp: 100);
            cs.TakeDamage(999);
            Assert.AreEqual(0, cs.CurrentHp);
        }

        [TestMethod]
        public void TakeDamage_Zero_NoChange()
        {
            var cs = new CombatStats(maxHp: 100);
            cs.TakeDamage(0);
            Assert.AreEqual(100, cs.CurrentHp);
        }

        [TestMethod]
        public void TakeDamage_Negative_Throws()
        {
            var cs = new CombatStats(maxHp: 100);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => cs.TakeDamage(-1));
        }

        // ── Heal ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Heal_IncreasesHp()
        {
            var cs = new CombatStats(maxHp: 100);
            cs.TakeDamage(50);
            cs.Heal(20);
            Assert.AreEqual(70, cs.CurrentHp);
        }

        [TestMethod]
        public void Heal_ClampsAtMaxHp()
        {
            var cs = new CombatStats(maxHp: 100);
            cs.TakeDamage(10);
            cs.Heal(999);
            Assert.AreEqual(100, cs.CurrentHp);
        }

        [TestMethod]
        public void Heal_Negative_Throws()
        {
            var cs = new CombatStats(maxHp: 100);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => cs.Heal(-1));
        }

        // ── Cooldown ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void CanAttack_InitiallyTrue()
        {
            var cs = new CombatStats(maxHp: 100, attackCooldownTicks: 3);
            Assert.IsTrue(cs.CanAttack);
        }

        [TestMethod]
        public void ResetCooldown_BlocksAttack()
        {
            var cs = new CombatStats(maxHp: 100, attackCooldownTicks: 3);
            cs.ResetCooldown();
            Assert.IsFalse(cs.CanAttack);
        }

        [TestMethod]
        public void TickCooldown_AfterReset_EventuallyAllowsAttack()
        {
            var cs = new CombatStats(maxHp: 100, attackCooldownTicks: 3);
            cs.ResetCooldown();
            cs.TickCooldown();
            cs.TickCooldown();
            Assert.IsFalse(cs.CanAttack);
            cs.TickCooldown();
            Assert.IsTrue(cs.CanAttack);
        }

        [TestMethod]
        public void TickCooldown_BelowZero_StaysReady()
        {
            var cs = new CombatStats(maxHp: 100, attackCooldownTicks: 1);
            cs.TickCooldown();   // already 0 — no-op
            cs.TickCooldown();
            Assert.IsTrue(cs.CanAttack);
        }
    }
}
