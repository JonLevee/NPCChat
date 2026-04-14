#nullable enable
using System;
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
    public readonly struct ShopTransactionCommand : IEquatable<ShopTransactionCommand>
    {
        public ObjectHandle PlayerHandle { get; }
        public string ItemId { get; }
        public ItemDef ItemDef { get; }
        public int Quantity { get; }
        public int GoldCost { get; }
        public bool IsBuy { get; }
        public ItemDef? GoldItemDef { get; }

        public ShopTransactionCommand(
            ObjectHandle playerHandle,
            string itemId,
            ItemDef itemDef,
            int quantity,
            int goldCost,
            bool isBuy,
            ItemDef? goldItemDef = null)
        {
            PlayerHandle = playerHandle;
            ItemId = itemId;
            ItemDef = itemDef;
            Quantity = quantity;
            GoldCost = goldCost;
            IsBuy = isBuy;
            GoldItemDef = goldItemDef;
        }

        public bool Equals(ShopTransactionCommand other) =>
            PlayerHandle.Equals(other.PlayerHandle) && ItemId == other.ItemId &&
            Quantity == other.Quantity && GoldCost == other.GoldCost && IsBuy == other.IsBuy;
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override bool Equals(object obj) => obj is ShopTransactionCommand other && Equals(other);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override int GetHashCode() => HashCode.Combine(PlayerHandle, ItemId, Quantity, GoldCost, IsBuy);
        public static bool operator ==(ShopTransactionCommand left, ShopTransactionCommand right) => left.Equals(right);
        public static bool operator !=(ShopTransactionCommand left, ShopTransactionCommand right) => !left.Equals(right);
    }
}
