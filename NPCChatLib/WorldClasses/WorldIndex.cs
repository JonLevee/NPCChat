using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{
    [DebuggerDisplay("Name=[{Name}]")]
    public abstract partial class WorldIndex(WorldDataOptions Options, string Name) : Dictionary<Position, BoundInfos>
    {
        public Position ToChunkCoord(Position position)
        {
            return new Position(
                (int)Math.Floor((double)(position.X / Options.ChunkSize)),
                (int)Math.Floor((double)(position.Y / Options.ChunkSize))
                );
        }

        public bool TryGetOverlap(Position position, out WorldObject conflicting)
        {
            var chunkPosition = ToChunkCoord(position);
            if (TryGetValue(chunkPosition, out BoundInfos bounds))
            {
                if (bounds.Any(b => b.Bounds.))
            }
        }
    }

    [Scoped]
    public class StaticWorldIndex(WorldDataOptions options) : WorldIndex(options, "Static")
    {
    }

    [Scoped]
    public class DynamicWorldIndex(WorldDataOptions options) : WorldIndex(options, "Dynamic")
    {
    }
}
