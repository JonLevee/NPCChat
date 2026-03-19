using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.WorldClasses
{
    public record WorldOptions(int ChunkSize);
    public readonly record struct ChunkCoord(int X, int Y);
    public readonly record struct Int2(int X, int Y);
    public readonly record struct IntRect(int X, int Y, int Width, int Height)
    {
        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;
    }

    public class WorldData(WorldOptions options)
    {
        public WorldOptions Options { get; private set; } = options;

        public Dictionary<ChunkCoord, List<int>> StaticIndex { get; } = [];
        public Dictionary<ChunkCoord, List<int>> DynamicIndex { get; } = [];
        public Dictionary<int, WorldObject> Objects { get; } = [];

    }

}
