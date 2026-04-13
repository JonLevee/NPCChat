using System.Drawing;
using NPCChat.Core.AIClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// Holds position while a hostile target is within close range, broadcasting
    /// alerts each tick so allies can respond.
    /// Completes after the player has been out of range for <c>timeoutTicks</c> ticks.
    /// Phase 11 (Combat) will extend this to actually attack.
    /// </summary>
    public sealed class CombatReadyTask : ActorTaskBase
    {
        private readonly int _closeRange;
        private readonly int _timeoutTicks;
        private int _timeoutRemaining;

        public CombatReadyTask(int closeRange = 4, int timeoutTicks = 40, int priority = 50)
            : base(priority)
        {
            _closeRange      = closeRange;
            _timeoutTicks    = timeoutTicks;
            _timeoutRemaining = timeoutTicks;
        }

        public override void Begin(in SimContext ctx)
        {
            base.Begin(ctx);
            // Stop any pending movement — we're holding position.
            ctx.Actor.Movement.ClearMovement();
            _timeoutRemaining = _timeoutTicks;
        }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (ctx.PlayerBounds is not { } playerBounds)
                return true;

            bool playerClose = IsWithin(ctx.Actor.Bounds, playerBounds, _closeRange);

            if (playerClose)
            {
                _timeoutRemaining = _timeoutTicks;

                // Broadcast alert so allies can converge.
                ctx.PostAlert?.Invoke(new AlertEvent
                {
                    Position = new Point(
                        playerBounds.Left + playerBounds.Width  / 2,
                        playerBounds.Top  + playerBounds.Height / 2),
                    Kind = AlertKind.PlayerDetected,
                    Tick = ctx.GameTick
                });
            }
            else
            {
                if (--_timeoutRemaining <= 0)
                    return true;   // player backed off — resume patrol
            }

            return false;
        }

        public override ITaskToken ToToken() => new CombatReadyTaskToken(Priority, _closeRange, _timeoutTicks);

        private static bool IsWithin(Bounds a, Bounds b, float range)
        {
            float dx = (a.Left + a.Width  * 0.5f) - (b.Left + b.Width  * 0.5f);
            float dy = (a.Top  + a.Height * 0.5f) - (b.Top  + b.Height * 0.5f);
            return System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy)) <= range;
        }
    }
}
