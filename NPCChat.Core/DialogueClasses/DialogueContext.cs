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

        /// <summary>
        /// Returns the quantity of an item in the player's inventory.
        /// Thread-safe: implemented via WorldData.SnapshotInventoryCount under a read lock.
        /// Null when no player exists in the world.
        /// </summary>
        public Func<string, int>? GetPlayerItemCount { get; init; }

        /// <summary>
        /// Returns the player's current reputation score with the given faction ID.
        /// UI-thread-safe: reads directly from the player's ReputationLog (UI-thread-owned).
        /// Null when no player exists in the world.
        /// </summary>
        public Func<string, int>? GetPlayerReputation { get; init; }
    }
}
