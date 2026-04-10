namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// The actor stands still for a fixed number of ticks, then the task completes.
    /// Used as a default "off-hours" or rest task.
    /// </summary>
    public sealed class IdleTask : ActorTaskBase
    {
        private readonly int _durationTicks;
        private int _ticksRemaining;

        /// <param name="durationTicks">How many ticks to idle. Use int.MaxValue to idle indefinitely.</param>
        /// <param name="priority">Task priority in the ActionQueue.</param>
        public IdleTask(int durationTicks, int priority = 0) : base(priority)
        {
            _durationTicks = durationTicks;
        }

        public override void Begin(in SimContext ctx)
        {
            _ticksRemaining = _durationTicks;
        }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (_durationTicks == int.MaxValue) return false;
            return --_ticksRemaining <= 0;
        }
    }
}
