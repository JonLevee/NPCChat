# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build NPCChat.slnx

# Run tests
dotnet test NPCChat.Tests

# Run a single test class
dotnet test NPCChat.Tests --filter "ClassName=AStarPathfinderTests"

# Run Editor (WPF)
dotnet run --project NPCChat.Editor

# Clean
dotnet clean
```

## Architecture Overview

NPCChat is a multi-threaded WPF RPG world simulation engine (Phase 10 of ~12 complete). Three projects:

- **NPCChat.Core** — all simulation/game logic, class library
- **NPCChat.Editor** — WPF desktop app, renders the world, drives UI
- **NPCChat.Tests** — MSTest unit tests

### Threading Model

Two threads with strict ownership:

| Thread | Responsibility |
|--------|---------------|
| **UI (Main)** | Render, input, read volatile InteractionSnapshot[], catch SimulationFault |
| **Simulation** | Behavior ticks, movement, pathfinding, write world state |

Synchronization:
- `ReaderWriterLockSlim` (NoRecursion policy) guards chunks/objects in `WorldData`
- `Channel<T>` queues for cross-thread commands: `MoveCommand`, `DropCommand`, `ShopTransactionCommand`, `QuestRewardCommand`
- Volatile reference swap publishes `InteractionSnapshot[]` to UI thread without locks
- `SimulationFault` exception field is set by sim thread and checked by UI timer

Never read mutable world state from the UI thread without acquiring a read lock. Never block the sim thread on UI operations.

### Behavior System

Each NPC has an `ActorComponent` with:
- **`Mode`** (volatile string) — current activity (Idle, Work, Wander, Sleep, etc.)
- **`ActorSchedule`** — list of `ScheduleEntry` records that transition Mode at given `GameHour`
- **`ReactiveRules`** (`BehaviorRule[]`) — evaluated every tick in priority order; higher-priority rules interrupt lower-priority active tasks
- **`ActionQueue`** — sorted queue of `IActorTask`; only the highest-priority task ticks each frame
- **`PerceptionRange`** — Chebyshev distance for NPC awareness (default 8 tiles)
- **`DistantProcessInterval`** — skip N ticks for actors far from player (LOD optimization)

**`BehaviorRule`** pattern: `Trigger: Func<SimContext, bool>` runs every tick. When it fires, `ActionFactory: Func<SimContext, IActorTask>` creates a task.

**`IActorTask`** lifecycle: `Begin()` → `Tick()` (returns true when done) → `Interrupt()`. Tasks hold `SubTasks: LinkedList<SubTask>` that can be extended dynamically via `OnComplete` callbacks. `MaxTurns` is a safety ceiling.

**`SimContext`** is a readonly struct passed to all behavior delegates — contains Actor, PlayerBounds, GameTick, GameHour, and delegates like `EnqueueMove`, `CheckLineOfSight`, `PostAlert`, `GetNearbyAlerts`.

### Dialogue System

**`DialogueTree`** is an immutable conversation graph owned by one NPC. Node types:
- `DialogueLineNode` — NPC speaks a line
- `DialogueChoiceNode` — player selects from options
- `DialoguePoolNode` — affinity-scored node pool with cooldown tracking
- `DialogueSequenceNode` — ordered multi-step sequence

`MoodAxes[]` define character trait dimensions (e.g., Friendliness, Greed) shared across pool nodes. Use `DialogueTreeBuilder` (fluent API) to author trees in code.

**`DialogueContext`** is a readonly struct for conditions/effects: Actor/Player refs, GameHour, GameTick, `GetPlayerItemCount()` and `GetPlayerReputation()` delegates (thread-safe via locks internally).

### World & Spatial Model

**`WorldData`** (Scoped DI) is the central simulation container. Spatial organization: `Dictionary<ChunkPosition, ChunkData>`. Object hierarchy:
- `WorldObjectStatic` — unmoveable scenery/obstacles
- `WorldObjectCarryable` — items that can be dropped/picked up
- `WorldObjectMoveable` — NPCs and player with `MovementState`

The simulation loop in `WorldData` each tick: process move commands → advance behaviors (schedules → reactive rules → task tick) → run pathfinding with budget → publish InteractionSnapshot.

### Dependency Injection

Custom attribute-based auto-registration. Decorate classes with:
- `[Singleton]` — one instance app-wide (StaticData, PathFinder)
- `[Scoped]` — one per service scope (WorldData, WorldDataBuilder)
- `[Transient]` — new instance each time

`ConfigureServices.Configure()` reflects over assemblies, finds these attributes, and builds `ServiceDescriptor` entries. No manual registration needed.

### Static Game Data

**`StaticData`** (Singleton) holds immutable definitions registered at startup:
- `ItemDef` — items (case-insensitive key lookup)
- `QuestDef` — quests with objectives and rewards
- `FactionDef` — factions for reputation system

Currently populated via `WorldDataBuilder.GetTemplates()` code. YAML loading (Phase 12) will supplement this.

### Key Design Conventions

- **Readonly structs for context objects** (`SimContext`, `DialogueContext`) — stack-allocated, passed by value to delegates
- **Immutable definitions** (`ItemDef`, `QuestDef`, `FactionDef`) — set once at startup, read freely without locks
- **Channel<T> for commands** — non-blocking; sim thread drains channels each tick
- **Volatile fields** only for data that crosses thread boundaries without locks (Mode, InteractionSnapshot reference)
- **LinkedList<SubTask>** allows tasks to chain new work dynamically in `OnComplete` callbacks
- **Perception-based LOD**: `DistantProcessInterval` on `ActorComponent` skips ticks for far-away actors

### Project Status

Phases 0–10 complete. Upcoming: Phase 11 (Combat), Phase 12 (YAML world serialization).
