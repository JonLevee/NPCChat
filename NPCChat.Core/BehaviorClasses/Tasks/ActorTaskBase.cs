#nullable enable
using System.Collections.Generic;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// Base class for IActorTask implementations. Manages the SubTask linked list,
    /// TurnsElapsed, and TurnsRemainingInCurrentSubTask so concrete tasks only need
    /// to implement Begin, Tick, and Interrupt.
    /// </summary>
    public abstract class ActorTaskBase : IActorTask
    {
        public int Priority { get; }
        public int MaxTurns { get; }
        public int TurnsElapsed { get; private set; }
        public LinkedList<SubTask> SubTasks { get; } = new();
        public LinkedListNode<SubTask>? CurrentSubTaskNode { get; private set; }
        public int TurnsRemainingInCurrentSubTask { get; set; }

        protected ActorTaskBase(int priority, int maxTurns = int.MaxValue)
        {
            Priority = priority;
            MaxTurns = maxTurns;
        }

        public virtual void Begin(in SimContext ctx)
        {
            CurrentSubTaskNode = SubTasks.First;
            if (CurrentSubTaskNode is not null)
                TurnsRemainingInCurrentSubTask = CurrentSubTaskNode.Value.DurationTurns;
        }

        public bool Tick(in SimContext ctx)
        {
            TurnsElapsed++;

            if (CurrentSubTaskNode is null)
                return TickNoSubTasks(ctx);

            // Advance the current subtask.
            TurnsRemainingInCurrentSubTask--;

            if (TurnsRemainingInCurrentSubTask > 0)
                return false; // Still working on this subtask.

            // Subtask complete — run OnComplete to get any injected subtasks.
            var completed = CurrentSubTaskNode.Value;
            if (completed.OnComplete is not null)
            {
                var injected = completed.OnComplete(ctx);
                if (injected is not null)
                {
                    var insertAfter = CurrentSubTaskNode;
                    foreach (var sub in injected)
                    {
                        SubTasks.AddAfter(insertAfter, sub);
                        insertAfter = insertAfter.Next!;
                    }
                }
            }

            // Advance to the next subtask node.
            CurrentSubTaskNode = CurrentSubTaskNode.Next;

            if (CurrentSubTaskNode is null)
                return OnAllSubTasksComplete(ctx);

            TurnsRemainingInCurrentSubTask = CurrentSubTaskNode.Value.DurationTurns;
            return false;
        }

        /// <summary>
        /// Called when the task has no SubTasks. Override to implement tick logic
        /// directly. Return true when the task is complete.
        /// </summary>
        protected virtual bool TickNoSubTasks(in SimContext ctx) => true;

        /// <summary>
        /// Called once all SubTasks have completed. Return true to complete the task,
        /// false to keep it active (e.g., a looping task that re-populates SubTasks).
        /// </summary>
        protected virtual bool OnAllSubTasksComplete(in SimContext ctx) => true;

        public virtual void Interrupt(in SimContext ctx) { }
    }
}
