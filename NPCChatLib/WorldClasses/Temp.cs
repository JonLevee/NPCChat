using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;


namespace GameWorld
{
    public readonly record struct ChunkPosition(int X, int Y);

    public readonly record struct Bounds
    {
        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public Bounds(int left, int top, int right, int bottom)
        {
            if (right <= left)
                throw new ArgumentOutOfRangeException(nameof(right), "Right must be greater than Left.");

            if (bottom <= top)
                throw new ArgumentOutOfRangeException(nameof(bottom), "Bottom must be greater than Top.");

            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static Bounds FromPositionSize(int x, int y, int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be > 0.");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be > 0.");

            return new Bounds(x, y, x + width, y + height);
        }

        public bool Intersects(in Bounds other)
        {
            return Left < other.Right &&
                   Right > other.Left &&
                   Top < other.Bottom &&
                   Bottom > other.Top;
        }

        public override string ToString()
            => $"[{Left},{Top}]..[{Right},{Bottom}]";
    }

    /// <summary>
    /// 32-bit packed handle:
    /// low 24 bits = slot id
    /// high 8 bits = generation
    /// </summary>
    public readonly struct ObjectHandle : IEquatable<ObjectHandle>
    {
        private const uint IdMask = 0x00FFFFFF;
        private readonly uint _value;

        public int Id => (int)(_value & IdMask);
        public byte Generation => (byte)(_value >> 24);

        public bool IsDefault => _value == 0;

        public ObjectHandle(int id, byte generation)
        {
            if ((uint)id > IdMask)
                throw new ArgumentOutOfRangeException(nameof(id), "Id must fit in 24 bits.");

            _value = ((uint)generation << 24) | (uint)id;
        }

        private ObjectHandle(uint rawValue)
        {
            _value = rawValue;
        }

        public static implicit operator uint(ObjectHandle handle) => handle._value;
        public static implicit operator ObjectHandle(uint rawValue) => new ObjectHandle(rawValue);

        public bool Equals(ObjectHandle other) => _value == other._value;
        public override bool Equals(object? obj) => obj is ObjectHandle other && Equals(other);
        public override int GetHashCode() => (int)_value;

        public static bool operator ==(ObjectHandle left, ObjectHandle right) => left._value == right._value;
        public static bool operator !=(ObjectHandle left, ObjectHandle right) => left._value != right._value;

        public override string ToString() => $"Handle(Id={Id}, Gen={Generation})";
    }

    public readonly record struct StaticChunkInfo(ObjectHandle Handle, Bounds Bounds);
    public readonly record struct DynamicChunkInfo(ObjectHandle Handle);

    public sealed class ChunkData
    {
        public List<StaticChunkInfo> StaticInfos { get; } = new();
        public List<DynamicChunkInfo> DynamicInfos { get; } = new();

        public bool IsEmpty => StaticInfos.Count == 0 && DynamicInfos.Count == 0;
    }

    public abstract class WorldObject
    {
        public ObjectHandle Handle { get; internal set; }
        public Bounds Bounds { get; internal set; }
        public abstract bool IsStatic { get; }

        protected WorldObject(Bounds bounds)
        {
            Bounds = bounds;
        }
    }

    public sealed class StaticWorldObject : WorldObject
    {
        public override bool IsStatic => true;

        public StaticWorldObject(Bounds bounds) : base(bounds)
        {
        }
    }

    public sealed class DynamicWorldObject : WorldObject
    {
        public override bool IsStatic => false;

        public DynamicWorldObject(Bounds bounds) : base(bounds)
        {
        }
    }

    internal struct ObjectSlot
    {
        public WorldObject? Object;
        public byte Generation;
        public bool IsOccupied;
    }

    public sealed class WorldData
    {
        private readonly int _chunkSize;
        private readonly Dictionary<ChunkPosition, ChunkData> _chunks = new();

        // slot-id indexed storage
        private readonly List<ObjectSlot> _slots = new();

        // free slot ids available for reuse
        private readonly Stack<int> _freeSlotIds = new();

        // packed list of currently active handles, if you later want iteration
        private readonly HashSet<ObjectHandle> _activeHandles = new();

        public IReadOnlyDictionary<ChunkPosition, ChunkData> Chunks => _chunks;

        public WorldData(int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be > 0.");

            _chunkSize = chunkSize;
        }

