using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{
    [DebuggerDisplay("Name=[{Name}]")]
    public abstract partial class WorldChunkIndex(
        WorldDataOptions Options,
        string Name) : Dictionary<Position, BoundInfos>
    {
        public bool TryGet(Bounds bounds, out List<WorldObject> conflicts)
        {
            throw new NotImplementedException();
        }
        private ChunkPosition ToChunk(Position position)
        {
            return new ChunkPosition(
                (int)Math.Floor((double)(position.X / Options.ChunkSize)),
                (int)Math.Floor((double)(position.Y / Options.ChunkSize))
                );
        }

    }

    [Scoped]
    public class StaticWorldChunkIndex(WorldDataOptions options) : WorldChunkIndex(options, "Static")
    {
    }

    [Scoped]
    public class DynamicWorldChunkIndex(WorldDataOptions options) : WorldChunkIndex(options, "Dynamic")
    {
    }
}
