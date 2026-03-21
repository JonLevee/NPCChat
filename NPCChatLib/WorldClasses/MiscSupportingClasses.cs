using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.WorldClasses
{
    public readonly record struct WorldCoord(int X, int Y);
    public readonly record struct ChunkCoord(int X, int Y);
    public readonly record struct Bounds(int X, int Y, int Width, int Height)
    {
        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;
    }
}
