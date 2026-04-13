#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using NPCChat.Core.Attributes;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.LoadingProviderClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.SaveClasses
{
    /// <summary>
    /// Overlays saved runtime state onto a freshly-built WorldData.
    /// Load flow:
    ///   1. Build the world from its definition (templates or YAML) — objects get WorldIds.
    ///   2. Call <see cref="LoadFromFile"/> (or <see cref="Restore"/>).
    ///   3. The service finds each saved object by WorldId and patches its mutable state.
    /// </summary>
    [Scoped]
    public sealed class LoadGameService
    {
        private readonly IMoveableActionsFactory _taskFactory;

        public LoadGameService(IMoveableActionsFactory taskFactory)
        {
            _taskFactory = taskFactory;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        public void LoadFromFile(
            string filePath,
            WorldData world,
            ObjectHandleManager handles,
            StaticData staticData)
        {
            var json = File.ReadAllText(filePath);
            Restore(json, world, handles, staticData);
        }

        /// <summary>Restores state from a JSON string (useful for testing).</summary>
        public void Restore(
            string json,
            WorldData world,
            ObjectHandleManager handles,
            StaticData staticData)
        {
            var file = JsonSerializer.Deserialize<WorldSaveFile>(json, SaveGameService.JsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize save file.");

            handles.RestoreNextWorldId(file.NextWorldId);

            // Build a WorldId → WorldObject lookup from the live world.
            var byId = world.EnumerateWorldObjects()
                            .ToDictionary(o => o.WorldId);

            foreach (var rec in file.Objects)
            {
                if (!byId.TryGetValue(rec.WorldId, out var obj))
                    continue;   // object no longer exists (e.g. template removed)

                ApplyRecord(rec, obj, world, staticData);
            }
        }

        // ── Per-object restore ───────────────────────────────────────────────────

        private void ApplyRecord(
            ObjectSaveRecord rec, WorldObject obj, WorldData world, StaticData staticData)
        {
            switch (rec.Type)
            {
                case "static":
                    // Static objects never move; no position update needed.
                    break;

                case "carryable":
                    // Template carryables don't move after placement; only quantity changes.
                    if (obj is WorldObjectCarryable carryable)
                        carryable.Quantity = rec.Quantity;
                    break;

                case "moveable":
                    if (obj is WorldObjectMoveable moveable)
                    {
                        RestoreMoveableBounds(rec, moveable, world);
                        ApplyMoveable(rec, moveable, staticData);
                    }
                    break;

                case "player":
                    if (obj is WorldObjectMoveable player)
                    {
                        RestoreMoveableBounds(rec, player, world);
                        ApplyPlayer(rec, player, staticData);
                    }
                    break;
            }
        }

        private static void RestoreMoveableBounds(
            ObjectSaveRecord rec, WorldObjectMoveable m, WorldData world)
        {
            if (rec.Bounds.Length != 4) return;
            var savedBounds = new Bounds(rec.Bounds[0], rec.Bounds[1], rec.Bounds[2], rec.Bounds[3]);
            if (m.Bounds != savedBounds)
                world.MoveDynamicObjectBetweenChunks(m.Handle, savedBounds);
        }

        private void ApplyMoveable(
            ObjectSaveRecord rec, WorldObjectMoveable m, StaticData staticData)
        {
            if (rec.Mode is not null && m.Actor is not null)
                m.Actor.Mode = rec.Mode;

            if (rec.CurrentHp.HasValue && m.Combat is not null)
                m.Combat.RestoreCurrentHp(rec.CurrentHp.Value);

            if (rec.ActionQueue is { Length: > 0 } && m.Actor is not null)
                RestoreActionQueue(rec.ActionQueue, m.Actor.ActionQueue);

            if (rec.Inventory is not null && m.Inventory is not null)
                RestoreInventory(rec.Inventory, m.Inventory, staticData);

            if (rec.Cooldowns is not null && m.Actor is not null)
                RestoreCooldowns(rec.Cooldowns, m.Actor.DialogueCooldowns);
        }

        private void ApplyPlayer(
            ObjectSaveRecord rec, WorldObjectMoveable p, StaticData staticData)
        {
            if (rec.CurrentHp.HasValue && p.Combat is not null)
                p.Combat.RestoreCurrentHp(rec.CurrentHp.Value);

            if (rec.Inventory is not null && p.Inventory is not null)
                RestoreInventory(rec.Inventory, p.Inventory, staticData);

            if (rec.QuestLog is not null && p.QuestLog is not null)
                p.QuestLog.RestoreState(
                    rec.QuestLog.ActiveIds,
                    rec.QuestLog.CompletedIds,
                    id => staticData.GetQuest(id));

            if (rec.Reputation is not null && p.ReputationLog is not null)
                p.ReputationLog.RestoreState(
                    rec.Reputation.Select(r =>
                        new System.Collections.Generic.KeyValuePair<string, int>(r.FactionId, r.Score)));
        }

        // ── Field restorers ──────────────────────────────────────────────────────

        private void RestoreActionQueue(ITaskToken[] tokens, ActionQueue queue)
        {
            queue.Clear();
            foreach (var token in tokens)
            {
                var task = _taskFactory.Create(token);
                queue.Enqueue(task);
                // Note: Begin() is NOT called here — the simulation loop calls Begin()
                // on the first tick the task is at the front of the queue.
            }
        }

        private static void RestoreInventory(
            InventoryItemSave[] items,
            NPCChat.Core.InventoryClasses.InventoryComponent inv,
            StaticData staticData)
        {
            // Clear existing inventory by removing all slots via TryRemove,
            // then re-add saved items.
            foreach (var slot in inv.Slots.ToList())
                inv.TryRemove(slot.Item.Id, slot.Quantity, out _);

            foreach (var item in items)
            {
                var def = staticData.GetItem(item.ItemId);
                if (def is null) continue;   // item removed from game data — skip
                inv.TryAdd(def, item.Quantity, out _);
            }
        }

        private static void RestoreCooldowns(
            CooldownEntrySave[] entries,
            NPCChat.Core.DialogueClasses.CooldownTracker tracker)
        {
            var self  = entries.Where(e => !e.IsGroup)
                               .Select(e => new System.Collections.Generic.KeyValuePair<string, double>(e.Key, e.LastUsed));
            var group = entries.Where(e =>  e.IsGroup)
                               .Select(e => new System.Collections.Generic.KeyValuePair<string, double>(e.Key, e.LastUsed));
            tracker.RestoreState(self, group);
        }
    }
}
