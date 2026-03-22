using System.Diagnostics;
using System.Threading;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{
    [DebuggerDisplay("ChunkSize={ChunkSize}, nextId={nextId}")]
    [Scoped]
    public class WorldDataOptions(int nextId = 0, int chunkSize = 16)
    {
        private int nextId = nextId;
        public int ChunkSize { get; set; } = chunkSize;

        public int GetNextId() { return Interlocked.Increment(ref nextId); }

    }
}
