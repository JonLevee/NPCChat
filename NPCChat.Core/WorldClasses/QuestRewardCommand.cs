#nullable enable
using System;
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// Enqueued by the UI thread (dialogue effect) to add or remove items from a
    /// player's inventory. Processed by the simulation thread to maintain thread safety.
    /// </summary>
    public readonly struct QuestRewardCommand : IEquatable<QuestRewardCommand>
    {
        public ObjectHandle Target { get; }
        public string ItemId { get; }
        public ItemDef? ItemDef { get; }   // required for additions; null for removals
        public int Quantity { get; }
        public bool IsRemoval { get; }

        public QuestRewardCommand(ObjectHandle target, string itemId, ItemDef? itemDef, int quantity, bool isRemoval)
        {
            Target = target;
            ItemId = itemId;
            ItemDef = itemDef;
            Quantity = quantity;
            IsRemoval = isRemoval;
        }

        public bool Equals(QuestRewardCommand other) =>
            Target.Equals(other.Target) && ItemId == other.ItemId &&
            Quantity == other.Quantity && IsRemoval == other.IsRemoval;
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override bool Equals(object obj) => obj is QuestRewardCommand other && Equals(other);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override int GetHashCode() => HashCode.Combine(Target, ItemId, Quantity, IsRemoval);
        public static bool operator ==(QuestRewardCommand left, QuestRewardCommand right) => left.Equals(right);
        public static bool operator !=(QuestRewardCommand left, QuestRewardCommand right) => !left.Equals(right);
    }
}
