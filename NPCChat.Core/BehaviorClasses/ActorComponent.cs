#nullable enable
using System;
using System.Collections.Generic;
using NPCChat.Core.DialogueClasses;
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
        public List<BehaviorRule> ReactiveRules { get; } = new List<BehaviorRule>();

        /// <summary>Priority-ordered queue of tasks to execute.</summary>
        public ActionQueue ActionQueue { get; } = new();

        /// <summary>
        /// How far (in grid units) this actor can detect other world objects.
        /// Measured as Chebyshev distance (matches 8-directional grid movement).
        /// </summary>
        public float PerceptionRange { get; set; } = 8f;

        /// <summary>
        /// For actors far from the player, process every N ticks instead of every tick.
        /// Default 10 means distant actors run at 1/10th the normal rate.
        /// </summary>
        public int DistantProcessInterval { get; set; } = 10;

        /// <summary>Ticks accumulated since this actor was last processed (used for distant throttling).</summary>
        public int TicksSinceLastProcess { get; set; }

        // ── Dialogue ──────────────────────────────────────────────────────────

        /// <summary>
        /// The NPC's dialogue graph. Null for actors that have no conversation (e.g. simple mobs).
        /// Owned by the sim thread; read by the UI thread only during an active DialogueSession.
        /// </summary>
        public DialogueTree? DialogueTree { get; set; }

        /// <summary>
        /// Named entry points into DialogueTree exposed to nearby players.
        /// Evaluated by the sim thread each tick; results published via InteractionSnapshot.
        /// </summary>
        public List<InteractionEntry> Interactions { get; } = new List<InteractionEntry>();

        /// <summary>
        /// Per-actor cooldown state for DialoguePoolNode picks.
        /// Owned by the sim/UI thread depending on who last picked a line.
        /// </summary>
        public CooldownTracker DialogueCooldowns { get; } = new();

        // ── Spatial helpers ───────────────────────────────────────────────────

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
