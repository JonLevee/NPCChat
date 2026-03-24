using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPCChatLib.Attributes;
using NPCChatLib.Exceptions;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{

    [Scoped]
    public class WorldData(
        WorldDataOptions Options,
        StaticWorldChunkIndex StaticChunkIndex,
        DynamicWorldChunkIndex DynamicChunkIndex)
    {
        public Dictionary<int, WorldObject> Objects { get; } = [];

        public IEnumerable<WorldChunkIndex> Indexes => new WorldChunkIndex[] { StaticChunkIndex, DynamicChunkIndex };
        public WorldChunkIndex GetPrimaryIndex(WorldObject o) => o.IsStatic() ? StaticChunkIndex : DynamicChunkIndex;
        public WorldChunkIndex GetNonPrimaryIndex(WorldObject o) => o.IsDynamic() ? StaticChunkIndex : DynamicChunkIndex;

        public void Add(WorldObject o, int x, int y)
        {
            o.Bounds = new(X: x, Y: y, Width: o.Bounds.Width, Height: o.Bounds.Height);
            if (o.Id == -1)
                o.Id = Options.GetNextId();
            var nonPrimaryIndex = GetNonPrimaryIndex(o);
            if (nonPrimaryIndex.TryGet(o.Bounds, out List<WorldObject> conflicts))
            {
                throw new WorldGenerationException();
            }
            var primary = GetPrimaryIndex(o);

        }
    }
}
