using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.WorldClasses
{
    public readonly record struct Position(int X, int Y);
    public readonly record struct Bounds
    {
        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }

        public Bounds(int X, int Y, int Width, int Height)
        {
            Left = X;
            Top = Y;
            Right = X + Width;
            Bottom = Y + Height;
        }
        public Bounds(Position position, Size size) : this(position.X, position.Y, size.Width, size.Height)
        {
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
    public readonly record struct BoundInfo(int Id, Bounds Bounds);

    public partial class BoundInfos : List<BoundInfo>
    {
        public bool TryGetOverlap(Position position, out int? id)
        {
            var overlap = this.FirstOrDefault(info => info.Bounds.Intersects(position));
            id = overlap.Id;
        }
    }

}
