using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NPCChatLib.WorldClasses
{

    [Scoped]
    public class WorldData(IWorldDataOptions options)
    {
        private int nextId = 0;

        public IWorldDataOptions Options { get; } = options;
        public Dictionary<ChunkCoord, List<int>> StaticIndex { get; } = [];
        public Dictionary<ChunkCoord, List<int>> DynamicIndex { get; } = [];
        public Dictionary<int, WorldObject> Objects { get; } = [];

        public void Add(WorldObject o, int x, int y)
        {
            o.Coord = new WorldCoord(x, y);
            if (o.Id == -1)
                o.Id = Interlocked.Increment(ref nextId);
            o.ChunkCoord = o.Coord.ToChunkCoord(Options);

            var list = GetIndex(o);

        }

        private List<int> GetIndex(WorldObject o)
        {
            var index = (o.IsStatic() ? StaticIndex : DynamicIndex);
            if (!index.TryGetValue(o.ChunkCoord, out List<int> list))
            {
                list = [];
                index[o.ChunkCoord] = list;
            }
            return list;
        }

    }

}
