#nullable enable
using System.Collections.Generic;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// A unit of work an actor can perform. Owns a LinkedList of SubTasks that
    /// can be dynamically extended via SubTask.OnComplete callbacks.
    /// Owned and ticked exclusively by the simulation thread.
    /// </summary>
    public interface IActorTask
    {
        /// <summary>Higher value = processed before lower-priority tasks.</summary>
        int Priority { get; }

        /// <summary>
        /// Maximum ticks this task may run before being forcibly completed.
        /// Guards against infinite SubTask loops. Use int.MaxValue for no limit.
        /// </summary>
        int MaxTurns { get; }

        /// <summary>Ticks elapsed since Begin was called.</summary>
        int TurnsElapsed { get; }

        /// <summary>The ordered sequence of subtasks. May be extended by OnComplete callbacks.</summary>
        LinkedList<SubTask> SubTasks { get; }

        /// <summary>The currently executing subtask node, or null if the task has no subtasks.</summary>
        LinkedListNode<SubTask>? CurrentSubTaskNode { get; }

        /// <summary>Ticks remaining in the current subtask.</summary>
        int TurnsRemainingInCurrentSubTask { get; set; }

        /// <summary>Called once by AdvanceBehaviors on the first tick this task is at the front of the queue.</summary>
        void Begin(in SimContext ctx);

        /// <summary>
        /// Called each tick this task is active. Returns true when the task is complete.
        /// AdvanceBehaviors increments TurnsElapsed and enforces MaxTurns before calling Tick.
        /// </summary>
        bool Tick(in SimContext ctx);

        /// <summary>
        /// Called by AdvanceBehaviors when a higher-priority task displaces this one.
        /// Use to cancel pending move commands or clean up state.
        /// </summary>
        void Interrupt(in SimContext ctx);
    }
}
