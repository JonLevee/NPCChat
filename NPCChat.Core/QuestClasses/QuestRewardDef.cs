namespace NPCChat.Core.QuestClasses
{
    /// <summary>
    /// Immutable definition of one item reward granted on quest completion.
    /// </summary>
    public sealed class QuestRewardDef
    {
        public string ItemId { get; init; } = string.Empty;
        public int Quantity { get; init; } = 1;
    }
}
