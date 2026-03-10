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
            do
            {
                yield return new Point(outerBounds.X, outerBounds.Y); 
                outerBounds.X += size.Width + world.SpacingOffset;
            } while (outerBounds.Height < outerBounds.X + outerBounds.Height / 2 &&
                outerBounds.Width < outerBounds.Y + outerBounds.Width / 2);
        }
    }
}
