using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.UI.Xaml.Controls.Primitives;
using NPCChatLib.Attributes;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Transient]
    public class WorldGenerator
    {
        public WorldBuilder Builder { get; private set; }
        public WorldGeneratorStrategies GeneratorStrategies { get; set; }
        public WorldGenerator(WorldGeneratorStrategies generatorStrategies = null)
        {
            GeneratorStrategies = generatorStrategies ?? new WorldGeneratorStrategies();
            this.Builder = new WorldBuilder();
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
                var building = new YamlBuilding
                {
                    Name = shopInfo.Item1,
                    Size = shopInfo.Item3 == Size.Empty ? GeneratorStrategies.DefaultShopSize : shopInfo.Item3,
                };
                (building.Location, List<Point> points) = FindNextOpenBuildingLocation(GeneratorStrategies.BuildingLocatorStrategy, building.Size);
                Builder.CreateShop(building,points);
                var npcCount = shopInfo.Item2 < 0 ? GeneratorStrategies.DefaultPeoplePerShop : shopInfo.Item2;
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

        public (Point,List<Point>) FindNextOpenBuildingLocation(BuildingLocatorStrategy strategy, Size size)
        {
            switch (strategy)
            {
                case BuildingLocatorStrategy.Clockwise:
                    var corners = new Point[]
                    {
                        new Point(0, 0),
                        new Point(0, Builder.World.WorldSize.Width - size.Width),
                        new Point(Builder.World.WorldSize.Height - size.Height, Builder.World.WorldSize.Width - size.Width),
                        new Point(Builder.World.WorldSize.Height - size.Height, 0)
                    };
                    while (corners[0].X < Builder.World.WorldSize.Width/2 && corners[0].Y < Builder.World.WorldSize.Height/2)
                    {
                        if (!Builder.IsLocationOccupied(corners[0], size, out string errorMessage, out List<Point> points))
                        {
                            return (corners[0], points);
                        }
                        if (!Builder.IsLocationOccupied(corners[1], size, out errorMessage, out points))
                        {
                            return (corners[1], points);
                        }
                        if (!Builder.IsLocationOccupied(corners[2], size, out errorMessage, out points))
                        {
                            return (corners[2], points);
                        }
                        if (!Builder.IsLocationOccupied(corners[3], size, out errorMessage, out points))
                        {
                            return (corners[3], points);
                        }
                        corners[0] = new Point(corners[0].X + size.Height, corners[0].Y);
                        corners[1] = new Point(corners[1].X + size.Height, corners[1].Y);
                        corners[2] = new Point(corners[2].X - size.Height, corners[2].Y);
                        corners[3] = new Point(corners[3].X - size.Height, corners[3].Y);
                    }
                    throw new InvalidOperationException("No open building locations found using Clockwise strategy");
                default:
                    throw new InvalidOperationException($"{strategy} not handled");
            }
        }
    }
}
