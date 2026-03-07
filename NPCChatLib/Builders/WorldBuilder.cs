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

        public WorldBuilder CreateShop(YamlBuilding building, List<Point> points = null)
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
    }
}
