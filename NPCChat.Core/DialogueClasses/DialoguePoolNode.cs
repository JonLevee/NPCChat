#nullable enable
using System;
using System.Collections.Generic;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Selects one line at runtime from a weighted pool of candidates, scored
    /// against the NPC's current mood vector (dot-product affinity), hard stat gates,
    /// and cooldowns. Replaces the old StockResponses system.
    ///
    /// Typical uses: ambient greetings, flavour remarks, idle chatter.
    /// </summary>
    public sealed class DialoguePoolNode : DialogueNode
    {
        public IReadOnlyList<DialoguePoolEntry> Entries { get; init; } = new List<DialoguePoolEntry>();

        /// <summary>Default next node after the picked line is displayed. Individual
        /// entries may override this via DialoguePoolEntry.NextNodeId.</summary>
        public string? NextNodeId { get; init; }
    }

    /// <summary>
    /// One candidate line inside a DialoguePoolNode.
    /// </summary>
    public sealed class DialoguePoolEntry
    {
        /// <summary>
        /// Stable identifier used by CooldownTracker.
        /// Set automatically by DialogueTreeBuilder; must be unique within the pool.
        /// </summary>
        public string Id { get; init; } = string.Empty;

        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// Optional intent tags. If the caller requests a specific intent, only
        /// entries that carry that tag (or have no tags) are eligible.
        /// </summary>
        public string[] Intent { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Normalized unit vector in the owner tree's MoodAxes space.
        /// Built by DialogueTreeBuilder.PoolBuilder from raw mood hints.
        /// Empty array → mood-neutral (scores 0.5 affinity against any mood).
        /// </summary>
        public float[] MoodVector { get; init; } = Array.Empty<float>();

        /// <summary>
        /// Stat gates: all must pass for the entry to be eligible.
        /// Keys are stat names (case-insensitive); values are MinMax ranges in 0..1.
        /// </summary>
        public Dictionary<string, MinMax> Requires { get; init; } = new Dictionary<string, MinMax>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Stat forbids: if any passes, the entry is excluded.
        /// </summary>
        public Dictionary<string, MinMax> Forbids   { get; init; } = new Dictionary<string, MinMax>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Minimum required dot-product affinity after clamping to [0..1].</summary>
        public float MinAffinity { get; init; }

        /// <summary>Relative weight applied to the final score. Default 1.</summary>
        public float Weight { get; init; } = 1f;

        public CooldownSpec Cooldown { get; init; } = new CooldownSpec();

        /// <summary>Overrides DialoguePoolNode.NextNodeId for this specific entry when set.</summary>
        public string? NextNodeId { get; init; }
    }
}
