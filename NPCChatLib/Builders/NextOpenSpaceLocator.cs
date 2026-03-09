using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    public enum NextOpenSpaceLocatorStrategy
    {
        Clockwise,
    }
    public class NextOpenSpaceLocator
    {
        private delegate IEnumerable<Point> GetNextStartingPoint(YamlWorld world, Size size);
        private Dictionary<NextOpenSpaceLocatorStrategy, GetNextStartingPoint> strategyMap;
        public NextOpenSpaceLocatorStrategy Strategy { get; set; }

        public NextOpenSpaceLocator(NextOpenSpaceLocatorStrategy strategy = NextOpenSpaceLocatorStrategy.Clockwise)
        {
            Strategy = strategy;
            strategyMap = new()
            {
                [NextOpenSpaceLocatorStrategy.Clockwise] = GetNextClockwiseStartingPoint
            };
        }

        public bool TryFindNextOpenLocation(YamlWorld world, Size buildingSize, out List<Point> openPoints)
        {
            foreach (var startingPoint in strategyMap[Strategy](world, buildingSize))
            {
                openPoints = [];
                foreach (var point in GetAllObjectPoints(startingPoint, buildingSize))
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

        public IEnumerable<Point> GetAllObjectPoints(Point point, Size size)
        {
            for (var y = point.Y; y < point.Y + size.Width; ++y)
            {
                for (var x = point.X; x < point.X + size.Height; ++x)
                {
                    yield return new Point(x, y);
                }
            }
        }
        private IEnumerable<Point> GetNextClockwiseStartingPoint(YamlWorld world, Size size)
        {
            var outerBounds = new Rectangle(Point.Empty, world.WorldSize);
            while (outerBounds.Height < outerBounds.X + outerBounds.Height / 2 &&
                outerBounds.Width < outerBounds.Y + outerBounds.Width / 2)
            {
                yield return new Point(outerBounds.X, outerBounds.Y); // Top-left
                yield return new Point(outerBounds.X, outerBounds.Y + outerBounds.Width - size.Width - 1); // Top-right
                yield return new Point(outerBounds.X + outerBounds.Height - size.Height - 1, outerBounds.Y + outerBounds.Width - size.Width - 1); // Bottom-right
                yield return new Point(outerBounds.X + outerBounds.Height - size.Height - 1, outerBounds.Y); // Bottom-left
                outerBounds.Inflate(-size.Height - world.SpacingOffset, -size.Height - world.SpacingOffset);
            }
        }
    }
}
