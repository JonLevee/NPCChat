using System;
using System.Collections.Generic;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;

// TODO: add background thread for non-UI updates

namespace NPCChatLib.WorldClasses
{
    [Scoped]
    public sealed class WorldData : IDisposable
    {
        private readonly Dictionary<ChunkPosition, ChunkData> _chunks = [];
        private readonly IWorldOptions options;
        private readonly ObjectHandleManager handleManager;
        private Task _simulationProcessingTask = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;
        private CancellationToken _cancellationToken;

        public WorldData(IWorldOptions options, ObjectHandleManager handleManager)
        {
            this.options = options;
            this.handleManager = handleManager;
            StartSimulationProcessing();
        }

        public IReadOnlyDictionary<ChunkPosition, ChunkData> Chunks => _chunks;

        public void StartSimulationProcessing()
        {
            if (_simulationProcessingTask != null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
                _simulationProcessingTask = Task.Factory.StartNew(SimulationProcessing, _cancellationToken);
            }
        }

        public void StopSimulationProcessing()
        {
            if (_simulationProcessingTask != null)
            {
                _cancellationTokenSource.Cancel();
                _simulationProcessingTask.Wait();
                _simulationProcessingTask.Wait();
                _simulationProcessingTask.Dispose();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null!;
                _simulationProcessingTask = null!;
            }
        }

        private void SimulationProcessing()
        {
            throw new NotImplementedException();
        }

        public ObjectHandle AddObject(WorldObject obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            if (!obj.Handle.IsDefault)
                throw new InvalidOperationException("Object already has a handle assigned.");

            EnsureNoIntersection(obj.Bounds, ignoreHandle: default);

            var handle = handleManager.GetNewHandle(obj);
            obj.Handle = handle;
            AddObjectToChunks(obj);

            return handle;
        }

        public void RemoveObject(ObjectHandle handle)
        {
            var obj = GetRequiredObject(handle);

            RemoveObjectFromChunks(obj);

            handleManager.RemoveSlot(handle);

        }

        public bool TryGetObject(ObjectHandle handle, out WorldObject obj)
        {
            obj = null;
            if (handleManager.TryGetSlot(handle, out ObjectSlot slot))
                obj = slot.Object;
            return obj is not null;
        }

        public void MoveDynamicObjectBetweenChunks(ObjectHandle handle, Bounds newBounds)
        {
            var obj = GetRequiredObject(handle);

            if (obj.Category == WorldObjectCategory.Static)
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
            if (obj.Category == WorldObjectCategory.Static)
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
            if (obj.Category == WorldObjectCategory.Static)
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
            int minChunkX = FloorDiv(bounds.Left, options.ChunkInfo.ChunkSize);
            int maxChunkX = FloorDiv(bounds.Right - 1, options.ChunkInfo.ChunkSize);
            int minChunkY = FloorDiv(bounds.Top, options.ChunkInfo.ChunkSize);
            int maxChunkY = FloorDiv(bounds.Bottom - 1, options.ChunkInfo.ChunkSize);

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

        public void Clear()
        {
            var handles = EnumerateWorldObjects().Select(x => x.Handle).ToArray();

            foreach (var handle in handles)
            {
                RemoveObject(handle);
            }

        }

        public IReadOnlyList<WorldObject> EnumerateWorldObjects()
        {
            var results = new List<WorldObject>();
            var seen = new HashSet<ObjectHandle>();

            foreach (var chunk in Chunks.Values)
            {
                foreach (var item in chunk.StaticInfos)
                {
                    if (seen.Add(item.Handle) && TryGetObject(item.Handle, out var obj) && obj is not null)
                    {
                        results.Add(obj);
                    }
                }

                foreach (var item in chunk.DynamicInfos)
                {
                    if (seen.Add(item.Handle) && TryGetObject(item.Handle, out var obj) && obj is not null)
                    {
                        results.Add(obj);
                    }
                }
            }

            return results
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Kind)
                .ThenBy(x => x.Bounds.Top)
                .ThenBy(x => x.Bounds.Left)
                .ToArray();
        }

        public void Dispose()
        {
            StopSimulationProcessing();
        }
    }
}