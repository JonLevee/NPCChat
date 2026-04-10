#nullable enable
namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// One interaction verb an actor exposes to nearby players (e.g. "Talk", "Shop", "Quest").
    /// Each entry is a named entry point into the actor's DialogueTree.
    ///
    /// The sim thread evaluates Condition each tick for actors within perception range
    /// and publishes the visible entries as part of InteractionSnapshot.
    /// </summary>
    public sealed class InteractionEntry
    {
        /// <summary>Short label shown to the player (e.g. "Talk", "Shop", "Quest").</summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>
        /// Node in the actor's DialogueTree to enter when this verb is selected.
        /// Must match a key in DialogueTree.Nodes.
        /// </summary>
        public string NodeId { get; init; } = string.Empty;

        /// <summary>
        /// If set and returns false, this entry is hidden from the player.
        /// Evaluated on the sim thread against a DialogueContext.
        /// </summary>
        public Func<DialogueContext, bool>? Condition { get; init; }

        /// <summary>Display order. Higher priority entries appear first in the list.</summary>
        public int Priority { get; init; }
    }
}
