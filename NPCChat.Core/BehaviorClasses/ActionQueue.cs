#nullable enable
using System.Collections.Generic;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// Priority-ordered queue of IActorTask. The highest-priority task is always
    /// at the front. Owned exclusively by the simulation thread.
    /// </summary>
    public sealed class ActionQueue
    {
        // Stored as a sorted list; small N makes linear insertion acceptable.
        private readonly List<IActorTask> _tasks = new List<IActorTask>();

        public bool IsEmpty => _tasks.Count == 0;

        /// <summary>Read-only view of all queued tasks, highest priority first.</summary>
        public IReadOnlyList<IActorTask> AllTasks => _tasks;

        /// <summary>Inserts the task in priority order (highest priority first).</summary>
        public void Enqueue(IActorTask task)
        {
            int i = 0;
            while (i < _tasks.Count && _tasks[i].Priority >= task.Priority)
                i++;
            _tasks.Insert(i, task);
        }

        /// <summary>Returns the highest-priority task without removing it, or null if empty.</summary>
        public IActorTask? TryPeekHighest() => _tasks.Count > 0 ? _tasks[0] : null;

        /// <summary>Removes and returns the highest-priority task, or null if empty.</summary>
        public IActorTask? Dequeue()
        {
            if (_tasks.Count == 0) return null;
            var task = _tasks[0];
            _tasks.RemoveAt(0);
            return task;
        }

        /// <summary>Removes all tasks without calling Interrupt on them.</summary>
        public void Clear() => _tasks.Clear();
    }
}
