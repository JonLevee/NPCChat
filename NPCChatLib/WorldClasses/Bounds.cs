using System.Diagnostics;
using System.Drawing;

namespace NPCChatLib.WorldClasses
{
    [DebuggerDisplay("{DebugText}")]
    public readonly record struct Bounds : IAutoDebugDisplay
    {
        [DebugDisplay]
        public int Left { get; }
        [DebugDisplay]
        public int Top { get; }
        [DebugDisplay]
        public int Right { get; }
        [DebugDisplay]
        public int Bottom { get; }
        [DebugDisplay]
        public int Width => Right - Left;
        [DebugDisplay]
        public int Height => Bottom - Top;

        public Bounds(int X, int Y, int Width, int Height)
        {
            Left = X;
            Top = Y;
            Right = X + Width;
            Bottom = Y + Height;
        }
        public Bounds(Position position, Size size) : this(position.X, position.Y, size.Width, size.Height) { }
        public Bounds(Position position) : this(position.X, position.Y, 1, 1) { }
        public Bounds(Size size) : this(-1, -1, size.Width, size.Height) { }

        public bool Intersects(Bounds other)
        {
            return Left < other.Right &&
                   Right > other.Left &&
                   Top < other.Bottom &&
                   Bottom > other.Top;
        }

        public bool Intersects(Position other)
        {
            return other.X >= Left &&
                   other.X < Right &&
                   other.Y >= Top &&
                   other.Y < Bottom;
        }
    }

}
