#nullable enable
namespace NPCChat.Core.ItemClasses
{
    public enum ItemKind
    {
        Weapon,
        Armour,
        Consumable,
        Material,
        Currency,
        Misc
    }

    /// <summary>
    /// Immutable definition of an item type. Registered in StaticData.Items at startup.
    /// Instances are shared — never mutated after registration.
    /// </summary>
    public sealed class ItemDef
    {
        /// <summary>Unique string identifier (e.g. "iron_sword"). Case-insensitive lookup.</summary>
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public ItemKind Kind { get; init; }

        /// <summary>Maximum quantity per inventory slot. 1 = non-stackable (weapons, armour).</summary>
        public int MaxStack { get; init; } = 1;

        /// <summary>Item weight in abstract units (reserved for future encumbrance).</summary>
        public float Weight { get; init; }

        /// <summary>Base buy/sell value in gold coins.</summary>
        public int BaseValue { get; init; }

        public string? Description { get; init; }
    }
}
