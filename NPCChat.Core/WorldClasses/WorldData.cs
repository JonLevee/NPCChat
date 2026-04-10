#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using NPCChat.Core.Attributes;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.Exceptions;
using NPCChat.Core.Extensions;
using NPCChat.Core.PathfindingClasses;

namespace NPCChat.Core.WorldClasses
{
    [Scoped]
    public sealed class WorldData : IDisposable
    {
        private readonly Dictionary<ChunkPosition, ChunkData> _chunks = [];
        private readonly List<WorldObjectMoveable> _moveableObjects = [];
        private readonly Queue<MoveCommand> _pendingPathCommands = new();
        private readonly IWorldOptions _options;
        private readonly ObjectHandleManager _handleManager;
        private readonly AStarPathfinder _pathfinder;

        // Threading
        private readonly ReaderWriterLockSlim _worldLock = new(LockRecursionPolicy.NoRecursion);
        private readonly Channel<MoveCommand> _moveCommands =
              Channel.CreateBounded<MoveCommand>(new BoundedChannelOptions(int.MaxValue)
              {
                  FullMode = BoundedChannelFullMode.DropOldest,
                  SingleReader = true,
                  SingleWriter = false
              });
        private Task _simulationProcessingTask = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;

        // Behavior system
        // Assumption: single player. If a second Player object is added, _playerObject
        // tracks only the most recently added one.
        private WorldObjectMoveable? _playerObject;
        private int _currentGameTick;

        /// <summary>
        /// Set by the simulation loop when it faults. The UI timer checks this
        /// each tick and rethrows on the UI thread so the error is visible.
        /// Null while the simulation is healthy.
        /// </summary>
        public Exception? SimulationFault { get; private set; }

