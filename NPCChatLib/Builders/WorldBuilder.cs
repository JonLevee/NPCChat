using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Data;
using NPCChatLib.Attributes;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Transient]
    public class WorldBuilder
    {
        public WorldYaml World { get; private set; }

        public Dictionary<Point, YamlBuilding> Occupied { get; private set; } = [];

        public WorldBuilder()
        {
            World = new WorldYaml();
        }

        public WorldBuilder SetWorldSize(Size size)
        {
            World.WorldSize = size;
            return this;
        }

        public WorldYaml Build()
        {
            return World;
        }

        public WorldBuilder CreateShop(YamlBuilding building)
        {
            if (IsLocationOccupied(building.Location, building.Size, out var overlapInfo, out List<Point> points))
            {
                throw new Exception($"Building {building.Name} at " + overlapInfo);
            }
            World.Buildings.Add(building);
            AddBuildingToOccupied(building, points);
            return this;
        }

        public bool IsLocationOccupied(Point location, Size size, out string overlapInfo, out List<Point> points)
        {
            overlapInfo = null;
            points = [];
            for (var h = 0; h < size.Height; ++h)
            {
                for (var w = 0; h < size.Width; ++w)
                {
                    var newPoint = new Point(location.Y + h, location.X + w);
                    if (Occupied.TryGetValue(newPoint, out var building))
                    {
                        overlapInfo = $"[{location},{size}] overlaps existing building {building.Name} at {newPoint}";
                        return true;
                    }
                    points.Add(newPoint);
                }
            }
            return false;
        }

        private void AddBuildingToOccupied(YamlBuilding building, List<Point> points) => points.ForEach(p => Occupied.Add(p, building));

        public enum BuildingLocatorStrategy
        {
            Clockwise,
            LeftToRight,
            RightToLeft,
            Random
        }

        [Transient]
        public class WorldGeneratorStrategies
        {
            public BuildingLocatorStrategy BuildingLocatorStrategy = BuildingLocatorStrategy.Clockwise;
            public Size WorldSize { get; set; } = new Size(10, 10);
            public Size DefaultShopSize { get; set; } = new Size(2, 2);
            public int DefaultPeoplePerShop { get; set; } = 2;
        }

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
                GenerateShops(
                    ("Weapon Shop", 2, new Size(2, 2)),
                    ("Armor Shop", -1, Size.Empty)
                    );
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
                    building.Location = FindNextOpenBuildingLocation(GeneratorStrategies.BuildingLocatorStrategy, building.Size);
                    Builder.CreateShop(building);
                }
                return this;
            }

            public Point FindNextOpenBuildingLocation(BuildingLocatorStrategy strategy, Size size)
            {
                Point point = new Point();
                switch (strategy)
                {
                    case BuildingLocatorStrategy.Clockwise:
                        for (var verticalOffset = 0; verticalOffset < Builder.World.WorldSize.Height / 2; ++verticalOffset)
                        {

                        }
                        while (verticalOffset < Builder.World.WorldSize.Height / 2)
                        {
                            for (int x = verticalOffset; x < Builder.World.WorldSize.Width - verticalOffset; ++x)
                            {
                                point = new Point(verticalOffset, x);
                                if (!Builder.Occupied.ContainsKey(point))
                                    return point;
                            }
                            for (int y = verticalOffset; y < Builder.World.WorldSize.Height - verticalOffset; ++y)
                            {
                                point = new Point(y, Builder.World.WorldSize.Width - verticalOffset);
                                if (!Builder.Occupied.ContainsKey(point))
                                    return point;
                            }
                            for (int x = Builder.World.WorldSize.Width - verticalOffset; x >= verticalOffset; --x)
                            {
                                point = new Point(Builder.World.WorldSize.Height - verticalOffset, x);
                                if (!Builder.Occupied.ContainsKey(point))
                                    return point;
                            }
                            for (int y = Builder.World.WorldSize.Height - verticalOffset; y >= verticalOffset; --y)
                            {
                                point = new Point(y, verticalOffset);
                                if (!Builder.Occupied.ContainsKey(point))
                                    return point;
                            }
                            ++verticalOffset;
                        }
                        for (int y = 0; y < Builder.World.WorldSize.Height; ++y)
                            break;
                    default:
                        throw new InvalidOperationException($"{strategy} not handled");
                }
                return point;
            }
        }
    }
