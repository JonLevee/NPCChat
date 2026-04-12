#nullable enable
using System;
using System.Drawing;
using NPCChat.Core.AIClasses;
using NPCChat.Core.WorldClasses;

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

        /// <summary>
        /// Returns true if there is an unobstructed line of sight between two bounds.
        /// Null when not available (e.g. during tests). Thread-safe: acquires read lock internally.
        /// </summary>
        public Func<Bounds, Bounds, bool>? CheckLineOfSight { get; }

        /// <summary>
        /// Posts an alert event so that nearby actors can react on the next tick.
        /// Sim-thread-only. Null when not available.
        /// </summary>
        public Action<AlertEvent>? PostAlert { get; }

        /// <summary>
        /// Returns alerts from the previous tick within a given Chebyshev radius of a center tile.
        /// Sim-thread-only. Null when not available.
        /// </summary>
        public Func<Point, int, AlertEvent[]>? GetNearbyAlerts { get; }

        /// <summary>
        /// Handle of the current player object, or null when no player exists.
        /// Used by combat tasks to look up the player's health component.
        /// </summary>
        public ObjectHandle? PlayerHandle { get; }

        /// <summary>
        /// Resolves a handle to its <see cref="WorldObjectMoveable"/>, or null if not found.
        /// Sim-thread-only. Null when not available (e.g. during tests that don't need it).
        /// </summary>
        public Func<ObjectHandle, WorldObjectMoveable?>? GetMoveable { get; }

        public SimContext(
            WorldObjectMoveable actor,
            Bounds? playerBounds,
            int gameTick,
            int gameHour,
            Action<MoveCommand> enqueueMove,
            Func<Bounds, Bounds, bool>? checkLineOfSight = null,
            Action<AlertEvent>? postAlert = null,
            Func<Point, int, AlertEvent[]>? getNearbyAlerts = null,
            ObjectHandle? playerHandle = null,
            Func<ObjectHandle, WorldObjectMoveable?>? getMoveable = null)
        {
            Actor            = actor;
            PlayerBounds     = playerBounds;
            GameTick         = gameTick;
            GameHour         = gameHour;
            EnqueueMove      = enqueueMove;
            CheckLineOfSight = checkLineOfSight;
            PostAlert        = postAlert;
            GetNearbyAlerts  = getNearbyAlerts;
            PlayerHandle     = playerHandle;
            GetMoveable      = getMoveable;
        }
    }
}
