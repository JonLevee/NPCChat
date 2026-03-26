using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NPCChatLib.Attributes;
using NPCChatLib.Exceptions;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{
    public readonly record struct ChunkPosition(int X, int Y);
    public record struct ChunkInfo(int Id, Bounds Bounds);
    public readonly record struct ChunkInfos()
    {
        public List<ChunkInfo> Chunks { get; } = [];
    }
    public record struct ChunkData(
        ChunkPosition ChunkPosition,
        WorldDataOptions options)
    {
        public ChunkInfos StaticChunkInfos = new();
        public ChunkInfos DynamicChunkInfos = new();

        private static IEnumerable<(string name, ChunkInfo info)> GetChunkInfos(string name, ChunkInfos items) => items.Chunks.Select(item => (name, item));

        public readonly IEnumerable<(string name, ChunkInfo info)> GetPrimaryChunkInfos(WorldObject o) => o.IsStatic()
            ? GetChunkInfos("Static", StaticChunkInfos)
            : GetChunkInfos("Dynamic", DynamicChunkInfos);

        public readonly IEnumerable<(string name, ChunkInfo info)> GetSecondaryChunkInfos(WorldObject o) => o.IsStatic()
            ? GetChunkInfos("Static", StaticChunkInfos)
            : GetChunkInfos("Dynamic", DynamicChunkInfos);

        public readonly void Add(WorldObject o)
        {
            ThrowIfAnyConflicts(o);
            var info = new ChunkInfo(o.Id, o.Bounds);
            if (o.IsStatic())
                StaticChunkInfos.Add(info);
            else
                DynamicChunkInfos.Add(info);
        }

        public readonly List<ChunkInfo> GetPrimaryChunkInfoList(WorldObject o) => o.IsStatic() ? StaticChunkInfos : DynamicChunkInfos;
        public readonly List<ChunkInfo> GetSecondaryChunkInfoList(WorldObject o) => o.IsStatic() ? DynamicChunkInfos : StaticChunkInfos;

        public readonly void ThrowIfAnyConflicts(WorldObject o)
        {
            var conflicts = GetConflicts(o).ToList();
            if (conflicts.Any())
            {
                conflicts.Insert(0, $"WorldObject {o.Id} {o.Bounds} has conflicts:");
                throw new WorldGenerationException(string.Join("\r\n", conflicts);
            }
        }

        public readonly IEnumerable<string> GetConflicts(WorldObject o)
        {
            var primary = GetPrimaryChunkInfoList(o);
            var relatedChunks = options.GetRelatedChunkPositions(o.Bounds).ToArray();
            foreach (var relatedChunk in relatedChunks)
            {
                foreach (var conflict in GetConflicts(o, "Secondary", GetSecondaryChunkInfoList(o)))
                    yield return conflict;
            }
        }

        private readonly IEnumerable<string> GetConflicts(WorldObject o, string name, List<ChunkInfo> items)
        {
            var itemsName = items == StaticChunkInfos ? "Static" : "Dynamic";
            string formatConflict(string kind, ChunkInfo conflict) => $"{kind} conflict in {itemsName}({name}) Id:{conflict.Id} {conflict.Bounds}";
            foreach (var info in items)
            {
                if (info.Id == o.Id)
                    yield return formatConflict("Id", info);
                if (info.Bounds.Intersects(o.Bounds))
                    yield return formatConflict("position", info);

            }
        }
    }


    [Scoped]
    public class WorldChunkIndex(WorldDataOptions options)
    {
        public readonly Dictionary<ChunkPosition, ChunkData> ChunkData = [];
        public int StaticCount => ChunkData.Values.Sum(v => v.StaticChunkInfos.Count);
        public int DynamicCount => ChunkData.Values.Sum(v => v.DynamicChunkInfos.Count);
        public int Count => ChunkData.Values.Sum(v => v.DynamicChunkInfos.Count + v.StaticChunkInfos.Count);

        public ChunkData GetOrAdd(ChunkPosition chunkPosition)
        {
            if (!ChunkData.TryGetValue(chunkPosition, out ChunkData chunkData))
            {
                chunkData = ChunkData[chunkPosition] = new(chunkPosition, options);
            }
            return chunkData;
        }
    }
}
