#nullable enable
using System;
using System.Collections.Generic;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// One player-selectable option within a DialogueChoiceNode.
    /// Up to 9 choices per node (keyboard shortcut 1–9).
    /// </summary>
    public sealed class DialogueChoice
    {
        /// <summary>Short label displayed to the player (keep under ~60 chars).</summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>If set and returns false, this choice is hidden from the player.</summary>
        public Func<DialogueContext, bool>? Condition { get; init; }

        /// <summary>Node to advance to when this choice is selected. Null ends the conversation.</summary>
        public string? NextNodeId { get; init; }

        /// <summary>Side effects applied the moment this choice is selected.</summary>
        public IReadOnlyList<DialogueEffect> OnSelectEffects { get; init; } = new List<DialogueEffect>();
    }
}
