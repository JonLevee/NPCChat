using NPCChat.Core.WorldClasses;
using System.Drawing;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// Lightweight context passed to all behavior delegates each tick.
    /// Contains read-only world state relevant to the actor being processed.
    /// </summary>
    public readonly struct SimContext
    {
        /// <summary>The actor being processed this tick.</summary>
        public WorldObjectMoveable Actor { get; }

        /// <summary>
        /// The player's bounds, or null if no player exists in the world.
        /// </summary>
        public Bounds? PlayerBounds { get; }

        /// <summary>Absolute simulation tick count since the world started.</summary>
        public int GameTick { get; }

        /// <summary>Current hour of the game day (0–23).</summary>
        public int GameHour { get; }

        /// <summary>
        /// Issues a move command for the actor. The command takes effect on
        /// the next sim tick (channel-based, non-blocking).
        /// </summary>
        public Action<MoveCommand> EnqueueMove { get; }

        public SimContext(
            WorldObjectMoveable actor,
            Bounds? playerBounds,
            int gameTick,
            int gameHour,
            Action<MoveCommand> enqueueMove)
        {
            Actor = actor;
            PlayerBounds = playerBounds;
            GameTick = gameTick;
            GameHour = gameHour;
            EnqueueMove = enqueueMove;
        }
    }
}
