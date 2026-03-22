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
    public class WorldData(
        WorldDataOptions Options,
        StaticWorldIndex StaticIndex,
        DynamicWorldIndex DynamicIndex)
    {
        public Dictionary<int, WorldObject> Objects { get; } = [];

        public IEnumerable<WorldIndex> Indexes => [StaticIndex, DynamicIndex];
        public WorldIndex GetIndex(WorldObject o) => o.IsStatic() ? StaticIndex : DynamicIndex;

        public void Add(WorldObject o, int x, int y)
        {
            o.Position = new Position(x, y);
            if (o.Id == -1)
                o.Id = Options.GetNextId();
            if (Indexes.Any(index => index.TryGetOverlap(o.Position)))
            {

            }

            var list = this.GetBounds(o);

        }
    }
}
