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
        public bool TryGetConflicts(Bounds bounds, out List<WorldObject> conflicts)
        {
            throw new NotImplementedException();
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
