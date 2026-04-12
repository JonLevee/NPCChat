using NPCChat.Core.AIClasses;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.CombatClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class AttackTaskTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────────

        private static readonly ObjectHandle PlayerHandle = new(1, 1);
        private static readonly ObjectHandle EnemyHandle  = new(2, 1);

        private static WorldObjectMoveable MakeActor(
            Bounds bounds,
            CombatStats? combat    = null,
            ObjectHandle handle    = default)
        {
            var actor = new WorldObjectMoveable
            {
                Kind    = WorldObjectKind.NPC,
                Bounds  = bounds,
                Handle  = handle,
                Combat  = combat,
                Actor   = new NPCChat.Core.BehaviorClasses.ActorComponent()
            };
            return actor;
        }

        /// <summary>
        /// Builds a SimContext that places the enemy at <paramref name="enemyBounds"/>
        /// and the player at <paramref name="playerBounds"/>. GetMoveable resolves
        /// <see cref="PlayerHandle"/> to <paramref name="player"/> (may be null).
        /// </summary>
        private static SimContext MakeCtx(
            WorldObjectMoveable enemy,
            Bounds? playerBounds,
            WorldObjectMoveable? player             = null,
            List<AlertEvent>?    capturedAlerts     = null)
        {
            Action<AlertEvent> postAlert = capturedAlerts is null
                ? _ => { }
                : e => capturedAlerts.Add(e);

            return new SimContext(
                actor:            enemy,
                playerBounds:     playerBounds,
                gameTick:         0,
                gameHour:         12,
                enqueueMove:      _ => { },
                postAlert:        postAlert,
                playerHandle:     playerBounds.HasValue ? PlayerHandle : null,
                getMoveable:      h => h == PlayerHandle ? player : null);
        }

        // ── No-op conditions ─────────────────────────────────────────────────────

        [TestMethod]
        public void Tick_NoCombatStats_ReturnsDone()
        {
            var enemy = MakeActor(new Bounds(0, 0, 1, 1), combat: null, handle: EnemyHandle);
            var ctx   = MakeCtx(enemy, new Bounds(1, 0, 2, 1));

            var task  = new AttackTask();
            task.Begin(ctx);
            bool done = task.Tick(ctx);
            Assert.IsTrue(done);
        }

        [TestMethod]
        public void Tick_NoPlayerBounds_ReturnsDone()
        {
            var combat = new CombatStats(maxHp: 100, attackRange: 2);
            var enemy  = MakeActor(new Bounds(0, 0, 1, 1), combat: combat, handle: EnemyHandle);
            var ctx    = MakeCtx(enemy, playerBounds: null);

            var task = new AttackTask();
            task.Begin(ctx);
            bool done = task.Tick(ctx);
            Assert.IsTrue(done);
        }

        [TestMethod]
        public void Tick_PlayerOutOfRange_ReturnsDone()
        {
            var combat = new CombatStats(maxHp: 100, attackRange: 1);
            var enemy  = MakeActor(new Bounds(0, 0, 1, 1), combat: combat, handle: EnemyHandle);
            // player is 5 tiles away — beyond range 1
            var ctx    = MakeCtx(enemy, new Bounds(5, 5, 6, 6));

            var task = new AttackTask();
            task.Begin(ctx);
            bool done = task.Tick(ctx);
            Assert.IsTrue(done);
        }

        // ── Attack fires ─────────────────────────────────────────────────────────

        [TestMethod]
        public void Tick_PlayerInRange_DealsDamage()
        {
            var playerCombat = new CombatStats(maxHp: 100);
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: playerCombat, handle: PlayerHandle);

            var myCombat = new CombatStats(maxHp: 50, damage: 15, attackRange: 2);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var ctx  = MakeCtx(enemy, player.Bounds, player);
            var task = new AttackTask();
            task.Begin(ctx);
            task.Tick(ctx);

            Assert.AreEqual(85, playerCombat.CurrentHp);
        }

        [TestMethod]
        public void Tick_PlayerInRange_ReturnsContinue()
        {
            var playerCombat = new CombatStats(maxHp: 100);
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: playerCombat, handle: PlayerHandle);

            var myCombat = new CombatStats(maxHp: 50, damage: 10, attackRange: 2);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var ctx  = MakeCtx(enemy, player.Bounds, player);
            var task = new AttackTask();
            task.Begin(ctx);
            bool done = task.Tick(ctx);
            Assert.IsFalse(done);
        }

        [TestMethod]
        public void Tick_PlayerInRange_NoCombatOnPlayer_NoCrash()
        {
            // Player exists but has no CombatStats — attack should still not throw
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: null, handle: PlayerHandle);

            var myCombat = new CombatStats(maxHp: 50, damage: 10, attackRange: 2);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var ctx  = MakeCtx(enemy, player.Bounds, player);
            var task = new AttackTask();
            task.Begin(ctx);
            // Should not throw; nothing to assert beyond no exception
            task.Tick(ctx);
        }

        // ── Cooldown ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void Tick_AfterAttack_CooldownBlocksNextHit()
        {
            var playerCombat = new CombatStats(maxHp: 100);
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: playerCombat, handle: PlayerHandle);

            // cooldown = 3 ticks after each attack
            // Tick sequence: fire(t1), block(t2), block(t3), fire(t4), ...
            var myCombat = new CombatStats(maxHp: 50, damage: 10, attackRange: 2, attackCooldownTicks: 3);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var ctx  = MakeCtx(enemy, player.Bounds, player);
            var task = new AttackTask();
            task.Begin(ctx);

            // t1 — cooldown starts at 0 so attack fires immediately
            task.Tick(ctx);
            Assert.AreEqual(90, playerCombat.CurrentHp);

            // t2, t3 — cooldown counting down (3→2→1), no attack
            task.Tick(ctx);
            task.Tick(ctx);
            Assert.AreEqual(90, playerCombat.CurrentHp);

            // t4 — TickCooldown brings it to 0, CanAttack = true, attack fires
            task.Tick(ctx);
            Assert.AreEqual(80, playerCombat.CurrentHp);
        }

        // ── Alert ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Tick_Attack_PostsCombatNoiseAlert()
        {
            var playerCombat = new CombatStats(maxHp: 100);
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: playerCombat, handle: PlayerHandle);

            var myCombat = new CombatStats(maxHp: 50, damage: 10, attackRange: 2);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var alerts = new List<AlertEvent>();
            var ctx    = MakeCtx(enemy, player.Bounds, player, capturedAlerts: alerts);
            var task   = new AttackTask();
            task.Begin(ctx);
            task.Tick(ctx);

            Assert.AreEqual(1, alerts.Count);
            Assert.AreEqual(AlertKind.CombatNoise, alerts[0].Kind);
        }

        [TestMethod]
        public void Tick_DuringCooldown_NoAlert()
        {
            var playerCombat = new CombatStats(maxHp: 100);
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: playerCombat, handle: PlayerHandle);

            var myCombat = new CombatStats(maxHp: 50, damage: 10, attackRange: 2, attackCooldownTicks: 5);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var alerts = new List<AlertEvent>();
            var ctx    = MakeCtx(enemy, player.Bounds, player, capturedAlerts: alerts);
            var task   = new AttackTask();
            task.Begin(ctx);
            task.Tick(ctx);         // fires
            alerts.Clear();
            task.Tick(ctx);         // on cooldown — no alert
            Assert.AreEqual(0, alerts.Count);
        }

        // ── Death ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Tick_KillsPlayer_IsDead()
        {
            var playerCombat = new CombatStats(maxHp: 5);
            var player = MakeActor(new Bounds(1, 0, 2, 1), combat: playerCombat, handle: PlayerHandle);

            var myCombat = new CombatStats(maxHp: 50, damage: 999, attackRange: 2);
            var enemy    = MakeActor(new Bounds(0, 0, 1, 1), combat: myCombat, handle: EnemyHandle);

            var ctx  = MakeCtx(enemy, player.Bounds, player);
            var task = new AttackTask();
            task.Begin(ctx);
            task.Tick(ctx);

            Assert.IsTrue(playerCombat.IsDead);
        }
    }
}
