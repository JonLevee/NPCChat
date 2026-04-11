#nullable enable
using System;
using System.Collections.Generic;
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.ShopClasses
{
    /// <summary>
    /// Attached to a WorldObjectMoveable to make it a merchant.
    /// Stock is set at world-build time and treated as unlimited supply.
    /// </summary>
    public sealed class ShopComponent
    {
        /// <summary>Items available for the player to buy.</summary>
        public List<ShopEntry> Stock { get; } = [];

        /// <summary>
        /// Fraction of BaseValue the shop pays when the player sells an item (0–1).
        /// </summary>
        public float SellMultiplier { get; init; } = 0.5f;

        /// <summary>Gold the shop pays for one unit of <paramref name="item"/>.</summary>
        public int GetSellPrice(ItemDef item) =>
            Math.Max(1, (int)Math.Floor(item.BaseValue * SellMultiplier));
    }
}
