using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls.Primitives;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Scoped]
    public class WorldGenerator
    {
        public WorldBuilder Builder { get; }
        public YamlWorld World { get; }
        public BuildingOptions Strategies { get; }
        public WorldGenerator(IServiceScope scope)
        {
            this.Strategies = scope.Get<BuildingOptions>();
            this.Builder = scope.Get<WorldBuilder>();
            this.World = scope.Get<YamlWorld>();
        }

        public WorldGenerator SetOptions(Action<BuildingOptions> setFunc)
        {
            setFunc(Strategies);
            return this;
        }


        public WorldGenerator GenerateDefault()
        {
            GenerateArchetypes();
            GenerateShops(
                ("Weapon Shop", 2, Size.Empty),
                ("Armor Shop", -1, Size.Empty)
                );
            return this;
        }

        public WorldGenerator GenerateArchetypes()
        {
            return this;
        }

        public WorldGenerator GenerateShops(params (string, int, Size)[] shopInfos)
        {
            foreach (var shopInfo in shopInfos)
            {
                var name = shopInfo.Item1;
                var npcCount = shopInfo.Item2 == -1 ? Strategies.DefaultPeoplePerShop : shopInfo.Item2;
                var size = shopInfo.Item3 == Size.Empty ? Strategies.DefaultShopSize : shopInfo.Item3;

                if (!Builder.SpaceLocator.TryFindNextOpenLocation(World, size, out Point point))
                    throw new InvalidOperationException($"No open building locations found for shop {name} with size {size}");
                var building = new YamlBuilding
                {
                    Name = name,
                    Size = size,
                    Location = point,
                };
                World.Occupied.Add(building);
                GenerateShopCharacters(building, npcCount);
                if (--npcCount >= 0)
                {
                    Builder.CreateShopkeeper(building);
                }
                if (--npcCount >= 0)
                {
                    Builder.CreateShopkeeperAssistant(building);
                }
            }
            return this;
        }

        public WorldGenerator GenerateShopCharacters(YamlBuilding building, int characterCount)
        {
            if (--characterCount >= 0)
            {
                Builder.CreateShopkeeper(building);
                if (--characterCount >= 0)
                {
                    Builder.CreateShopkeeper(building);
                }
            }
            return this;
        }
    }
}
