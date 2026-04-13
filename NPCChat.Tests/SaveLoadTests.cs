#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.Builders;
using NPCChat.Core.CombatClasses;
using NPCChat.Core.Extensions;
using NPCChat.Core.LoadingProviderClasses;
using NPCChat.Core.SaveClasses;
using NPCChat.Core.WorldBuilderTemplates;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    /// <summary>
    /// Round-trip tests: build a world, mutate state, serialize, restore onto a
    /// fresh world, and assert the state matches.
    /// </summary>
    [TestClass]
    public sealed class SaveLoadTests : UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize() => TestInitializeBase();

        // ── World builder ────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the canonical test world. Call in both save and load scopes to get
        /// identical WorldId assignments.
        ///
        /// Object creation order (and therefore WorldId sequence):
        ///   1 — Player  (2×2 at 5,5)
        ///   2 — Guard   (2×2 at 20,20) — has WaypointPatrolTask + DialogueCooldowns
        ///   3 — Mob     (2×2 at 30,30) — has CombatStats (MaxHp=100)
        ///   4 — Coin    (1×1 at 40,40) — carryable, Quantity=15
        /// </summary>
        private static void BuildWorld(IServiceProvider sp)
        {
            var builder = sp.Get<WorldDataBuilder>()!;

            // Register static data (singleton — safe to call repeatedly; overwrites are idempotent).
            using var t = builder.GetTemplates();
            t.RegisterSeedItems()
             .RegisterSeedFactions()
             .RegisterSeedQuests()
             .AddPlayer(5, 5)
             .AddGuard(20, 20, patrolRadius: 3);

            // Mob with CombatStats for HP round-trip test.
            builder.World.AddObject(new WorldObjectMoveable
            {
                Kind     = WorldObjectKind.NPC,
                Handle   = ObjectHandle.None,
                Bounds   = new Bounds(30, 30, 32, 32),
                MaxSpeed = 4f,
                Actor    = new ActorComponent { Mode = "Idle" },
                Combat   = new CombatStats(maxHp: 100),
            });

            // Loose item for carryable quantity test.
            var coin = builder.StaticData.GetItem("gold_coin")!;
            builder.World.AddObject(new WorldObjectCarryable
            {
                Kind     = WorldObjectKind.Item,
                Handle   = ObjectHandle.None,
                Bounds   = new Bounds(40, 40, 41, 41),
                ItemDef  = coin,
                Quantity = 15,
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static WorldObjectMoveable GetPlayer(WorldData w) =>
            w.EnumerateWorldObjects()
             .OfType<WorldObjectMoveable>()
             .First(m => m.Kind == WorldObjectKind.Player);

        /// <summary>Returns the NPC with the highest WorldId of Kind==NPC (the Mob we created last).</summary>
        private static WorldObjectMoveable GetMob(WorldData w) =>
            w.EnumerateWorldObjects()
             .OfType<WorldObjectMoveable>()
             .Where(m => m.Kind == WorldObjectKind.NPC && m.Combat is not null)
             .First();

        private static WorldObjectMoveable GetGuard(WorldData w) =>
            w.EnumerateWorldObjects()
             .OfType<WorldObjectMoveable>()
             .First(m => m.Kind == WorldObjectKind.NPC && m.Actor?.ActionQueue.IsEmpty == false);

        private static WorldObjectCarryable GetCoin(WorldData w) =>
            w.EnumerateWorldObjects()
             .OfType<WorldObjectCarryable>()
             .First();

        // ── Tests ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_CarryableQuantity()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                GetCoin(world1).Quantity = 42;
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2    = sp2.Get<WorldData>()!;
            var handles2  = sp2.Get<ObjectHandleManager>()!;
            var static2   = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            Assert.AreEqual(42, GetCoin(world2).Quantity);
        }

        [TestMethod]
        public void RoundTrip_MoveablePosition()
        {
            string json;
            Bounds movedBounds;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1   = sp1.Get<WorldData>()!;
                var handles1 = sp1.Get<ObjectHandleManager>()!;
                var guard    = GetGuard(world1);
                movedBounds  = new Bounds(50, 50, 52, 52);
                world1.MoveDynamicObjectBetweenChunks(guard.Handle, movedBounds);
                json = sp1.Get<SaveGameService>()!.Serialize(world1, handles1);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            Assert.AreEqual(movedBounds, GetGuard(world2).Bounds);
        }

        [TestMethod]
        public void RoundTrip_NpcHp()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                GetMob(world1).Combat!.TakeDamage(30); // 100 − 30 = 70
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            Assert.AreEqual(70, GetMob(world2).Combat!.CurrentHp);
        }

        [TestMethod]
        public void RoundTrip_PlayerInventory()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1   = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                var player = GetPlayer(world1);
                var iron   = sp1.Get<StaticData>()!.GetItem("iron_ore")!;
                player.Inventory!.TryAdd(iron, 7, out _);
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            var inv = GetPlayer(world2).Inventory!;
            Assert.AreEqual(1, inv.Slots.Count);
            Assert.AreEqual("iron_ore", inv.Slots[0].Item.Id);
            Assert.AreEqual(7, inv.Slots[0].Quantity);
        }

        [TestMethod]
        public void RoundTrip_PlayerQuestLog()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1   = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                var staticData = sp1.Get<StaticData>()!;
                var player = GetPlayer(world1);
                var quest  = staticData.GetQuest("fetch_iron_ore")!;
                player.QuestLog!.StartQuest(quest);
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            var log = GetPlayer(world2).QuestLog!;
            Assert.IsTrue(log.HasActiveQuest("fetch_iron_ore"));
        }

        [TestMethod]
        public void RoundTrip_CompletedQuestSurvivesLoad()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                var player = GetPlayer(world1);
                var quest  = sp1.Get<StaticData>()!.GetQuest("fetch_iron_ore")!;
                player.QuestLog!.StartQuest(quest);
                player.QuestLog!.TryComplete("fetch_iron_ore");
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            var log = GetPlayer(world2).QuestLog!;
            Assert.IsFalse(log.HasActiveQuest("fetch_iron_ore"),  "Quest should not be active after completion.");
            Assert.IsTrue(log.HasCompletedQuest("fetch_iron_ore"), "Quest should appear in completed list.");
        }

        [TestMethod]
        public void RoundTrip_PlayerReputation()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                GetPlayer(world1).ReputationLog!.AddReputation("townsfolk", 150);
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            Assert.AreEqual(150, GetPlayer(world2).ReputationLog!.GetReputation("townsfolk"));
        }

        [TestMethod]
        public void RoundTrip_NpcActionQueueToken()
        {
            string json;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                // Guard has a WaypointPatrolTask enqueued by AddGuard.
                var guard = GetGuard(world1);
                Assert.IsFalse(guard.Actor!.ActionQueue.IsEmpty, "Guard should have a task queued.");
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            var guard2 = GetGuard(world2);
            Assert.IsFalse(guard2.Actor!.ActionQueue.IsEmpty, "Action queue should be restored.");
            var task = guard2.Actor.ActionQueue.TryPeekHighest()!;
            Assert.IsInstanceOfType<WaypointPatrolTask>(task, "Highest-priority task should be WaypointPatrolTask.");
        }

        [TestMethod]
        public void RoundTrip_NpcCooldowns()
        {
            string json;
            const string selfKey  = "npc_h1||entryA";
            const string groupKey = "npc_h1||G||greetings";
            const double timestamp = 500.0;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1 = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1 = sp1.Get<WorldData>()!;
                var guard  = GetGuard(world1);
                // Seed cooldown state directly via RestoreState (bypasses needing a live session).
                guard.Actor!.DialogueCooldowns.RestoreState(
                    new[] { new KeyValuePair<string, double>(selfKey, timestamp) },
                    new[] { new KeyValuePair<string, double>(groupKey, timestamp) });
                json = sp1.Get<SaveGameService>()!.Serialize(world1, sp1.Get<ObjectHandleManager>()!);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            var (self, group) = GetGuard(world2).Actor!.DialogueCooldowns.SaveState();
            Assert.IsTrue(self.TryGetValue(selfKey, out var selfTs),   "Self cooldown key should be restored.");
            Assert.AreEqual(timestamp, selfTs,  1e-9,                  "Self cooldown timestamp should match.");
            Assert.IsTrue(group.TryGetValue(groupKey, out var groupTs), "Group cooldown key should be restored.");
            Assert.AreEqual(timestamp, groupTs, 1e-9,                  "Group cooldown timestamp should match.");
        }

        [TestMethod]
        public void RoundTrip_NextWorldIdIsPreserved()
        {
            string json;
            int savedNextId;

            // ── Save ──────────────────────────────────────────────────────────────
            using (var scope1 = Services.CreateScope())
            {
                var sp1      = scope1.ServiceProvider;
                BuildWorld(sp1);
                var world1   = sp1.Get<WorldData>()!;
                var handles1 = sp1.Get<ObjectHandleManager>()!;
                savedNextId  = handles1.NextWorldId;
                json = sp1.Get<SaveGameService>()!.Serialize(world1, handles1);
            }

            // ── Load ──────────────────────────────────────────────────────────────
            using var scope2 = Services.CreateScope();
            var sp2 = scope2.ServiceProvider;
            BuildWorld(sp2);
            var world2   = sp2.Get<WorldData>()!;
            var handles2 = sp2.Get<ObjectHandleManager>()!;
            var static2  = sp2.Get<StaticData>()!;
            sp2.Get<LoadGameService>()!.Restore(json, world2, handles2, static2);

            Assert.AreEqual(savedNextId, handles2.NextWorldId,
                "NextWorldId counter must be restored so new objects get unique IDs.");
        }
    }
}
