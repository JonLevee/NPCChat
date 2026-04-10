namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// How an actor reacts when a higher-priority task requests an interrupt
    /// during this subtask.
    /// </summary>
    public enum InterruptDisposition : byte
    {
        /// <summary>Actor pauses willingly with no negative mood effect.</summary>
        Willing = 0,

        /// <summary>Actor pauses but expresses displeasure (mood/dialogue effect in Phase 5).</summary>
        Grudging = 1,

        /// <summary>Actor will not pause; the interrupting task must wait.</summary>
        Refuses = 2
    }
}
