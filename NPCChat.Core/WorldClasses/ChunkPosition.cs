using System;

namespace NPCChat.Core.WorldClasses
{
    public readonly struct ChunkPosition : IEquatable<ChunkPosition>
    {
        public int X { get; }
        public int Y { get; }

        public ChunkPosition(int x, int y) { X = x; Y = y; }

        public bool Equals(ChunkPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is ChunkPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(ChunkPosition left, ChunkPosition right) => left.Equals(right);
        public static bool operator !=(ChunkPosition left, ChunkPosition right) => !left.Equals(right);
        public override string ToString() => $"({X}, {Y})";
    }
}