        public ObjectHandle AddObject(WorldObject obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            if (!obj.Handle.IsDefault)
                throw new InvalidOperationException("Object already has a handle assigned.");

            EnsureNoIntersection(obj.Bounds, ignoreHandle: default);

            int slotId;
            byte generation;

            if (_freeSlotIds.Count > 0)
            {
                slotId = _freeSlotIds.Pop();
                var slot = _slots[slotId];

                if (slot.IsOccupied)
                    throw new InvalidOperationException($"Internal error: free slot {slotId} is still occupied.");

                generation = slot.Generation;
            }
            else
            {
                slotId = _slots.Count;
                if ((uint)slotId > 0x00FFFFFF)
                    throw new InvalidOperationException("Maximum object count exceeded for 24-bit handle ids.");

                generation = 1; // start at 1 so default(0) is never a valid active handle
                _slots.Add(default);
            }

            var handle = new ObjectHandle(slotId, generation);

            if (_activeHandles.Contains(handle))
                throw new InvalidOperationException($"Duplicate active handle detected: {handle}.");

            obj.Handle = handle;

            _slots[slotId] = new ObjectSlot
            {
                Object = obj,
                Generation = generation,
                IsOccupied = true
            };

            AddObjectToChunks(obj);
            _activeHandles.Add(handle);

            return handle;
        }

        public void RemoveObject(ObjectHandle handle)
        {
            var obj = GetRequiredObject(handle);

            RemoveObjectFromChunks(obj);

            int slotId = handle.Id;
            var slot = _slots[slotId];

            if (!slot.IsOccupied)
                throw new InvalidOperationException($"Handle {handle} refers to an unoccupied slot.");

            if (slot.Generation != handle.Generation)
                throw new InvalidOperationException($"Handle {handle} is stale.");

            slot.Object = null;
            slot.IsOccupied = false;
            slot.Generation = unchecked((byte)(slot.Generation + 1));

            _slots[slotId] = slot;

            if (!_activeHandles.Remove(handle))
                throw new InvalidOperationException($"Internal error: active handle set did not contain {handle}.");
        }

        public bool TryGetObject(ObjectHandle handle, out WorldObject? obj)
        {
            obj = null;

            int slotId = handle.Id;
            if (slotId < 0 || slotId >= _slots.Count)
                return false;

            var slot = _slots[slotId];
            if (!slot.IsOccupied)
                return false;

            if (slot.Generation != handle.Generation)
                return false;

            obj = slot.Object;
            return obj is not null;
        }

        public void MoveDynamicObjectBetweenChunks(ObjectHandle handle, Bounds newBounds)
        {
            var obj = GetRequiredObject(handle);

            if (obj.IsStatic)
                throw new InvalidOperationException($"Object {handle} is static and cannot be moved with {nameof(MoveDynamicObjectBetweenChunks)}.");

            var oldBounds = obj.Bounds;

            if (oldBounds == newBounds)
                return;

            EnsureNoIntersection(newBounds, ignoreHandle: handle);

            RemoveDynamicObjectFromChunks(handle, oldBounds);

            try
            {
                obj.Bounds = newBounds;
                AddDynamicObjectToChunks(handle, newBounds);
            }
            catch
            {
                obj.Bounds = oldBounds;
                AddDynamicObjectToChunks(handle, oldBounds);
                throw;
            }
        }

        private WorldObject GetRequiredObject(ObjectHandle handle)
        {
            if (!TryGetObject(handle, out var obj) || obj is null)
                throw new KeyNotFoundException($"No active object found for handle {handle}.");

            return obj;
        }

        private void EnsureNoIntersection(Bounds bounds, ObjectHandle ignoreHandle)
        {
            var candidateChunks = EnumerateTouchedChunks(bounds);
            var seen = new HashSet<ObjectHandle>();

            foreach (var chunkPos in candidateChunks)
            {
                if (!_chunks.TryGetValue(chunkPos, out var chunk))
                    continue;

                foreach (var staticInfo in chunk.StaticInfos)
                {
                    if (staticInfo.Handle == ignoreHandle)
                        continue;

                    if (!seen.Add(staticInfo.Handle))
                        continue;

                    if (staticInfo.Bounds.Intersects(bounds))
                        throw new InvalidOperationException(
                            $"Bounds {bounds} intersect existing static object {staticInfo.Handle} with bounds {staticInfo.Bounds}.");
                }

                foreach (var dynamicInfo in chunk.DynamicInfos)
                {
                    if (dynamicInfo.Handle == ignoreHandle)
                        continue;

                    if (!seen.Add(dynamicInfo.Handle))
                        continue;

                    var other = GetRequiredObject(dynamicInfo.Handle);

                    if (other.Bounds.Intersects(bounds))
                    {
                        throw new InvalidOperationException(
                            $"Bounds {bounds} intersect existing dynamic object {dynamicInfo.Handle} with bounds {other.Bounds}.");
                    }
                }
            }
        }

        private void AddObjectToChunks(WorldObject obj)
        {
            if (obj.IsStatic)
            {
                AddStaticObjectToChunks(obj.Handle, obj.Bounds);
            }
            else
            {
                AddDynamicObjectToChunks(obj.Handle, obj.Bounds);
            }
        }

