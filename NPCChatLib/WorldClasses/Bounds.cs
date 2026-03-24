using System.Diagnostics;
using System.Drawing;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{
    public readonly record struct Bounds
    {
        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }
        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public Bounds(int X, int Y, int Width, int Height)
        {
            Left = X;
            Top = Y;
            Right = X + Width;
            Bottom = Y + Height;
        }

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
