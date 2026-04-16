#nullable enable
using System;
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.ShopClasses
{
    /// <summary>
    /// One item line in an NPC's shop. Stock is considered unlimited.
    /// </summary>
    public sealed class ShopEntry
    {
        public string  ItemId          { get; init; } = string.Empty;
        public ItemDef ItemDef         { get; init; } = null!;

        /// <summary>Multiplier applied to ItemDef.BaseValue to get the buy price.</summary>
        public float PriceMultiplier { get; init; } = 1.0f;

        /// <summary>Gold cost for the player to buy one unit.</summary>
        public int BuyPrice => (int)Math.Ceiling(ItemDef.BaseValue * PriceMultiplier);
    }
}
