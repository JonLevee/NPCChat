using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{
    [Scoped]
    public class WorldDataOptions(int chunkSize = 16)
    {
        private int nextId = 0;
        public int ChunkSize { get; set; } = chunkSize;

        public void SetNextId(int id) => nextId = id;
        public int GetNextId() { return Interlocked.Increment(ref nextId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkPosition ToChunk(Bounds bounds) => new(ToChunkValue(bounds.Left), ToChunkValue(bounds.Top));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkPosition ToChunk(Position position) => new(ToChunkValue(position.X), ToChunkValue(position.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkPosition ToChunk(int x, int y) => new(ToChunkValue(x), ToChunkValue(y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToChunkValue(int value) => (int)Math.Floor((double)(value / ChunkSize));

    }
}
