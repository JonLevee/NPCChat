using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{
    public readonly record struct ChunkPosition(int X, int Y);
    public readonly record struct BoundInfo(int Id, Bounds Bounds);
    public record struct ChunkInfo(int Id, Bounds Bounds);
    public record struct ChunkData(ChunkPosition ChunkPosition)
    {
        public List<ChunkInfo> StaticChunkInfos = [];
        public List<ChunkInfo> DynamicChunkInfos = [];

        foobar
        public readonly IEnumerable<(bool isPrimary, ChunkInfo)> GetChunkInfos(WorldObject o)
        {
            List<ChunkInfo>[] lists = o.IsStatic() ? [StaticChunkInfos, DynamicChunkInfos] : [DynamicChunkInfos, StaticChunkInfos];
            foreach (var list in lists)
            {
                foreach (var info in list)
                    yield return (list == lists[0], info);
            }
        }
    }

    [Scoped]
    public class WorldChunkIndex
    {
        private readonly Dictionary<ChunkPosition, ChunkData> ChunkData = [];

        public ChunkData GetOrAdd(ChunkPosition chunkPosition)
        {
            if (!ChunkData.TryGetValue(chunkPosition, out ChunkData chunkData))
            {
                chunkData = ChunkData[chunkPosition] = new(chunkPosition);
            }
            return chunkData;
        }
    }
}
