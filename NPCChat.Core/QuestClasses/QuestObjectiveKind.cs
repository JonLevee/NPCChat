namespace NPCChat.Core.QuestClasses
{
    public enum QuestObjectiveKind
    {
        /// <summary>Player must have a quantity of an item in their inventory.</summary>
        CollectItem,
        /// <summary>Player must initiate a conversation with a specific NPC archetype.</summary>
        TalkToNpc,
        /// <summary>Player must give an item to a specific NPC.</summary>
        DeliverItem
    }
}
