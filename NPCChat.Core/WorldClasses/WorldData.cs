using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using NPCChat.Core.Attributes;
using NPCChat.Core.Exceptions;
using NPCChat.Core.Extensions;

namespace NPCChat.Core.WorldClasses
{
    [Scoped]
    public sealed class WorldData : IDisposable
    {
        private readonly Dictionary<ChunkPosition, ChunkData> _chunks = [];
        private readonly List<WorldObjectMoveable> _moveableObjects = [];
        private readonly IWorldOptions _options;
        private readonly ObjectHandleManager _handleManager;

        // Threading
        private readonly ReaderWriterLockSlim _worldLock = new(LockRecursionPolicy.NoRecursion);
        private readonly Channel<MoveCommand> _moveCommands =
            Channel.CreateUnbounded<MoveCommand>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        private Task _simulationProcessingTask = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;

        /// <summary>
        /// Set by the simulation loop when it faults. The UI timer checks this
        /// each tick and rethrows on the UI thread so the error is visible.
        /// Null while the simulation is healthy.
        /// </summary>
        public Exception SimulationFault { get; private set; }

        public WorldData(IWorldOptions options, ObjectHandleManager handleManager)
        {
            _options = options;
            _handleManager = handleManager;
            StartSimulationProcessing();
        }

        /// <summary>Current number of occupied spatial chunks.</summary>
        public int ChunkCount
        {
            get
            {
                _worldLock.EnterReadLock();
                try { return _chunks.Count; }
                finally { _worldLock.ExitReadLock(); }
            }
        }

        // ── Simulation control ──────────────────────────────────────────────

        public void StartSimulationProcessing()
        {
            if (_simulationProcessingTask == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _simulationProcessingTask = Task.Factory.StartNew(
                    () => SimulationProcessingAsync(_cancellationTokenSource.Token),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                ).Unwrap();
            }
        }

        public void StopSimulationProcessing()
        {
            if (_simulationProcessingTask != null)
            {
                _cancellationTokenSource.Cancel();
                _simulationProcessingTask.Wait();
                _simulationProcessingTask.Dispose();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null!;
                _simulationProcessingTask = null!;
            }
        }

        private async Task SimulationProcessingAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (_moveCommands.Reader.Count > _options.MoveCommandQueueThreshold)
                        throw new SimulationOverflowException(
                            $"Move command queue exceeded threshold of {_options.MoveCommandQueueThreshold}. " +
                            $"Current count: {_moveCommands.Reader.Count}");

                    // Drain commands — Phase 2 will path and store them; discard for now.
                    while (_moveCommands.Reader.TryRead(out _)) { }

                    await Task.Delay(_options.SimTickMs, ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Normal shutdown — not a fault.
            }
            catch (Exception ex)
            {
                SimulationFault = ex;
            }
        }

        // ── Command queue ───────────────────────────────────────────────────

        /// <summary>
        /// Enqueues a move command from the UI thread. Never blocks.
        /// The simulation loop processes it on the next tick.
        /// </summary>
        public void EnqueueMoveCommand(MoveCommand cmd)
        {
            _moveCommands.Writer.TryWrite(cmd);
        }

        // ── Object management ───────────────────────────────────────────────

        public ObjectHandle AddObject(WorldObject obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            if (!obj.Handle.IsDefault)
                throw new InvalidOperationException("Object already has a handle assigned.");

            _worldLock.EnterWriteLock();
            try
            {
                EnsureNoIntersection(obj.Bounds, ignoreHandle: default);

                var handle = _handleManager.GetNewHandle(obj);
                obj.Handle = handle;
                AddObjectToChunks(obj);

                if (obj is WorldObjectMoveable moveable)
                    _moveableObjects.Add(moveable);

                return handle;
            }
            finally
            {
                _worldLock.ExitWriteLock();
            }
        }

        public void RemoveObject(ObjectHandle handle)
        {
            _worldLock.EnterWriteLock();
            try
            {
                var obj = GetRequiredObject(handle);
                RemoveObjectFromChunks(obj);

                if (obj is WorldObjectMoveable moveable)
                    _moveableObjects.Remove(moveable);

                _handleManager.RemoveSlot(handle);
            }
            finally
            {
                _worldLock.ExitWriteLock();
            }
        }

        public bool TryGetObject(ObjectHandle handle, out WorldObject obj)
        {
            obj = null;
            if (_handleManager.TryGetSlot(handle, out ObjectSlot slot))
                obj = slot.Object;
            return obj is not null;
        }

