#nullable enable
using System.Collections.Generic;
using NPCChat.Core.BehaviorClasses;

namespace NPCChat.Core.SaveClasses
{
    // ── Root save document ───────────────────────────────────────────────────────

    /// <summary>
    /// Root of a JSON save file. Captures all mutable world state.
    /// Immutable template state (behaviors, dialogue trees, shop stock) is NOT
    /// saved — it is reconstructed from the world definition on load.
    /// </summary>
    public sealed class WorldSaveFile
    {
        public int    Version     { get; init; } = 1;
        /// <summary>ISO 8601 UTC timestamp written at save time.</summary>
        public string SavedAt     { get; init; } = "";
        /// <summary>Restore into ObjectHandleManager so new objects get unique IDs.</summary>
        public int    NextWorldId { get; init; }
        public ObjectSaveRecord[] Objects { get; init; } = [];
    }

    // ── Per-object save record ───────────────────────────────────────────────────

    /// <summary>
    /// Serialized state for one WorldObject. The <see cref="Type"/> field
    /// distinguishes object categories: "static", "carryable", "moveable", "player".
    /// Fields irrelevant to a given type are left at their default (null/0).
    /// </summary>
    public sealed class ObjectSaveRecord
    {
        /// <summary>Object category tag: "static" | "carryable" | "moveable" | "player".</summary>
        public string Type    { get; init; } = "";
        public int    WorldId { get; init; }

        /// <summary>Current bounds as [left, top, right, bottom].</summary>
        public int[]  Bounds  { get; init; } = [];

        // ── Carryable ────────────────────────────────────────────────────────────
        public string? ItemId   { get; init; }
        public int     Quantity { get; init; }

        // ── Moveable (NPC / Mob) ─────────────────────────────────────────────────
        public string?              Mode       { get; init; }
        public int?                 CurrentHp  { get; init; }
        /// <summary>
        /// Serialized ActionQueue. Tasks restart from Begin() on load.
        /// </summary>
        public ITaskToken[]?        ActionQueue { get; init; }
        public InventoryItemSave[]? Inventory  { get; init; }
        /// <summary>Dialogue cooldown state for this NPC's CooldownTracker.</summary>
        public CooldownEntrySave[]? Cooldowns  { get; init; }

        // ── Player-only ──────────────────────────────────────────────────────────
        public QuestLogSave?           QuestLog   { get; init; }
        public ReputationEntrySave[]?  Reputation { get; init; }
    }

    // ── Supporting value types ───────────────────────────────────────────────────

    /// <summary>One inventory slot: item id + quantity.</summary>
    public sealed class InventoryItemSave
    {
        public string ItemId   { get; init; } = "";
        public int    Quantity { get; init; }
    }

    /// <summary>
    /// One entry from CooldownTracker's internal key→timestamp maps.
    /// <see cref="IsGroup"/> distinguishes the self-key dict from the group-key dict.
    /// </summary>
    public sealed class CooldownEntrySave
    {
        public string Key       { get; init; } = "";
        public double LastUsed  { get; init; }
        public bool   IsGroup   { get; init; }
    }

    /// <summary>Quest journal snapshot.</summary>
    public sealed class QuestLogSave
    {
        public string[] ActiveIds    { get; init; } = [];
        public string[] CompletedIds { get; init; } = [];
    }

    /// <summary>One faction reputation score.</summary>
    public sealed class ReputationEntrySave
    {
        public string FactionId { get; init; } = "";
        public int    Score     { get; init; }
    }
}
