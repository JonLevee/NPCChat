using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private delegate IEnumerable<Point> GetNextStartingPoint(Size size);
        private Dictionary<NextOpenSpaceLocatorStrategy, GetNextStartingPoint> strategyMap;
        private readonly BuildingOptions options;
        private readonly YamlWorld yamlWorld;

        public NextOpenSpaceLocator(BuildingOptions options, YamlWorld yamlWorld)
        {
            strategyMap = new()
            {
                [NextOpenSpaceLocatorStrategy.Clockwise] = GetNextClockwiseStartingPoint
            };
            this.options = options;
            this.yamlWorld = yamlWorld;
        }

        public bool TryFindNextOpenLocation(Size buildingSize, out Point nextLocation)
        {
            foreach (var startingPoint in strategyMap[options.LocatorStrategy](buildingSize))
            {
                if (!yamlWorld.Occupied.IsLocationOccupied(startingPoint, buildingSize, out _))
                {
                    nextLocation = startingPoint;
                    return true;
                }
            }
            nextLocation = Point.Empty;
            return false;
        }

        private bool SurroundingPointsAreOpen(Point point)
        {
            if (yamlWorld.Occupied.ContainsKey(point))
                return false;
            for (var x = point.X - yamlWorld.SpacingOffset; x < point.X + yamlWorld.SpacingOffset; ++x)
            {
                for (var y = point.Y - yamlWorld.SpacingOffset; y < point.Y + yamlWorld.SpacingOffset; ++y)
                {
                    if (x >= 0 &&
                        x < yamlWorld.WorldSize.Width &&
                        y >= 0 &&
                        y < yamlWorld.WorldSize.Height &&
                        yamlWorld.Occupied.ContainsKey(new Point(x, y)))
                        return false;
                }
            }
            return true;
        }

        private bool MoveUntilEmpty(ref Point point, int xDelta = 0, int yDelta = 0)
        {
            Assert.IsTrue(xDelta + yDelta > 0 && (xDelta > 0 || yDelta > 0), $"one must be non-zero: xDelta:{xDelta}, yDelta: {yDelta}");
            //if (!yamlWorld.Occupied.ContainsKey(nextPoint))
            throw new NotImplementedException();
        }

        private IEnumerable<Point> GetNextClockwiseStartingPoint(Size size)
        {
            var nextPoint = new Point();

            while (nextPoint.Y < yamlWorld.WorldSize.Height - size.Height)
            {
                while (nextPoint.X < yamlWorld.WorldSize.Width - size.Width)
                {
                    if (SurroundingPointsAreOpen(nextPoint))
                        yield return nextPoint;
                    nextPoint.X++;
                }
            }
            var outerBounds = new Rectangle(Point.Empty, yamlWorld.WorldSize);
            do
            {
                yield return new Point(outerBounds.X, outerBounds.Y);
                outerBounds.X += size.Width + yamlWorld.SpacingOffset;
            } while (outerBounds.Height < outerBounds.X + outerBounds.Height / 2 &&
                outerBounds.Width < outerBounds.Y + outerBounds.Width / 2);
        }
    }
}
