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

        public (ChunkPosition chunkPos, WorldChunkIndex primary, WorldChunkIndex nonPrimary) GetChunkAndIndexes(WorldObject o)
        {
            var chunkPosition = Options.ToChunk(o.Bounds);
            return o.IsStatic()
                ? (chunkPosition, StaticChunkIndex, DynamicChunkIndex)
                : (chunkPosition, DynamicChunkIndex, StaticChunkIndex);
        }
    }
}
