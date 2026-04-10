namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// A reactive rule evaluated each tick. When Trigger returns true and this
    /// rule's Priority exceeds the current task's priority, a new task is injected
    /// into the actor's ActionQueue via ActionFactory.
    /// </summary>
    public sealed class BehaviorRule
    {
        /// <summary>
        /// Condition evaluated each tick. Should be fast (no pathfinding, no allocations).
        /// </summary>
        public required Func<SimContext, bool> Trigger { get; init; }

        /// <summary>
        /// Creates the task to enqueue when Trigger fires. Called at most once per
        /// trigger event — the returned task is pushed into the ActionQueue.
        /// </summary>
        public required Func<SimContext, IActorTask> ActionFactory { get; init; }

        /// <summary>Higher value = higher priority than the default task.</summary>
        public int Priority { get; init; }
    }
}
