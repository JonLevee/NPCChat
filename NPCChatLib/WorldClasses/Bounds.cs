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

        public Bounds(int x, int y, Size size)
        {
            Left = x;
            Top = y;
            Right = x + size.Width;
            Bottom = y + size.Height;
        }

        public bool Intersects(Bounds other)
        {
            return Left < other.Right &&
                   Right > other.Left &&
                   Top < other.Bottom &&
                   Bottom > other.Top;
        }
    }

}
