using System.Collections.Generic;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.PathfindingClasses
{
    /// <summary>
    /// A snapshot of world obstacles used by the pathfinder.
    /// Built under the world read lock then used outside of it, so A* never
    /// holds the lock during computation.
    /// </summary>
    public sealed class PathGrid
    {
        private readonly List<Bounds> _obstacles;
        private readonly int _moverWidth;
        private readonly int _moverHeight;

        public PathGrid(List<Bounds> obstacles, int moverWidth, int moverHeight)
        {
            _obstacles = obstacles;
            _moverWidth = moverWidth;
            _moverHeight = moverHeight;
        }

        /// <summary>
        /// Returns true if the mover's full footprint fits at grid position (x, y)
        /// without overlapping any obstacle.
        /// </summary>
        public bool IsPassable(int x, int y)
        {
            var moverBounds = new Bounds(x, y, x + _moverWidth, y + _moverHeight);
            foreach (var obstacle in _obstacles)
            {
                if (obstacle.Intersects(moverBounds))
                    return false;
            }
            return true;
        }
    }
}
