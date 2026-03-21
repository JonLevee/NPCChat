using System.Diagnostics;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{
    public interface IWorldDataOptions
    {
        int ChunkSize { get; }
    }

    [DebuggerDisplay("ChunkSize:{ChunkSize}")]
    [Scoped(serviceType: typeof(IWorldDataOptions))]
    public class WorldDataOptions(int chunkSize = 16) : IWorldDataOptions
    {
        public int ChunkSize { get; set; } = chunkSize;
    }
}
