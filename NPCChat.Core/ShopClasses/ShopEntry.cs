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
        public required string  ItemId          { get; init; }
        public required ItemDef ItemDef         { get; init; }

        /// <summary>Multiplier applied to ItemDef.BaseValue to get the buy price.</summary>
        public float PriceMultiplier { get; init; } = 1.0f;

        /// <summary>Gold cost for the player to buy one unit.</summary>
        public int BuyPrice => (int)Math.Ceiling(ItemDef.BaseValue * PriceMultiplier);
    }
}
