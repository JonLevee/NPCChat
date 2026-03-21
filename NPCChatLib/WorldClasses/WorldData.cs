using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{

    [Scoped]
    public class WorldData(IWorldDataOptions options)
    {
        public IWorldDataOptions Options { get; } = options;
        public Dictionary<ChunkCoord, List<int>> StaticIndex { get; } = [];
        public Dictionary<ChunkCoord, List<int>> DynamicIndex { get; } = [];
        public Dictionary<int, WorldObject> Objects { get; } = [];

    }

}
