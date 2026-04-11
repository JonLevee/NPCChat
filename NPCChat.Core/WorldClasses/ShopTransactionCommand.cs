#nullable enable
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// Enqueued by the UI thread; processed by the sim thread under the write lock.
    /// IsBuy = true  → player pays GoldCost gold, receives Quantity of item.
    /// IsBuy = false → player loses Quantity of item, receives GoldCost gold.
    /// <see cref="GoldItemDef"/> must be supplied for sell transactions so the sim
    /// thread can credit gold even when the player's inventory contains none yet.
    /// </summary>
    public readonly record struct ShopTransactionCommand(
        ObjectHandle PlayerHandle,
        string       ItemId,
        ItemDef      ItemDef,
        int          Quantity,
        int          GoldCost,
        bool         IsBuy,
        ItemDef?     GoldItemDef = null);
}
