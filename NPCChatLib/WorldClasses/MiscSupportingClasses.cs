using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.WorldClasses
{
    public readonly record struct Position(int X, int Y);
    public readonly record struct ChunkPosition(int X, int Y);
    public readonly record struct BoundInfo(int Id, Bounds Bounds);

    public partial class BoundInfos(ChunkPosition Position) : List<BoundInfo>
    {
    }

}
