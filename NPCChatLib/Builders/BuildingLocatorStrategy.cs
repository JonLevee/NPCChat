using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    public interface IBuildingLocatorStrategy
    {
        bool TryFindNextOpenLocation(YamlWorld world, Size buildingSize, out List<Point> openPoints);
        IEnumerable<Point> GetNextStartingPointToCheck(YamlWorld world, Size buildingSize);
        IEnumerable<Point> GetInnerPointsToCheck(Point point, Size buildingSize);
    }

    public abstract class BuildingLocatorStrategy : IBuildingLocatorStrategy
    {
        public static IBuildingLocatorStrategy Clockwise { get; } = new BuildingLocatorStrategyClockwise();
        public abstract IEnumerable<Point> GetNextStartingPointToCheck(YamlWorld world, Size buildingSize);

        public bool TryFindNextOpenLocation(YamlWorld world, Size buildingSize, out List<Point> openPoints)
        {
            foreach (var startingPoint in GetNextStartingPointToCheck(world, buildingSize))
            {
                openPoints = [];
                foreach (var point in GetInnerPointsToCheck(startingPoint, buildingSize))
                {
                    if (world.Occupied.ContainsKey(point))
                    {
                        openPoints.Clear();
                        break;
                    }
                }
                if (openPoints.Any())
                {
                    return true;
                }
            }
            openPoints = [];
            return false;
        }

        public IEnumerable<Point> GetInnerPointsToCheck(Point point, Size buildingSize)
        {
            for (var y = point.Y; y < point.Y + buildingSize.Width; ++y)
            {
                for (var x = point.X; x < point.X + buildingSize.Height; ++x)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }

    public class BuildingLocatorStrategyClockwise : BuildingLocatorStrategy
    {
        public override IEnumerable<Point> GetNextStartingPointToCheck(YamlWorld world, Size buildingSize)
        {
            var outerBounds = new Rectangle(Point.Empty, world.WorldSize);
            while (outerBounds.Height < outerBounds.X + outerBounds.Height / 2 &&
                outerBounds.Width < outerBounds.Y + outerBounds.Width / 2)
            {
                yield return new Point(outerBounds.X, outerBounds.Y); // Top-left
                yield return new Point(outerBounds.X, outerBounds.Y + outerBounds.Width - buildingSize.Width - 1); // Top-right
                yield return new Point(outerBounds.X + outerBounds.Height - buildingSize.Height - 1, outerBounds.Y + outerBounds.Width - buildingSize.Width - 1); // Bottom-right
                yield return new Point(outerBounds.X + outerBounds.Height - buildingSize.Height - 1, outerBounds.Y); // Bottom-left
                outerBounds.Inflate(-buildingSize.Height - world.SpacingOffset, -buildingSize.Height - world.SpacingOffset);
            }
        }
    }
}
