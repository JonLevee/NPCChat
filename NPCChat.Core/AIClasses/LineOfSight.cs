using System;
using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.AIClasses
{
    /// <summary>
    /// Simple ray-march line-of-sight check on the tile grid.
    /// Samples half-tile steps along the segment from <c>from</c> to <c>to</c>
    /// and returns false if any sample falls inside a static obstacle.
    /// </summary>
    public static class LineOfSight
    {
        /// <summary>
        /// Returns true if there is an unobstructed line of sight from <paramref name="from"/>
        /// to <paramref name="to"/> given the supplied list of static obstacle bounds.
        /// </summary>
        public static bool Check(Point from, Point to, IReadOnlyList<Bounds> obstacles)
        {
            if (obstacles.Count == 0) return true;

            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len < 0.5f) return true;

            // Step in half-tile increments along the segment.
            int steps = (int)(len * 2) + 1;
            float nx = dx / len * 0.5f;
            float ny = dy / len * 0.5f;

            for (int i = 1; i <= steps; i++)
            {
                int tx = (int)MathF.Floor(from.X + nx * i);
                int ty = (int)MathF.Floor(from.Y + ny * i);

                foreach (var obs in obstacles)
                {
                    if (tx >= obs.Left && tx < obs.Right &&
                        ty >= obs.Top  && ty < obs.Bottom)
                        return false;
                }
            }
            return true;
        }
    }
}
