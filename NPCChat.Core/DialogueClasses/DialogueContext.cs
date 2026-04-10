#nullable enable
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Lightweight context passed to all dialogue conditions and effects.
    /// Constructed both by the sim thread (interaction exposure) and the UI thread
    /// (tree navigation and choice evaluation).
    /// </summary>
    public readonly struct DialogueContext
    {
        /// <summary>The NPC actor involved in the interaction.</summary>
        public WorldObjectMoveable Actor { get; init; }

        /// <summary>The player, or null if no player exists in the world.</summary>
        public WorldObjectMoveable? Player { get; init; }

        /// <summary>Current hour of the game day (0–23).</summary>
        public int GameHour { get; init; }

        /// <summary>Absolute simulation tick count since the world started.</summary>
        public int GameTick { get; init; }
    }
}
