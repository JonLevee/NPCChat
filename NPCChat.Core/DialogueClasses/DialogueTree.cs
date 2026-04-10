#nullable enable
namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// A complete dialogue graph owned by one NPC (via ActorComponent).
    /// Nodes are keyed by their Id. Use DialogueTreeBuilder to construct.
    /// </summary>
    public sealed class DialogueTree
    {
        public string Id { get; init; } = string.Empty;

        /// <summary>Id of the node entered when a conversation begins at the tree root.</summary>
        public string RootNodeId { get; init; } = string.Empty;

        /// <summary>
        /// Ordered mood axis names shared by all DialoguePoolNodes in this tree.
        /// Index i here maps to index i in DialoguePoolEntry.MoodVector and Character.MoodVector.
        /// </summary>
        public string[] MoodAxes { get; init; } = [];

        public IReadOnlyDictionary<string, DialogueNode> Nodes { get; init; } =
            new Dictionary<string, DialogueNode>();

        /// <summary>Returns the node with the given id, or null if not found.</summary>
        public DialogueNode? GetNode(string id) =>
            Nodes.TryGetValue(id, out var node) ? node : null;
    }
}
