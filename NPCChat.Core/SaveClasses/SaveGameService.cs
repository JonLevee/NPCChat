#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NPCChat.Core.Attributes;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.SaveClasses
{
    /// <summary>
    /// Serializes the current WorldData snapshot to a JSON save file.
    /// Call from the UI thread after pausing the simulation, or during a controlled
    /// save point where world state is stable.
    /// </summary>
    [Scoped]
    public sealed class SaveGameService
    {
        internal static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented             = true,
            PropertyNamingPolicy      = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition    = JsonIgnoreCondition.WhenWritingNull,
        };

        // ── Public API ───────────────────────────────────────────────────────────

        public void SaveToFile(WorldData world, ObjectHandleManager handles, string filePath)
        {
            var json = Serialize(world, handles);
            File.WriteAllText(filePath, json);
        }

        /// <summary>Returns the save file as a JSON string (useful for testing).</summary>
        public string Serialize(WorldData world, ObjectHandleManager handles)
        {
            var objects = world.EnumerateWorldObjects()
                               .Select(BuildRecord)
                               .ToArray();

            var file = new WorldSaveFile
            {
                SavedAt     = DateTime.UtcNow.ToString("o"),
                NextWorldId = handles.NextWorldId,
                Objects     = objects
            };

            return JsonSerializer.Serialize(file, JsonOptions);
        }

        // ── Record builders ──────────────────────────────────────────────────────

        private static ObjectSaveRecord BuildRecord(WorldObject obj) => obj switch
        {
            WorldObjectStatic   s => BuildStatic(s),
            WorldObjectCarryable c => BuildCarryable(c),
            WorldObjectMoveable  m when m.Kind == WorldObjectKind.Player => BuildPlayer(m),
            WorldObjectMoveable  m => BuildMoveable(m),
            _ => throw new InvalidOperationException($"Unknown WorldObject subtype: {obj.GetType().Name}")
        };

        private static ObjectSaveRecord BuildStatic(WorldObjectStatic obj) => new()
        {
            Type    = "static",
            WorldId = obj.WorldId,
            Bounds  = BoundsToArray(obj.Bounds),
        };

        private static ObjectSaveRecord BuildCarryable(WorldObjectCarryable obj) => new()
        {
            Type     = "carryable",
            WorldId  = obj.WorldId,
            Bounds   = BoundsToArray(obj.Bounds),
            ItemId   = obj.ItemDef.Id,
            Quantity = obj.Quantity,
        };

        private static ObjectSaveRecord BuildMoveable(WorldObjectMoveable obj) => new()
        {
            Type        = "moveable",
            WorldId     = obj.WorldId,
            Bounds      = BoundsToArray(obj.Bounds),
            Mode        = obj.Actor?.Mode,
            CurrentHp   = obj.Combat?.CurrentHp,
            ActionQueue = BuildActionQueue(obj.Actor),
            Inventory   = BuildInventory(obj.Inventory),
            Cooldowns   = BuildCooldowns(obj.Actor?.DialogueCooldowns),
        };

        private static ObjectSaveRecord BuildPlayer(WorldObjectMoveable obj) => new()
        {
            Type        = "player",
            WorldId     = obj.WorldId,
            Bounds      = BoundsToArray(obj.Bounds),
            CurrentHp   = obj.Combat?.CurrentHp,
            Inventory   = BuildInventory(obj.Inventory),
            QuestLog    = BuildQuestLog(obj.QuestLog),
            Reputation  = BuildReputation(obj.ReputationLog),
        };

        // ── Field serializers ────────────────────────────────────────────────────

        private static int[] BoundsToArray(Bounds b)
            => [b.Left, b.Top, b.Right, b.Bottom];

        private static ITaskToken[]? BuildActionQueue(ActorComponent? actor)
        {
            if (actor is null || actor.ActionQueue.IsEmpty) return null;
            var tokens = actor.ActionQueue.AllTasks.Select(t => t.ToToken()).ToArray();
            return tokens.Length > 0 ? tokens : null;
        }

        private static InventoryItemSave[]? BuildInventory(
            NPCChat.Core.InventoryClasses.InventoryComponent? inv)
        {
            if (inv is null || inv.Slots.Count == 0) return null;
            return inv.Slots
                .Select(s => new InventoryItemSave { ItemId = s.Item.Id, Quantity = s.Quantity })
                .ToArray();
        }

        private static CooldownEntrySave[]? BuildCooldowns(CooldownTracker? tracker)
        {
            if (tracker is null) return null;
            var (self, group) = tracker.SaveState();
            if (self.Count == 0 && group.Count == 0) return null;

            var entries = new List<CooldownEntrySave>(self.Count + group.Count);
            foreach (var kv in self)
                entries.Add(new CooldownEntrySave { Key = kv.Key, LastUsed = kv.Value, IsGroup = false });
            foreach (var kv in group)
                entries.Add(new CooldownEntrySave { Key = kv.Key, LastUsed = kv.Value, IsGroup = true });
            return entries.ToArray();
        }

        private static QuestLogSave? BuildQuestLog(NPCChat.Core.QuestClasses.QuestLog? log)
        {
            if (log is null) return null;
            var (active, completed) = log.SaveState();
            return new QuestLogSave
            {
                ActiveIds    = active.ToArray(),
                CompletedIds = completed.ToArray(),
            };
        }

        private static ReputationEntrySave[]? BuildReputation(
            NPCChat.Core.FactionClasses.ReputationLog? rep)
        {
            if (rep is null) return null;
            var scores = rep.AllScores;
            if (scores.Count == 0) return null;
            return scores
                .Select(kv => new ReputationEntrySave { FactionId = kv.Key, Score = kv.Value })
                .ToArray();
        }
    }
}
