#nullable enable
using System.Collections.Generic;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Runs a fixed list of child nodes in order, then advances to NextNodeId.
    /// Useful for multi-line NPC speeches, cutscene-style exchanges, or
    /// chaining pool picks followed by a choice.
    /// </summary>
    public sealed class DialogueSequenceNode : DialogueNode
    {
        /// <summary>IDs of nodes to run in sequence. Each node's own NextNodeId is ignored;
        /// the sequence drives progression.</summary>
        public IReadOnlyList<string> NodeIds { get; init; } = new List<string>();

        /// <summary>Node to advance to after the last child completes. Null ends the conversation.</summary>
        public string? NextNodeId { get; init; }
    }
}
