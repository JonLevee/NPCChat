using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.WorldClasses
{

    public class WorldData(WorldOptions options = null)
    {
        public WorldOptions Options { get; private set; } = options ?? new();

        public Dictionary<ChunkCoord, List<int>> StaticIndex { get; } = [];
        public Dictionary<ChunkCoord, List<int>> DynamicIndex { get; } = [];
        public Dictionary<int, WorldObject> Objects { get; } = [];

    }

}
