using System.Drawing;

namespace NPCChatLib.WorldClasses
{
    /// <summary>
    /// Mutable movement state for a WorldObjectMoveable.
    /// Owned exclusively by the background simulation thread once movement begins.
    /// The UI thread reads a copy via PositionPatch (Phase 1).
    /// </summary>
    public sealed class MovementState
    {
        /// <summary>
        /// Remaining waypoints to reach FinalTarget, in grid coordinates.
        /// Dequeued one tile per simulation step.
        /// </summary>
        public Queue<Point> Path { get; } = new();

        /// <summary>
        /// The ultimate destination the object is navigating to.
        /// Null when the object is not moving.
        /// Retained after pathing completes to detect "already at target" re-clicks.
        /// </summary>
        public Point? FinalTarget { get; set; }

        /// <summary>
        /// Current facing direction, updated each time a step is taken.
        /// </summary>
        public Direction8 Facing { get; set; } = Direction8.S;

        public bool IsMoving => Path.Count > 0;

        public void ClearMovement()
        {
            Path.Clear();
            FinalTarget = null;
        }
    }
}
