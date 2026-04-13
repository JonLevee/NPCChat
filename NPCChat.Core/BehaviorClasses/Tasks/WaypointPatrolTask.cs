using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// The actor visits a fixed list of waypoints in order, then loops back to the first.
    /// Used for NPCs with predictable work routes (e.g., Blacksmith moving between forge stations).
    /// Loops indefinitely until interrupted or removed from the queue.
    /// </summary>
    public sealed class WaypointPatrolTask : ActorTaskBase
    {
        private readonly IReadOnlyList<Point> _waypoints;
        private int _waypointIndex;
        private bool _waitingForArrival;

        /// <param name="waypoints">Ordered list of world-grid tile coordinates to visit.</param>
        /// <param name="priority">Task priority in the ActionQueue.</param>
        public WaypointPatrolTask(IReadOnlyList<Point> waypoints, int priority = 0) : base(priority)
        {
            if (waypoints.Count == 0)
                throw new ArgumentException("WaypointPatrolTask requires at least one waypoint.", nameof(waypoints));
            _waypoints = waypoints;
        }

        public override void Begin(in SimContext ctx)
        {
            _waypointIndex = 0;
            _waitingForArrival = false;
            IssueNextMove(ctx);
        }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (_waitingForArrival)
            {
                if (ctx.Actor.Movement.IsMoving) return false;

                // Arrived — advance to the next waypoint.
                _waypointIndex = (_waypointIndex + 1) % _waypoints.Count;
                _waitingForArrival = false;
                IssueNextMove(ctx);
            }

            return false; // Never self-completes; removed by schedule or interrupt.
        }

        public override void Interrupt(in SimContext ctx)
        {
            ctx.Actor.Movement.ClearMovement();
            _waitingForArrival = false;
        }

        private void IssueNextMove(in SimContext ctx)
        {
            ctx.EnqueueMove(new MoveCommand(ctx.Actor.Handle, _waypoints[_waypointIndex]));
            _waitingForArrival = true;
        }

        public override ITaskToken ToToken()
        {
            var pts = new Point[_waypoints.Count];
            for (int i = 0; i < _waypoints.Count; i++) pts[i] = _waypoints[i];
            return new WaypointPatrolTaskToken(Priority, pts);
        }
    }
}
