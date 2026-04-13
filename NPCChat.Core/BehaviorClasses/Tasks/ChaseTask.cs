using System;
using System.Drawing;
using NPCChat.Core.AIClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// Pursues the player, re-pathing every few ticks as the player moves.
    /// Completes when the player is no longer within the actor's perception range.
    /// While active, posts a PlayerDetected alert each tick so nearby allies can react.
    /// </summary>
    public sealed class ChaseTask : ActorTaskBase
    {
        private int _repathCooldown;

        public ChaseTask(int priority = 40) : base(priority) { }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (ctx.PlayerBounds is not { } playerBounds) return true;

            // Stop chasing if player left perception range.
            if (!ctx.Actor.Actor!.CanPerceive(ctx.Actor.Bounds, playerBounds)) return true;

            // Broadcast alert so allies can investigate.
            var center = Center(playerBounds);
            ctx.PostAlert?.Invoke(new AlertEvent
            {
                Position = center,
                Kind     = AlertKind.PlayerDetected,
                Tick     = ctx.GameTick
            });

            // Re-path periodically as the player moves.
            if (--_repathCooldown <= 0)
            {
                _repathCooldown = 5;
                ctx.EnqueueMove(new MoveCommand(ctx.Actor.Handle, center));
            }

            return false;
        }

        public override ITaskToken ToToken() => new ChaseTaskToken(Priority);

        public override void Interrupt(in SimContext ctx)
            => ctx.Actor.Movement.ClearMovement();

        private static Point Center(Bounds b) =>
            new(b.Left + b.Width / 2, b.Top + b.Height / 2);
    }
}
