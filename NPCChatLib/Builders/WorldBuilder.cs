using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Attributes;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Transient]
    public class WorldBuilder
    {
        public YamlWorld World { get; private set; }
        public WorldBuilderStrategies BuilderStrategies { get; set; }
        public NextOpenSpaceLocator SpaceLocator { get; }
        public Dictionary<Point, YamlBuilding> Occupied { get; private set; } = [];

        public WorldBuilder(WorldBuilderStrategies builderStrategies, NextOpenSpaceLocator spaceLocator)
        {
            World = new YamlWorld();
            BuilderStrategies = builderStrategies;
            SpaceLocator = spaceLocator;
        }

        public WorldBuilder SetWorldSize(Size size)
        {
            World.WorldSize = size;
            return this;
        }

        public YamlWorld Build()
        {
            return World;
        }

        public WorldBuilder CreateShop(YamlBuilding building)
        {
            if (points == null && IsLocationOccupied(building.Location, building.Size, out var overlapInfo, out points))
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

        internal void CreateShopkeeper(YamlBuilding building)
        {

        }

        internal void CreateShopkeeperAssistant(YamlBuilding building)
        {
            throw new NotImplementedException();
        }

        internal void Add(IYamlObject yamlObject, List<Point> points = null)
        {
            Assert.AreNotEqual(Size.Empty, yamlObject.Size);
            if (points == null)
            {
                if (IsLocationOccupied(yamlObject.Location, yamlObject.Size, out var overlapInfo, out points))
                {
                    throw new Exception($"Object {yamlObject.Name} at " + overlapInfo);
                }
                yamlObject.Location = points.First();
            }
            Assert.IsNotNull(points);

        }
    }
}
