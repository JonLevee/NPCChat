#nullable enable
namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// A single scripted line spoken by one party, followed by an optional next node.
    /// Suitable for NPC monologues, short player interjections, and flavor text.
    /// </summary>
    public sealed class DialogueLineNode : DialogueNode
    {
        /// <summary>"npc" or "player".</summary>
        public string Speaker { get; init; } = "npc";

        public string Text { get; init; } = string.Empty;

        /// <summary>Node to advance to after this line is displayed. Null ends the conversation.</summary>
        public string? NextNodeId { get; init; }
    }
}
