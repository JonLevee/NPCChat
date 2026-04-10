using System.Collections.Generic;


namespace NPCChat.Core.WorldClasses
{
    public interface IObjectHandle
    {
        ObjectHandle Handle { get; }
    }

    public readonly record struct StaticChunkInfo(ObjectHandle Handle, Bounds Bounds) : IObjectHandle;

    public readonly record struct DynamicChunkInfo(ObjectHandle Handle) : IObjectHandle;

    public sealed class ChunkData
    {
        public List<StaticChunkInfo> StaticInfos { get; } = new();
        public List<DynamicChunkInfo> DynamicInfos { get; } = new();

        public bool IsEmpty => StaticInfos.Count == 0 && DynamicInfos.Count == 0;
    }
}