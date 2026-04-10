#nullable enable
using System.Collections.Generic;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// A single step within an IActorTask. SubTasks execute sequentially.
    /// OnComplete may inject new subtasks immediately after this one in the
    /// owning task's SubTasks list, enabling dynamic, data-driven sequences.
    /// </summary>
    public sealed class SubTask
    {
        /// <summary>Human-readable name for debugging and editor display.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>How many sim ticks this subtask takes to complete.</summary>
        public int DurationTurns { get; init; } = 1;

        /// <summary>Whether a higher-priority task may pause this subtask mid-execution.</summary>
        public bool IsInterruptible { get; init; } = true;

        /// <summary>How the actor reacts if interrupted during this subtask.</summary>
        public InterruptDisposition InterruptDisposition { get; init; } = InterruptDisposition.Willing;

        /// <summary>
        /// Optional callback invoked when this subtask completes. Return subtasks to
        /// insert directly after this node in the owning task's SubTasks list.
        /// Return null or empty to make no change.
        /// </summary>
        public Func<SimContext, IEnumerable<SubTask>?>? OnComplete { get; init; }
    }
}
