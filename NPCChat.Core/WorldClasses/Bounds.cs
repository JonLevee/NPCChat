using System;
using System.Drawing;


namespace NPCChatLib.WorldClasses
{
    public readonly record struct Bounds
    {
        public static readonly Bounds None = new();

        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public Bounds(int left, int top, int right, int bottom)
        {
            if (right <= left)
                throw new ArgumentOutOfRangeException(nameof(right), "Right must be greater than Left.");

            if (bottom <= top)
                throw new ArgumentOutOfRangeException(nameof(bottom), "Bottom must be greater than Top.");

            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Bounds(int x, int y, Size size) : this(x, y, x + size.Width, y + size.Height)
        {
        }

        //public static Bounds FromPositionSize(int x, int y, int width, int height)
        //{
        //    if (width <= 0)
        //        throw new ArgumentOutOfRangeException(nameof(width), "Width must be > 0.");

        //    if (height <= 0)
        //        throw new ArgumentOutOfRangeException(nameof(height), "Height must be > 0.");

        //    return new Bounds(x, y, x + width, y + height);
        //}

        public bool Intersects(in Bounds other)
        {
            return Left < other.Right &&
                   Right > other.Left &&
                   Top < other.Bottom &&
                   Bottom > other.Top;
        }

        public override string ToString()
            => $"[{Left},{Top}]..[{Right},{Bottom}]";
    }
}