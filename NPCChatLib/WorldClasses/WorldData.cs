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
        WorldDataOptions Options,
        WorldChunkIndex WorldIndex)
    {
        public Dictionary<int, WorldObject> Objects { get; } = [];

        public void Add(WorldObject o)
        {
            var errors = new StringBuilder();
            var conflicts = new List<(string, WorldObject)>();
            var relatedChunks = Options.GetRelatedChunkPositions(o.Bounds).ToArray();
            foreach (var relatedChunk in relatedChunks)
            {
                var chunkData = WorldIndex.GetOrAdd(relatedChunk);
                foreach (var chunk in chunkData.GetChunkInfos(o))
                {
                }
            }


            var boundInfos = new List<BoundInfos>();
            try
            {
                if (!Objects.TryAdd(o.Id, o))
                    conflicts.Add(("dup Id in Objects", o));
                var relatedChunks = Options.GetRelatedChunkPositions(o.Bounds).ToArray();
                var primary = GetPrimaryIndex(o);
                for (int i = 0; i < relatedChunks.Length; i++)
                {
                    var chunkPosition = relatedChunks[i];
                    foreach (var index in Indexes)
                    {
                        BoundInfos infos;
                        if (index == primary)
                        {
                            infos = index.GetOrAdd(chunkPosition);
                            foreach (var info in infos)
                            {
                                if (info.Id == o.Id)
                                    conflicts.Add(($"dup Id in {chunkPosition}", o));
                                else if (info.Bounds.Intersects(o.Bounds))
                                    conflicts.Add(($"position conflict", Objects[info.Id]));
                            }

                            continue;
                        }
                        if (index.TryGet(chunkPosition, index == primary, out infos))
                        {
                            foreach (var info in infos)
                            {
                                if (info.Bounds.Intersects(o.Bounds))
                                    conflicts.Add(Objects[info.Id]);
                            }

                        }
                        infos.Where(info => info.Bounds.Intersects(o.Bounds)).ForEach(info => conflicts.Add(Objects[info.Id]));
                        if (index == primary)
                            boundInfos.Add(infos);
                    }
                }
                if (conflicts.Any())
                {
                    throw new WorldGenerationException(o, conflicts);
                }

            }
            catch (Exception e)
            {
                var errorText = new StringBuilder($"new object {o.GetDescription()} conflicts with:\r\n");
                errorText.AppendLine("  " + e.Message);
                throw;
            }

            if (Objects.TryGetValue(o.Id, out WorldObject conflict))
            {
                errorText.AppendLine($"  [conflicting id]: {conflict.GetDescription()}");
            }
            else
            {
                var relatedChunks = Options.GetRelatedChunkPositions(o.Bounds).ToArray();
                //var chunkPosition = Options.ToChunk(o.Bounds);
                var index = GetNonPrimaryIndex(o);
                CheckForConflicts(index);
                if (index.TryGetValue(o, out BoundInfos infos))
                {
                    foreach (var info in infos)
                    {
                        if (o.Bounds.Intersects(info.Bounds))
                        {
                            errorText.AppendLine($"  [conflicting position]: {Objects[info.Id].GetDescription()}");
                        }
                    }
                }
                index = GetPrimaryIndex(o);
                var relatedChunks = Options.GetRelatedChunkPositions(o.Bounds).ToArray();
                foreach (var chunk in relatedChunks)
                {

                }
                if (index.TryGetValue(chunkPosition, out infos))
                {
                    foreach (var info in infos)
                    {
                        if (o.Bounds.Intersects(info.Bounds))
                        {
                            errorText.AppendLine($"  [conflicting position]: {Objects[info.Id].GetDescription()}");
                        }
                    }
                }
            }
            if (errorText.Length > 0)
            {
                errorText.Insert(0, $"new object {o.GetDescription()} conflicts with:\r\n");
                error = errorText.ToString();
            }
            return error != null;
        }

        private void CheckForConflicts(WorldChunkIndex index)
        {

        }





        public bool TryGetConflicts(ChunkPosition chunkPosition, WorldObject o, out string message)
        {
            message = null;
            var conflicts = new List<WorldObject>();
            foreach (var index in Indexes)
            {
                if (index.TryGetValue(chunkPosition, out BoundInfos infos))
                {
                    foreach (var info in infos)
                    {
                        if (o.Bounds.Intersects(info.Bounds))
                        {
                            if (message == null)
                                message = $"new object {o.GetDescription()} conflicts with:";
                            message += "\r\n  " + Objects[info.Id].GetDescription();
                        }
                    }
                }
            }

            return message != null;
        }
    }
}