        private void RemoveObjectFromChunks(WorldObject obj)
        {
            if (obj.IsStatic)
            {
                RemoveStaticObjectFromChunks(obj.Handle, obj.Bounds);
            }
            else
            {
                RemoveDynamicObjectFromChunks(obj.Handle, obj.Bounds);
            }
        }

        private void AddStaticObjectToChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                var chunk = GetOrCreateChunk(chunkPos);

                // prevent duplicate handle insertion into same chunk
                foreach (var existing in chunk.StaticInfos)
                {
                    if (existing.Handle == handle)
                        throw new InvalidOperationException($"Static handle {handle} already exists in chunk {chunkPos}.");
                }

                chunk.StaticInfos.Add(new StaticChunkInfo(handle, bounds));
            }
        }

        private void AddDynamicObjectToChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                var chunk = GetOrCreateChunk(chunkPos);

                foreach (var existing in chunk.DynamicInfos)
                {
                    if (existing.Handle == handle)
                        throw new InvalidOperationException($"Dynamic handle {handle} already exists in chunk {chunkPos}.");
                }

                chunk.DynamicInfos.Add(new DynamicChunkInfo(handle));
            }
        }

        private void RemoveStaticObjectFromChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                if (!_chunks.TryGetValue(chunkPos, out var chunk))
                    throw new InvalidOperationException($"Expected static object {handle} chunk {chunkPos} to exist, but it did not.");

                bool removed = RemoveStaticHandleFromChunk(chunk, handle);

                if (!removed)
                    throw new InvalidOperationException($"Expected static object {handle} in chunk {chunkPos}, but it was not found.");

                CleanupChunkIfEmpty(chunkPos, chunk);
            }
        }

        private void RemoveDynamicObjectFromChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                if (!_chunks.TryGetValue(chunkPos, out var chunk))
                    throw new InvalidOperationException($"Expected dynamic object {handle} chunk {chunkPos} to exist, but it did not.");

                bool removed = RemoveDynamicHandleFromChunk(chunk, handle);

                if (!removed)
                    throw new InvalidOperationException($"Expected dynamic object {handle} in chunk {chunkPos}, but it was not found.");

                CleanupChunkIfEmpty(chunkPos, chunk);
            }
        }

        private static bool RemoveStaticHandleFromChunk(ChunkData chunk, ObjectHandle handle)
        {
            for (int i = 0; i < chunk.StaticInfos.Count; i++)
            {
                if (chunk.StaticInfos[i].Handle == handle)
                {
                    int last = chunk.StaticInfos.Count - 1;
                    chunk.StaticInfos[i] = chunk.StaticInfos[last];
                    chunk.StaticInfos.RemoveAt(last);
                    return true;
                }
            }

            return false;
        }

        private static bool RemoveDynamicHandleFromChunk(ChunkData chunk, ObjectHandle handle)
        {
            for (int i = 0; i < chunk.DynamicInfos.Count; i++)
            {
                if (chunk.DynamicInfos[i].Handle == handle)
                {
                    int last = chunk.DynamicInfos.Count - 1;
                    chunk.DynamicInfos[i] = chunk.DynamicInfos[last];
                    chunk.DynamicInfos.RemoveAt(last);
                    return true;
                }
            }

            return false;
        }

        private ChunkData GetOrCreateChunk(ChunkPosition chunkPos)
        {
            if (_chunks.TryGetValue(chunkPos, out var chunk))
                return chunk;

            chunk = new ChunkData();
            _chunks.Add(chunkPos, chunk);
            return chunk;
        }

        private void CleanupChunkIfEmpty(ChunkPosition chunkPos, ChunkData chunk)
        {
            if (chunk.IsEmpty)
            {
                _chunks.Remove(chunkPos);
            }
        }

        private IEnumerable<ChunkPosition> EnumerateTouchedChunks(Bounds bounds)
        {
            int minChunkX = FloorDiv(bounds.Left, _chunkSize);
            int maxChunkX = FloorDiv(bounds.Right - 1, _chunkSize);
            int minChunkY = FloorDiv(bounds.Top, _chunkSize);
            int maxChunkY = FloorDiv(bounds.Bottom - 1, _chunkSize);

            for (int y = minChunkY; y <= maxChunkY; y++)
            {
                for (int x = minChunkX; x <= maxChunkX; x++)
                {
                    yield return new ChunkPosition(x, y);
                }
            }
        }

        private static int FloorDiv(int value, int divisor)
        {
            int quotient = value / divisor;
            int remainder = value % divisor;

            if (remainder != 0 && ((remainder < 0) != (divisor < 0)))
                quotient--;

            return quotient;
        }
    }
}