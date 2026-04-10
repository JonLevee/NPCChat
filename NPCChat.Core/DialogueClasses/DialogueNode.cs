#nullable enable
namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Base class for all dialogue nodes. A DialogueTree is a dictionary of these.
    ///
    /// Node types:
    ///   DialogueLineNode     — single scripted line, then advance to NextNodeId
    ///   DialoguePoolNode     — weighted-random pick from N candidates (mood-scored)
    ///   DialogueChoiceNode   — NPC speaks, player picks from up to 9 options
    ///   DialogueSequenceNode — run a list of child nodes in order
    /// </summary>
    public abstract class DialogueNode
    {
        /// <summary>Unique identifier within the owning DialogueTree.</summary>
        public required string Id { get; init; }

        /// <summary>
        /// Optional gate evaluated before entering this node.
        /// If it returns false the node is skipped (callers should advance to NextNodeId
        /// or treat the tree branch as unavailable).
        /// </summary>
        public Func<DialogueContext, bool>? Condition { get; init; }

        /// <summary>Effects applied when this node is entered.</summary>
        public IReadOnlyList<DialogueEffect> OnEnterEffects { get; init; } = [];
    }
}
