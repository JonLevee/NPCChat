namespace NPCChat.Core.QuestClasses
{
    /// <summary>
    /// Immutable definition of a quest. Registered in StaticData at startup.
    /// </summary>
    public sealed class QuestDef
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public QuestObjectiveDef[] Objectives { get; init; } = [];
        public QuestRewardDef[] Rewards { get; init; } = [];
    }
}
