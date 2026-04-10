using System.Collections.Generic;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// Behavior component attached to a WorldObjectMoveable to make it an Actor.
    /// Owned exclusively by the simulation thread (like MovementState).
    /// The UI thread may read Mode via the volatile field for debug display.
    /// </summary>
    public sealed class ActorComponent
    {
        /// <summary>
        /// Current behavior mode (e.g., "Work", "Idle", "Wander", "Sleep").
        /// Written by the sim thread, may be read by the UI thread — declared volatile.
        /// </summary>
        public volatile string Mode = string.Empty;

        /// <summary>Schedule that drives time-based mode transitions.</summary>
        public ActorSchedule Schedule { get; } = new();

        /// <summary>Reactive rules evaluated each tick in priority order.</summary>
        public List<BehaviorRule> ReactiveRules { get; } = [];

        /// <summary>Priority-ordered queue of tasks to execute.</summary>
        public ActionQueue ActionQueue { get; } = new();

        /// <summary>
        /// How far (in grid units) this actor can detect other world objects.
        /// Line-of-sight is deferred to Phase 5; this is a range-only check.
        /// </summary>
        public float PerceptionRange { get; set; } = 8f;

        /// <summary>
        /// For actors far from the player, process every N ticks instead of every tick.
        /// Default 10 means distant actors run at 1/10th the normal rate.
        /// </summary>
        public int DistantProcessInterval { get; set; } = 10;

        /// <summary>Ticks accumulated since this actor was last processed (used for distant throttling).</summary>
        public int TicksSinceLastProcess { get; set; }

        /// <summary>
        /// Returns true if the given bounds are within this actor's perception range.
        /// Measured as Chebyshev distance (matches 8-directional grid movement).
        /// </summary>
        public bool CanPerceive(Bounds actorBounds, Bounds targetBounds)
        {
            int dx = Math.Abs(
                (actorBounds.Left + actorBounds.Right) / 2 -
                (targetBounds.Left + targetBounds.Right) / 2);
            int dy = Math.Abs(
                (actorBounds.Top + actorBounds.Bottom) / 2 -
                (targetBounds.Top + targetBounds.Bottom) / 2);
            return Math.Max(dx, dy) <= (int)PerceptionRange;
        }
    }
}
