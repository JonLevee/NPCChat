namespace NPCChatLib.WorldClasses
{
    public enum Direction8 : byte
    {
        N = 0,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW
    }

    public static class Direction8Extensions
    {
        /// <summary>
        /// Returns the Direction8 value for a grid delta.
        /// dx positive = East, dx negative = West.
        /// dy positive = South (y increases downward), dy negative = North.
        /// Returns S if both deltas are zero.
        /// </summary>
        public static Direction8 FromDelta(int dx, int dy)
        {
            if (dx == 0 && dy < 0) return Direction8.N;
            if (dx > 0 && dy < 0) return Direction8.NE;
            if (dx > 0 && dy == 0) return Direction8.E;
            if (dx > 0 && dy > 0) return Direction8.SE;
            if (dx == 0 && dy > 0) return Direction8.S;
            if (dx < 0 && dy > 0) return Direction8.SW;
            if (dx < 0 && dy == 0) return Direction8.W;
            if (dx < 0 && dy < 0) return Direction8.NW;
            return Direction8.S;
        }

        /// <summary>
        /// Returns the (dx, dy) grid delta for a direction.
        /// </summary>
        public static (int dx, int dy) ToDelta(this Direction8 direction) => direction switch
        {
            Direction8.N  => ( 0, -1),
            Direction8.NE => ( 1, -1),
            Direction8.E  => ( 1,  0),
            Direction8.SE => ( 1,  1),
            Direction8.S  => ( 0,  1),
            Direction8.SW => (-1,  1),
            Direction8.W  => (-1,  0),
            Direction8.NW => (-1, -1),
            _             => ( 0,  1)
        };
    }
}
