using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.InventoryClasses
{
    /// <summary>One stack of items occupying a single inventory slot.</summary>
    public sealed class InventorySlot
    {
        public InventorySlot(ItemDef item, int quantity)
        {
            Item     = item;
            Quantity = quantity;
        }

        public ItemDef Item     { get; }
        public int Quantity { get; set; }
    }
}