        public WorldData(IWorldOptions options, ObjectHandleManager handleManager, AStarPathfinder pathfinder)
        {
            _options = options;
            _handleManager = handleManager;
            _pathfinder = pathfinder;
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

                    // Drain channel into pending queue.
                    while (_moveCommands.Reader.TryRead(out var cmd))
                        _pendingPathCommands.Enqueue(cmd);

                    // Process up to PathBudgetPerTick commands this tick.
                    int budget = _options.PathBudgetPerTick;
                    while (budget > 0 && _pendingPathCommands.Count > 0)
                    {
                        ProcessMoveCommand(_pendingPathCommands.Dequeue());
                        budget--;
                    }

                    AdvanceMoveables();
                    AdvanceBehaviors();

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

        private void ProcessMoveCommand(MoveCommand cmd)
        {
            WorldObjectMoveable mover;
            Bounds moverBounds;

            _worldLock.EnterReadLock();
            try
            {
                if (!TryGetObject(cmd.Mover, out var obj) || obj is not WorldObjectMoveable m)
                    return;
                mover = m;
                moverBounds = mover.Bounds;
            }
            finally
            {
                _worldLock.ExitReadLock();
            }

            // Center the mover over the clicked tile.
            var targetTopLeft = new Point(
                cmd.Target.X - moverBounds.Width / 2,
                cmd.Target.Y - moverBounds.Height / 2);
            var sourceTopLeft = new Point(moverBounds.Left, moverBounds.Top);

            if (sourceTopLeft == targetTopLeft)
            {
                mover.Movement.ClearMovement();
                return;
            }

            var grid = CreatePathGrid(cmd.Mover, moverBounds);
            var path = _pathfinder.FindPath(grid, sourceTopLeft, targetTopLeft, _options.MaxPathIterations);

            if (path is null || path.Count == 0)
            {
                mover.Movement.ClearMovement();
                return;
            }

            mover.Movement.ClearMovement();
            mover.Movement.FinalTarget = targetTopLeft;
            foreach (var step in path)
                mover.Movement.Path.Enqueue(step);
        }

        public PathGrid CreatePathGrid(ObjectHandle moverHandle, Bounds moverBounds)
        {
            var obstacles = new List<Bounds>();

            _worldLock.EnterReadLock();
            try
            {
                var seen = new HashSet<ObjectHandle>();

                foreach (var chunk in _chunks.Values)
                {
                    foreach (var info in chunk.StaticInfos)
                    {
                        if (seen.Add(info.Handle))
                            obstacles.Add(info.Bounds);
                    }

                    foreach (var info in chunk.DynamicInfos)
                    {
                        if (info.Handle == moverHandle) continue;
                        if (seen.Add(info.Handle) && TryGetObject(info.Handle, out var obj) && obj is not null)
                            obstacles.Add(obj.Bounds);
                    }
                }
            }
            finally
            {
                _worldLock.ExitReadLock();
            }

            return new PathGrid(obstacles, moverBounds.Width, moverBounds.Height);
        }

        private void AdvanceMoveables()
        {
            // Snapshot the list under read lock to avoid races with AddObject/RemoveObject.
            WorldObjectMoveable[] snapshot;
            _worldLock.EnterReadLock();
            try { snapshot = _moveableObjects.ToArray(); }
            finally { _worldLock.ExitReadLock(); }

            float tickSeconds = _options.SimTickMs / 1000f;

            foreach (var mover in snapshot)
            {
                if (!mover.Movement.IsMoving) continue;

                // Accumulate fractional progress based on MaxSpeed (grid units/sec).
                mover.Movement.StepAccumulator += mover.MaxSpeed * tickSeconds;
                if (mover.Movement.StepAccumulator < 1f) continue;
                mover.Movement.StepAccumulator -= 1f;

                var nextStep = mover.Movement.Path.Peek();
                var currentBounds = mover.Bounds;
                var newBounds = new Bounds(
                    nextStep.X, nextStep.Y,
                    nextStep.X + currentBounds.Width,
                    nextStep.Y + currentBounds.Height);

                _worldLock.EnterWriteLock();
                try
                {
                    // Verify the mover hasn't been removed since the snapshot.
                    if (!TryGetObject(mover.Handle, out _))
                    {
                        mover.Movement.ClearMovement();
                        continue;
                    }

                    // Verify the next step is still unobstructed.
                    EnsureNoIntersection(newBounds, mover.Handle);

                    RemoveDynamicObjectFromChunks(mover.Handle, currentBounds);
                    mover.Bounds = newBounds;
                    AddDynamicObjectToChunks(mover.Handle, newBounds);

                    mover.Movement.Path.Dequeue();
                    mover.Movement.Facing = Direction8Extensions.FromDelta(
                        nextStep.X - currentBounds.Left,
                        nextStep.Y - currentBounds.Top);
                }
                catch (InvalidOperationException)
                {
                    // Step is newly blocked — abandon the current path.
                    mover.Movement.ClearMovement();
                }
                finally
                {
                    _worldLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Returns a snapshot of all moveable objects' current positions.
        /// Safe to call from the UI thread.
        /// </summary>
        public (ObjectHandle Handle, Bounds Bounds)[] SnapshotMoveablePositions()
        {
            _worldLock.EnterReadLock();
            try
            {
                var result = new (ObjectHandle, Bounds)[_moveableObjects.Count];
                for (int i = 0; i < _moveableObjects.Count; i++)
                    result[i] = (_moveableObjects[i].Handle, _moveableObjects[i].Bounds);
                return result;
            }
            finally { _worldLock.ExitReadLock(); }
        }

        /// <summary>
        /// Returns a snapshot of the remaining path steps for the given moveable.
        /// Returns an empty array if the handle is unknown or not moving.
        /// Safe to call from the UI thread.
        /// </summary>
        public Point[] SnapshotPath(ObjectHandle handle)
        {
            _worldLock.EnterReadLock();
            try
            {
                if (!TryGetObject(handle, out var obj) || obj is not WorldObjectMoveable m)
                    return [];
                return m.Movement.Path.ToArray();
            }
            finally { _worldLock.ExitReadLock(); }
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
                {
                    _moveableObjects.Add(moveable);
                    if (moveable.Kind == WorldObjectKind.Player)
                        _playerObject = moveable;
                }

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
                {
                    _moveableObjects.Remove(moveable);
                    if (moveable == _playerObject)
                        _playerObject = null;
                }

                _handleManager.RemoveSlot(handle);
            }
            finally
            {
                _worldLock.ExitWriteLock();
            }
        }

        public bool TryGetObject(ObjectHandle handle, out WorldObject? obj)
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
            int minChunkX = FloorDiv(bounds.Left, _options.ChunkInfo.ChunkSize);
            int maxChunkX = FloorDiv(bounds.Right - 1, _options.ChunkInfo.ChunkSize);
            int minChunkY = FloorDiv(bounds.Top, _options.ChunkInfo.ChunkSize);
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

        private void AdvanceBehaviors()
        {
            // Snapshot under read lock — same pattern as AdvanceMoveables.
            WorldObjectMoveable[] snapshot;
            Bounds? playerBounds;
            _worldLock.EnterReadLock();
            try
            {
                snapshot = _moveableObjects.ToArray();
                playerBounds = _playerObject?.Bounds;
            }
            finally { _worldLock.ExitReadLock(); }

            int gameTick = _currentGameTick++;
            int gameHour = (gameTick / _options.TicksPerGameHour) % 24;

            foreach (var actor in snapshot)
            {
                if (actor.Actor is not { } component) continue;

                // Proximity partitioning: distant actors run every N ticks.
                bool isNearby = playerBounds is null || IsNearbyPlayer(actor.Bounds, playerBounds.Value);
                if (!isNearby)
                {
                    component.TicksSinceLastProcess++;
                    if (component.TicksSinceLastProcess < component.DistantProcessInterval)
                        continue;
                    component.TicksSinceLastProcess = 0;
                }

                var ctx = new SimContext(actor, playerBounds, gameTick, gameHour, EnqueueMoveCommand);

                // 1. Schedule check — transition mode if the time range changed.
                var scheduledMode = component.Schedule.GetModeForTime(gameHour);
                if (!string.IsNullOrEmpty(scheduledMode) && scheduledMode != component.Mode)
                    component.Mode = scheduledMode;

                // 2. Reactive rules — inject a task if a higher-priority trigger fires.
                var currentTask = component.ActionQueue.TryPeekHighest();
                int currentPriority = currentTask?.Priority ?? int.MinValue;

                foreach (var rule in component.ReactiveRules)
                {
                    if (rule.Priority <= currentPriority) continue;
                    if (!rule.Trigger(ctx)) continue;

                    // Higher-priority task wins: interrupt the current one.
                    if (currentTask is not null)
                    {
                        component.ActionQueue.Dequeue();
                        currentTask.Interrupt(ctx);
                    }

                    component.ActionQueue.Enqueue(rule.ActionFactory(ctx));
                    break;
                }

                // 3. Tick current task.
                var task = component.ActionQueue.TryPeekHighest();
                if (task is null) continue;

                // Call Begin on the first tick.
                if (task.TurnsElapsed == 0)
                    task.Begin(ctx);

                // Enforce MaxTurns guard.
                if (task.TurnsElapsed >= task.MaxTurns)
                {
                    component.ActionQueue.Dequeue();
                    continue;
                }

                bool done = task.Tick(ctx);
                if (done)
                    component.ActionQueue.Dequeue();
            }
        }

        private bool IsNearbyPlayer(Bounds actorBounds, Bounds playerBounds)
        {
            int chunkSize = _options.ChunkInfo.ChunkSize;
            int ax = actorBounds.Left / chunkSize;
            int ay = actorBounds.Top / chunkSize;
            int px = playerBounds.Left / chunkSize;
            int py = playerBounds.Top / chunkSize;
            return Math.Abs(ax - px) <= 2 && Math.Abs(ay - py) <= 2;
        }

        public void Dispose()
        {
            StopSimulationProcessing();
            _worldLock.Dispose();
        }
    }
}
