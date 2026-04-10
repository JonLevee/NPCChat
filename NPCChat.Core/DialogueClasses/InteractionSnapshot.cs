#nullable enable
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// One entry in the interaction menu shown near an actor.
    /// </summary>
    public readonly struct InteractionOption
    {
        /// <summary>Short label shown to the player.</summary>
        public string Label  { get; init; }

        /// <summary>Entry node in the actor's DialogueTree.</summary>
        public string NodeId { get; init; }

        /// <summary>1-based keyboard index (1–9).</summary>
        public int Index { get; init; }
    }

    /// <summary>
    /// Published by the sim thread each tick for actors within the player's
    /// perception range that have at least one visible interaction.
    /// Consumed by the UI thread to render interaction overlays on the canvas.
    /// </summary>
    public sealed class InteractionSnapshot
    {
        public ObjectHandle ActorHandle { get; init; }
        public Bounds       ActorBounds { get; init; }

        /// <summary>Display name for the actor (e.g. "Blacksmith"). May be null.</summary>
        public string? ActorName { get; init; }

        /// <summary>Visible interaction options, ordered by priority then label. Max 9.</summary>
        public InteractionOption[] Options { get; init; } = [];
    }
}
