using System.Drawing;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// Moves the actor to an alert position, then idles briefly before completing.
    /// Used for group alert propagation: a guard hears about a detection and
    /// investigates the location.
    /// </summary>
    public sealed class InvestigateTask : ActorTaskBase
    {
        private readonly Point _target;
        private int _lookTicks;
        private bool _arrived;

        public InvestigateTask(Point target, int priority = 25, int lookDuration = 30)
            : base(priority, maxTurns: 200)
        {
            _target    = target;
            _lookTicks = lookDuration;
        }

        public override void Begin(in SimContext ctx)
        {
            base.Begin(ctx);
            _arrived = false;
            ctx.EnqueueMove(new MoveCommand(ctx.Actor.Handle, _target));
        }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (!_arrived)
            {
                if (ctx.Actor.Movement.IsMoving) return false;
                _arrived = true;
                return false;
            }

            // Stand and look around for _lookTicks ticks, then done.
            return --_lookTicks <= 0;
        }

        public override ITaskToken ToToken() => new InvestigateTaskToken(Priority, _target.X, _target.Y, _lookTicks);

        public override void Interrupt(in SimContext ctx)
            => ctx.Actor.Movement.ClearMovement();
    }
}
