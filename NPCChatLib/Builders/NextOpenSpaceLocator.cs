using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    public enum NextOpenSpaceLocatorStrategy
    {
        Clockwise,
    }

    [Scoped]
    public class NextOpenSpaceLocator
    {
        private delegate IEnumerable<Point> GetNextStartingPoint(YamlWorld world, Size size);
        private Dictionary<NextOpenSpaceLocatorStrategy, GetNextStartingPoint> strategyMap;
        private readonly BuildingOptions options;

        public NextOpenSpaceLocator(BuildingOptions options)
        {
            strategyMap = new()
            {
                [NextOpenSpaceLocatorStrategy.Clockwise] = GetNextClockwiseStartingPoint
            };
            this.options = options;
        }

        public bool TryFindNextOpenLocation(YamlWorld world, Size buildingSize, out Point nextLocation)
        {
            foreach (var startingPoint in strategyMap[options.LocatorStrategy](world, buildingSize))
            {
                if (!world.Occupied.IsLocationOccupied(startingPoint, buildingSize, out _))
                {
                    nextLocation = startingPoint;
                    return true;
                }
            }
            nextLocation = Point.Empty;
            return false;
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
