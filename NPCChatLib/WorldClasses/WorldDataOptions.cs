using System;
using System.Collections.Generic;
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
        public IEnumerable<ChunkPosition> GetRelatedChunkPositions(Bounds bounds)
        {
            var chunk = ToChunk(bounds);
            yield return chunk;
            if (bounds.Right > chunk.X * ChunkSize)
                yield return new ChunkPosition(chunk.X + 1, chunk.Y);
            if (bounds.Bottom > chunk.Y * ChunkSize)
                yield return new ChunkPosition(chunk.X, chunk.Y + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToChunkValue(int value) => (int)Math.Floor((double)(value / ChunkSize));

    }
}
