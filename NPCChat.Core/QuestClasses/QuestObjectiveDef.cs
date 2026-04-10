namespace NPCChat.Core.QuestClasses
{
    /// <summary>
    /// Immutable definition of one objective within a quest.
    /// </summary>
    public sealed class QuestObjectiveDef
    {
        public string Id { get; init; } = string.Empty;

        public QuestObjectiveKind Kind { get; init; }

        /// <summary>Short description shown in the quest panel (e.g. "Collect iron ore").</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Item ID for CollectItem / DeliverItem objectives.
        /// NPC archetype string for TalkToNpc objectives.
        /// </summary>
        public string TargetId { get; init; } = string.Empty;

        /// <summary>How many units are required to satisfy this objective.</summary>
        public int RequiredCount { get; init; } = 1;
    }
}
