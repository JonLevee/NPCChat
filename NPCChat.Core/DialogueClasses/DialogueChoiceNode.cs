using System.Collections.Generic;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// The NPC delivers a line, then the player selects from up to 9 choices.
    /// This is the primary branching node type.
    /// </summary>
    public sealed class DialogueChoiceNode : DialogueNode
    {
        /// <summary>What the NPC says before presenting the player's options.</summary>
        public string NpcText { get; init; } = string.Empty;

        /// <summary>Available choices. Shown in order; hidden when Condition returns false.
        /// Maximum 9 (keyboard shortcuts 1–9).</summary>
        public IReadOnlyList<DialogueChoice> Choices { get; init; } = new List<DialogueChoice>();
    }
}
