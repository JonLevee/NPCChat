using System;
using System.Drawing;
using NPCChat.Core.Attributes;
using NPCChat.Core.Builders;
using NPCChat.Core.InventoryClasses;
using NPCChat.Core.ItemClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.WorldBuilderTemplates
{
    public static class BuildingSize
    {
        /// <summary>
        /// Width:5, Height: 3
        /// </summary>
        public readonly static Size Small = new(5, 3);
    }

    [Transient]
    public partial class Templates(WorldDataBuilder builder) : IDisposable
    {
        private const float DefaultMoveSpeed  = 4.0f;  // grid units per second
        private const float DefaultItemRadius = 2.0f;  // grid units — auto-pickup range
        private readonly Size CharacterSize = new Size(2, 2);

        public Templates AddPlayer(int x, int y)
        {
            var o = new WorldObjectMoveable
            {
                Kind                = WorldObjectKind.Player,
                Handle              = ObjectHandle.None,
                Bounds              = new Bounds(x, y, CharacterSize),
                MaxSpeed            = DefaultMoveSpeed,
                ItemPickupRadius    = DefaultItemRadius,
                ResourcePickupRadius = 0f,
                Inventory           = new InventoryComponent { MaxSlots = 30 }
            };
            builder.World.AddObject(o);
            return this;
        }

        public Templates AddNPC(int x, int y) => AddMoveable(WorldObjectKind.NPC, x, y, CharacterSize);
        public Templates AddShop(int x, int y, Size size) => AddStatic(WorldObjectKind.Building, x, y, size);

        /// <summary>
        /// Registers core item definitions into StaticData. Called once at world startup.
        /// </summary>
        public Templates RegisterSeedItems()
        {
            var items = new ItemDef[]
            {
                new() { Id = "iron_sword",    Name = "Iron Sword",    Kind = ItemKind.Weapon,      MaxStack = 1,    Weight = 3.0f, BaseValue = 40  },
                new() { Id = "leather_armour",Name = "Leather Armour",Kind = ItemKind.Armour,       MaxStack = 1,    Weight = 5.0f, BaseValue = 30  },
                new() { Id = "health_potion", Name = "Health Potion", Kind = ItemKind.Consumable,   MaxStack = 10,   Weight = 0.5f, BaseValue = 15, Description = "Restores health." },
                new() { Id = "iron_ore",      Name = "Iron Ore",      Kind = ItemKind.Material,     MaxStack = 50,   Weight = 2.0f, BaseValue = 5   },
                new() { Id = "gold_coin",     Name = "Gold Coin",     Kind = ItemKind.Currency,     MaxStack = 9999, Weight = 0.01f,BaseValue = 1   },
                new() { Id = "arrow",         Name = "Arrow",         Kind = ItemKind.Weapon,       MaxStack = 99,   Weight = 0.1f, BaseValue = 1   },
                new() { Id = "wood_plank",    Name = "Wood Plank",    Kind = ItemKind.Material,     MaxStack = 50,   Weight = 1.0f, BaseValue = 2   },
                new() { Id = "bread",         Name = "Bread",         Kind = ItemKind.Consumable,   MaxStack = 10,   Weight = 0.3f, BaseValue = 3,  Description = "Simple food." },
            };

            foreach (var item in items)
                builder.StaticData.RegisterItem(item);

            return this;
        }

        /// <summary>Spawns a loose item on the map.</summary>
        public Templates AddItem(string itemId, int x, int y, int quantity = 1)
        {
            var itemDef = builder.StaticData.GetItem(itemId)
                ?? throw new InvalidOperationException($"Unknown item id '{itemId}'. Call RegisterSeedItems() first.");

            var o = new WorldObjectCarryable
            {
                Kind     = WorldObjectKind.Item,
                Handle   = ObjectHandle.None,
                Bounds   = new Bounds(x, y, x + 1, y + 1),
                ItemDef  = itemDef,
                Quantity = quantity
            };

            builder.World.AddObject(o);
            return this;
        }

        public Templates AddSmallTown()
        {
            RegisterSeedItems();

            AddShop(10, 2, BuildingSize.Small);
            AddShop(18, 2, BuildingSize.Small);
            AddShop(2, 8, BuildingSize.Small);
            AddShop(12, 9, BuildingSize.Small);
            AddShop(22, 8, BuildingSize.Small);
            AddShop(6, 16, BuildingSize.Small);
            AddShop(20, 16, BuildingSize.Small);
            AddPlayer(16, 14);
            AddBlacksmith(8, 14, shopX: 2, shopY: 2);
            AddFarmer(24, 14);

            // Scatter a few items for auto-pickup testing.
            AddItem("gold_coin",     17, 14, quantity: 10);
            AddItem("health_potion", 16, 16, quantity: 2);
            AddItem("iron_ore",      18, 15, quantity: 3);

            return this;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private Templates AddMoveable(WorldObjectKind kind, int x, int y, Size size)
        {
            var o = new WorldObjectMoveable
            {
                Kind = kind,
                Handle = ObjectHandle.None,
                Bounds = new Bounds(x, y, size),
                MaxSpeed = DefaultMoveSpeed
            };

            builder.World.AddObject(o);
            return this;
        }

        private Templates AddStatic(WorldObjectKind kind, int x, int y, Size size)
        {
            var o = new WorldObjectStatic
            {
                Kind = kind,
                Handle = ObjectHandle.None,
                Bounds = new Bounds(x, y, size)
            };

            builder.World.AddObject(o);
            return this;
        }
    }
}
