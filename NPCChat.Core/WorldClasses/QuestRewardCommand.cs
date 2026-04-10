#nullable enable
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// Enqueued by the UI thread (dialogue effect) to add or remove items from a
    /// player's inventory. Processed by the simulation thread to maintain thread safety.
    /// </summary>
    public readonly record struct QuestRewardCommand(
        ObjectHandle Target,
        string ItemId,
        ItemDef? ItemDef,   // required for additions; null for removals
        int Quantity,
        bool IsRemoval);
}
