using System;
using System.Collections.Generic;


namespace NPCChat.Core.WorldClasses
{
    public interface IObjectHandle
    {
        ObjectHandle Handle { get; }
    }

    public readonly struct StaticChunkInfo : IObjectHandle, IEquatable<StaticChunkInfo>
    {
        public ObjectHandle Handle { get; }
        public Bounds Bounds { get; }

        public StaticChunkInfo(ObjectHandle handle, Bounds bounds) { Handle = handle; Bounds = bounds; }

        public bool Equals(StaticChunkInfo other) => Handle.Equals(other.Handle) && Bounds.Equals(other.Bounds);
        public override bool Equals(object obj) => obj is StaticChunkInfo other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Handle, Bounds);
        public static bool operator ==(StaticChunkInfo left, StaticChunkInfo right) => left.Equals(right);
        public static bool operator !=(StaticChunkInfo left, StaticChunkInfo right) => !left.Equals(right);
    }

    public readonly struct DynamicChunkInfo : IObjectHandle, IEquatable<DynamicChunkInfo>
    {
        public ObjectHandle Handle { get; }

        public DynamicChunkInfo(ObjectHandle handle) { Handle = handle; }

        public bool Equals(DynamicChunkInfo other) => Handle.Equals(other.Handle);
        public override bool Equals(object obj) => obj is DynamicChunkInfo other && Equals(other);
        public override int GetHashCode() => Handle.GetHashCode();
        public static bool operator ==(DynamicChunkInfo left, DynamicChunkInfo right) => left.Equals(right);
        public static bool operator !=(DynamicChunkInfo left, DynamicChunkInfo right) => !left.Equals(right);
    }

    public sealed class ChunkData
    {
        public List<StaticChunkInfo> StaticInfos { get; } = new List<StaticChunkInfo>();
        public List<DynamicChunkInfo> DynamicInfos { get; } = new List<DynamicChunkInfo>();

        public bool IsEmpty => StaticInfos.Count == 0 && DynamicInfos.Count == 0;
    }
}
