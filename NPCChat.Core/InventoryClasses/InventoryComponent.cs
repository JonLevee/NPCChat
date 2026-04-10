#nullable enable
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.InventoryClasses
{
    /// <summary>
    /// Slot-based inventory that can be attached to any WorldObject.
    /// Thread safety: callers (sim thread) are responsible for synchronisation.
    /// </summary>
    public sealed class InventoryComponent
    {
        private readonly List<InventorySlot> _slots = new();

        /// <summary>Maximum number of slots. 0 = unlimited.</summary>
        public int MaxSlots { get; init; }

        /// <summary>
        /// Item kinds this inventory will accept. Null = accept all kinds.
        /// Use this to restrict containers (e.g. a quiver only takes Weapon ammo).
        /// </summary>
        public ItemKind[]? AllowedKinds { get; init; }

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public bool CanAccept(ItemDef item)
            => AllowedKinds is null || AllowedKinds.Contains(item.Kind);

        /// <summary>
        /// Tries to add <paramref name="quantity"/> of <paramref name="item"/>.
        /// Returns true if all were added. <paramref name="remaining"/> is however many
        /// could not fit (0 on full success).
        /// </summary>
        public bool TryAdd(ItemDef item, int quantity, out int remaining)
        {
            remaining = quantity;
            if (!CanAccept(item)) return false;

            // Fill existing stacks first (stackable items only).
            if (item.MaxStack > 1)
            {
                foreach (var slot in _slots)
                {
                    if (!string.Equals(slot.Item.Id, item.Id, StringComparison.OrdinalIgnoreCase)) continue;
                    int space = item.MaxStack - slot.Quantity;
                    if (space <= 0) continue;
                    int add = Math.Min(space, remaining);
                    slot.Quantity += add;
                    remaining    -= add;
                    if (remaining == 0) return true;
                }
            }

            // Open new slots for whatever is still remaining.
            while (remaining > 0)
            {
                if (MaxSlots > 0 && _slots.Count >= MaxSlots)
                    return false;   // no room
                int add = Math.Min(item.MaxStack, remaining);
                _slots.Add(new InventorySlot(item, add));
                remaining -= add;
            }

            return true;
        }

        /// <summary>
        /// Tries to remove up to <paramref name="quantity"/> of the item with
        /// <paramref name="itemId"/>. Returns true if at least one was removed.
        /// <paramref name="removed"/> is the actual count taken.
        /// </summary>
        public bool TryRemove(string itemId, int quantity, out int removed)
        {
            removed = 0;
            int need = quantity;

            // Iterate backwards so RemoveAt indices stay valid.
            for (int i = _slots.Count - 1; i >= 0 && need > 0; i--)
            {
                var slot = _slots[i];
                if (!string.Equals(slot.Item.Id, itemId, StringComparison.OrdinalIgnoreCase)) continue;

                int take    = Math.Min(slot.Quantity, need);
                slot.Quantity -= take;
                removed       += take;
                need          -= take;

                if (slot.Quantity == 0)
                    _slots.RemoveAt(i);
            }

            return removed > 0;
        }

        /// <summary>Total quantity of the item with the given id across all slots.</summary>
        public int CountOf(string itemId)
            => _slots
                .Where(s => string.Equals(s.Item.Id, itemId, StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Quantity);
    }
}
