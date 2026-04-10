#nullable enable
using System.Drawing;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// The actor picks random nearby waypoints and walks to them in sequence.
    /// Used for NPCs that roam an area (e.g., Farmer on break).
    /// Loops indefinitely until interrupted or the task is removed from the queue.
    /// </summary>
    public sealed class WanderTask : ActorTaskBase
    {
        private readonly int _wanderRadius;
        private readonly Random _rng;
        private int _ticksUntilNextWaypoint;

        /// <param name="wanderRadius">Maximum grid-unit distance from the actor's starting position to pick waypoints.</param>
        /// <param name="priority">Task priority in the ActionQueue.</param>
        /// <param name="rng">Optional RNG instance for deterministic tests.</param>
        public WanderTask(int wanderRadius, int priority = 0, Random? rng = null) : base(priority)
        {
            _wanderRadius = wanderRadius;
            _rng = rng ?? Random.Shared;
        }

        public override void Begin(in SimContext ctx)
        {
            _ticksUntilNextWaypoint = 0;
        }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            // Only issue a new move command when the actor has finished moving.
            if (ctx.Actor.Movement.IsMoving) return false;

            // Wait a short random pause between waypoints.
            if (_ticksUntilNextWaypoint > 0)
            {
                _ticksUntilNextWaypoint--;
                return false;
            }

            var origin = ctx.Actor.Bounds;
            int centerX = (origin.Left + origin.Right) / 2;
            int centerY = (origin.Top + origin.Bottom) / 2;

            int targetX = centerX + _rng.Next(-_wanderRadius, _wanderRadius + 1);
            int targetY = centerY + _rng.Next(-_wanderRadius, _wanderRadius + 1);

            ctx.EnqueueMove(new MoveCommand(ctx.Actor.Handle, new Point(targetX, targetY)));

            // Pause 1–5 ticks at the destination before picking the next waypoint.
            _ticksUntilNextWaypoint = _rng.Next(1, 6);

            return false; // Never self-completes; removed by schedule or interrupt.
        }

        public override void Interrupt(in SimContext ctx)
        {
            ctx.Actor.Movement.ClearMovement();
        }
    }
}