        public void MoveDynamicObjectBetweenChunks(ObjectHandle handle, Bounds newBounds)
        {
            _worldLock.EnterWriteLock();
            try
            {
                var obj = GetRequiredObject(handle);

                if (obj is not WorldObjectMoveable)
                    throw new InvalidOperationException(
                        $"Object {handle} is not moveable and cannot be moved with {nameof(MoveDynamicObjectBetweenChunks)}.");

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
            finally
            {
                _worldLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            // EnumerateWorldObjects acquires read lock, releases it, then
            // RemoveObject acquires write lock per call — no re-entrancy.
            var handles = EnumerateWorldObjects().Select(x => x.Handle).ToArray();
            foreach (var handle in handles)
                RemoveObject(handle);
        }

        public IReadOnlyList<WorldObject> EnumerateWorldObjects()
        {
            _worldLock.EnterReadLock();
            try
            {
                var results = new List<WorldObject>();
                var seen = new HashSet<ObjectHandle>();

                foreach (var chunk in _chunks.Values)
                {
                    foreach (var item in chunk.StaticInfos)
                    {
                        if (seen.Add(item.Handle) && TryGetObject(item.Handle, out var obj) && obj is not null)
                            results.Add(obj);
                    }

                    foreach (var item in chunk.DynamicInfos)
                    {
                        if (seen.Add(item.Handle) && TryGetObject(item.Handle, out var obj) && obj is not null)
                            results.Add(obj);
                    }
                }

                return results
                    .OrderBy(x => x.Category)
                    .ThenBy(x => x.Kind)
                    .ThenBy(x => x.Bounds.Top)
                    .ThenBy(x => x.Bounds.Left)
                    .ToArray();
            }
            finally
            {
                _worldLock.ExitReadLock();
            }
        }

        // ── Private helpers ─────────────────────────────────────────────────

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
                    if (staticInfo.Handle == ignoreHandle) continue;
                    if (!seen.Add(staticInfo.Handle)) continue;
                    if (staticInfo.Bounds.Intersects(bounds))
                        throw new InvalidOperationException(
                            $"Bounds {bounds} intersect existing static object {staticInfo.Handle} with bounds {staticInfo.Bounds}.");
                }

                foreach (var dynamicInfo in chunk.DynamicInfos)
                {
                    if (dynamicInfo.Handle == ignoreHandle) continue;
                    if (!seen.Add(dynamicInfo.Handle)) continue;
                    var other = GetRequiredObject(dynamicInfo.Handle);
                    if (other.Bounds.Intersects(bounds))
                        throw new InvalidOperationException(
                            $"Bounds {bounds} intersect existing dynamic object {dynamicInfo.Handle} with bounds {other.Bounds}.");
                }
            }
        }

        private void AddObjectToChunks(WorldObject obj)
        {
            if (obj is WorldObjectMoveable)
                AddDynamicObjectToChunks(obj.Handle, obj.Bounds);
            else
                AddStaticObjectToChunks(obj.Handle, obj.Bounds);
        }

        private void RemoveObjectFromChunks(WorldObject obj)
        {
            if (obj is WorldObjectMoveable)
                RemoveDynamicObjectFromChunks(obj.Handle, obj.Bounds);
            else
                RemoveStaticObjectFromChunks(obj.Handle, obj.Bounds);
        }

        private void AddStaticObjectToChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                var chunk = GetOrCreateChunk(chunkPos);
                foreach (var existing in chunk.StaticInfos)
                    if (existing.Handle == handle)
                        throw new InvalidOperationException($"Static handle {handle} already exists in chunk {chunkPos}.");
                chunk.StaticInfos.Add(new StaticChunkInfo(handle, bounds));
            }
        }

        private void AddDynamicObjectToChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                var chunk = GetOrCreateChunk(chunkPos);
                foreach (var existing in chunk.DynamicInfos)
                    if (existing.Handle == handle)
                        throw new InvalidOperationException($"Dynamic handle {handle} already exists in chunk {chunkPos}.");
                chunk.DynamicInfos.Add(new DynamicChunkInfo(handle));
            }
        }

        private void RemoveStaticObjectFromChunks(ObjectHandle handle, Bounds bounds)
        {
            foreach (var chunkPos in EnumerateTouchedChunks(bounds))
            {
                if (!_chunks.TryGetValue(chunkPos, out var chunk))
                    throw new InvalidOperationException($"Expected static object {handle} chunk {chunkPos} to exist, but it did not.");
                if (!RemoveStaticHandleFromChunk(chunk, handle))
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
                if (!RemoveDynamicHandleFromChunk(chunk, handle))
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
                _chunks.Remove(chunkPos);
        }

        private IEnumerable<ChunkPosition> EnumerateTouchedChunks(Bounds bounds)
        {
            int minChunkX = FloorDiv(bounds.Left,       _options.ChunkInfo.ChunkSize);
            int maxChunkX = FloorDiv(bounds.Right - 1,  _options.ChunkInfo.ChunkSize);
            int minChunkY = FloorDiv(bounds.Top,        _options.ChunkInfo.ChunkSize);
            int maxChunkY = FloorDiv(bounds.Bottom - 1, _options.ChunkInfo.ChunkSize);

            for (int y = minChunkY; y <= maxChunkY; y++)
                for (int x = minChunkX; x <= maxChunkX; x++)
                    yield return new ChunkPosition(x, y);
        }

        private static int FloorDiv(int value, int divisor)
        {
            int quotient = value / divisor;
            int remainder = value % divisor;
            if (remainder != 0 && ((remainder < 0) != (divisor < 0)))
                quotient--;
            return quotient;
        }

        public void Dispose()
        {
            StopSimulationProcessing();
            _worldLock.Dispose();
        }
    }
}
