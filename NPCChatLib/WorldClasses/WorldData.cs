using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPCChatLib.Attributes;
using NPCChatLib.Exceptions;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{

    [Scoped]
    public class WorldData(
        WorldDataOptions options,
        WorldChunkIndex worldIndex)
    {
        public WorldDataOptions Options { get; } = options;
        public WorldChunkIndex WorldIndex { get; } = worldIndex;
        public readonly Dictionary<ChunkPosition, ChunkData> ChunkData = [];

        public Dictionary<int, WorldObject> Objects { get; } = [];

        public void Add(WorldObject o)
        {
            WorldIndex.thr
            var conflicts = new List<(string, WorldObject)>();
            var relatedChunks = Options.GetRelatedChunkPositions(o.Bounds).ToArray();
            List<Action> addActions = [];
            foreach (var relatedChunk in relatedChunks)
            {
                var chunkData = WorldIndex.GetOrAdd(relatedChunk);
                foreach ((var isPrimary, var name, var info) in chunkData.GetChunkInfos(o))
                {
                    if (info.Id == o.Id)
                        conflicts.Add(($"dup Id in {name}{relatedChunk}", o));
                    else if (info.Bounds.Intersects(o.Bounds))
                        conflicts.Add(($"position conflict in {name}{relatedChunk}", Objects[info.Id]));
                    else if (isPrimary)
                    {
                        addActions.Add(() => chunkData.Add(o));
                    }
                }
            }
            if (conflicts.Any())
            {
                addActions.Clear();
                throw new WorldGenerationException(o, conflicts);
            }
            Objects.Add(o.Id, o);
            addActions.ForEach(action => action());
        }
    }

    public class WorldObjectVerification(WorldData worldData)
    {
        private readonly WorldDataOptions options = worldData.Options;
        public void ThrowIfAnyConflicts(WorldObject o)
        {
            var conflicts = GetConflicts(o).ToList();
            if (conflicts.Any())
            {
                conflicts.Insert(0, $"WorldObject {o.Id} {o.Bounds} has conflicts:");
                throw new WorldGenerationException(string.Join("\r\n", conflicts);
            }
        }

        public IEnumerable<string> GetConflicts(WorldObject o)
        {
            var primary = GetPrimaryChunkInfoList(o);
            var relatedChunks = options.GetRelatedChunkPositions(o.Bounds).ToArray();
            foreach (var relatedChunk in relatedChunks)
            {
                foreach (var conflict in GetConflicts(o, "Secondary", GetSecondaryChunkInfoList(o)))
                    yield return conflict;
            }
        }

        private IEnumerable<string> GetConflicts(WorldObject o, string name, List<ChunkInfo> items)
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
}
