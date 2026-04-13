using System;
using System.Drawing;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// Runs directly away from the player, re-pathing every few ticks.
    /// Completes once the player is no longer within perception range.
    /// </summary>
    public sealed class FleeTask : ActorTaskBase
    {
        private int _repathCooldown;

        public FleeTask(int priority = 60) : base(priority) { }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (ctx.PlayerBounds is not { } playerBounds) return true;

            // Safe — player left detection range.
            if (!ctx.Actor.Actor!.CanPerceive(ctx.Actor.Bounds, playerBounds)) return true;

            if (--_repathCooldown <= 0)
            {
                _repathCooldown = 8;

                // Flee target: actor's position reflected away from player center.
                var actorX = ctx.Actor.Bounds.Left + ctx.Actor.Bounds.Width  / 2;
                var actorY = ctx.Actor.Bounds.Top  + ctx.Actor.Bounds.Height / 2;
                var playerX = playerBounds.Left + playerBounds.Width  / 2;
                var playerY = playerBounds.Top  + playerBounds.Height / 2;

                // Flee by twice the distance between actor and player.
                var fleeTarget = new Point(
                    Math.Clamp(actorX * 2 - playerX, 0, 200),
                    Math.Clamp(actorY * 2 - playerY, 0, 200));

                ctx.EnqueueMove(new MoveCommand(ctx.Actor.Handle, fleeTarget));
            }

            return false;
        }

        public override ITaskToken ToToken() => new FleeTaskToken(Priority);

        public override void Interrupt(in SimContext ctx)
            => ctx.Actor.Movement.ClearMovement();
    }
}
