using System;
using System.Collections.Generic;
using System.Drawing;

namespace NPCChat.Core.PathfindingClasses
{
    /// <summary>
    /// A* pathfinder for 8-directional grid movement.
    /// Uses an octile distance heuristic and strict diagonal corner-cutting prevention.
    /// </summary>
    public static class AStarPathfinder
    {
        private static readonly float Sqrt2 = (float)Math.Sqrt(2.0);

        // Cardinal directions (cost 1) then diagonal directions (cost √2).
        // Order: N, E, S, W, NE, SE, SW, NW
        private static readonly (int dx, int dy, float cost)[] Neighbors =
        [
            ( 0, -1, 1f),
            ( 1,  0, 1f),
            ( 0,  1, 1f),
            (-1,  0, 1f),
            ( 1, -1, 1.4142136f),
            ( 1,  1, 1.4142136f),
            (-1,  1, 1.4142136f),
            (-1, -1, 1.4142136f),
        ];

        /// <summary>
        /// Finds a path from <paramref name="source"/> to <paramref name="target"/> on the given grid.
        /// Both points are the top-left corner of the mover's footprint.
        /// </summary>
        /// <returns>
        /// List of positions from source (exclusive) to target (inclusive), or
        /// null if no path exists or <paramref name="maxIterations"/> is exceeded.
        /// Returns an empty list if source == target.
        /// </returns>
        public static List<Point> FindPath(PathGrid grid, Point source, Point target, int maxIterations)
        {
            if (source == target)
                return [];

            if (!grid.IsPassable(target.X, target.Y))
                return null;

            var openSet   = new PriorityQueue<Point, float>();
            var gScore    = new Dictionary<Point, float>();
            var cameFrom  = new Dictionary<Point, Point>();
            var closedSet = new HashSet<Point>();

            gScore[source] = 0f;
            openSet.Enqueue(source, Heuristic(source, target));

            int iterations = 0;

            while (openSet.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                var current = openSet.Dequeue();

                if (current == target)
                    return ReconstructPath(cameFrom, current);

                // Skip stale open-set entries (lazy deletion).
                if (!closedSet.Add(current))
                    continue;

                foreach (var (dx, dy, cost) in Neighbors)
                {
                    var neighbor = new Point(current.X + dx, current.Y + dy);

                    if (closedSet.Contains(neighbor))
                        continue;

                    if (!grid.IsPassable(neighbor.X, neighbor.Y))
                        continue;

                    // Strict diagonal corner-cutting prevention:
                    // both orthogonal neighbours must be passable.
                    if (dx != 0 && dy != 0)
                    {
                        if (!grid.IsPassable(current.X + dx, current.Y) ||
                            !grid.IsPassable(current.X,      current.Y + dy))
                            continue;
                    }

                    float tentativeG = gScore[current] + cost;

                    if (gScore.TryGetValue(neighbor, out float existingG) && tentativeG >= existingG)
                        continue;

                    gScore[neighbor]   = tentativeG;
                    cameFrom[neighbor] = current;
                    openSet.Enqueue(neighbor, tentativeG + Heuristic(neighbor, target));
                }
            }

            return null; // No path found or iteration limit reached.
        }

        private static float Heuristic(Point a, Point b)
        {
            float dx = Math.Abs(a.X - b.X);
            float dy = Math.Abs(a.Y - b.Y);
            // Octile distance: optimal heuristic for 8-directional grids.
            return Math.Max(dx, dy) + (Sqrt2 - 1f) * Math.Min(dx, dy);
        }

        private static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point>();
            while (cameFrom.TryGetValue(current, out var prev))
            {
                path.Add(current);
                current = prev;
            }
            path.Reverse();
            return path;
        }
    }
}
