# NPCChat — Planning

## Current Phase
**Phase 13: Unity Prototype — Small Town**
Goal: render the small town in Unity with player WASD + mouse movement, a couple NPCs, couple buildings. NPCChat.Core runs the simulation; Unity is the game client. Use Kenney 2D isometric assets (prototyping style — lighting/shaders deferred).

## Completed (this session)
- Reviewed full codebase state (world.yaml, YamlWorldLoader, GridRenderer, Unity starter project)
- Confirmed Phase 12 (YAML serialization) is complete and fully working
- **Reversed prior "procedural map" decision** — Unity Tilemap editor (WYSIWYG) replaces procedural generation
- Decided on hybrid architecture: Unity Tilemap for visual layout, YAML for game data
- Decided on YAML folder hierarchy with loader-resolved includes (no post-build merge)
- Created `/wrap-up` custom slash command at `.claude/commands/wrap-up.md`

## Completed (prior sessions, Phases 0–12)
- World object hierarchy (Static, Carryable, Moveable)
- Dual-thread model (UI + Simulation), ReaderWriterLockSlim, Channel<T> commands
- A* pathfinding, step movement
- Behavior system (ActorComponent, ActionQueue, schedules, reactive rules, perception)
- Dialogue system (nodes, trees, mood scoring, context-sensitive verbs, session state machine)
- Inventory & Loot, Quests, Factions & Reputation
- Advanced AI (combat alerts, line-of-sight, alert board)
- Shopkeepers & Economy, Combat (AttackTask, CombatStats)
- YAML world loading (YamlWorldLoader, YamlWorldDef, world.yaml)
- JSON save/load (SaveGameService, LoadGameService)
- Unity project created; NPCChat.Core DLL + dependencies auto-copied to Assets/Plugins via post-build target
- netstandard2.1 migration complete; all tests pass

## Next Steps (ordered)
1. **Extract Kenney Miniature packs** — unzip Farm, Dungeon, Overworld, Library, Prototype from KennyAssets\
2. **YAML folder reorganization** — split world.yaml into Data\ hierarchy; add `includes:` resolution to YamlWorldLoader
3. **Wire YAML to WPF editor** — replace AddSmallTown() button with YamlWorldLoader path; add asset root path to UserSettings; verify simulation still works
4. **Unity isometric Tilemap scene** — configure isometric grid (132×83px tiles), import Isometric Tiles Base, lay out town ground tiles
5. **Import building sprites** — Isometric Medieval Town pack; place buildings visually in Unity scene
6. **Unity YAML loader scripts** — script to load Data\ → WorldData → start simulation (YamlDotNet DLL already in Assets/Plugins via post-build copy)
7. **Unity GameObjects** — player + NPC prefabs driven by simulation positions each tick; WASD + mouse input

## Open Questions / Decisions Needed
- Which Kenney Miniature style for characters? (Need to see extracted packs before deciding)
- Authoring convention for the pathfinding seam: placing a building visually in Unity requires also adding its bounds to layout.yaml. This needs to be documented.
- Asset root path config: stored in UserSettings (persistent), picker button in WPF editor, default to NPCChat.Unity\NPCGame\Assets\

## Key Architecture Decisions

| Decision | Detail |
|----------|--------|
| Map authoring | **Unity Tilemap (WYSIWYG)** — reversed prior "procedural" decision |
| Game data authoring | YAML (NPC defs, item placements, quest data, building obstacle bounds) |
| WPF Editor role | **Simulation debugger only** — colored polygon rendering sufficient; no sprite rendering needed |
| YAML organization | Folder hierarchy + loader-resolved `includes:` at startup; no post-build merge |
| Save files | JSON captures runtime state; separate from YAML starting-world files |
| Asset source of truth | `NPCChat.Unity\NPCGame\Assets\` — both WPF editor and Unity reference same folder |
| DLL location | `NPCChat.Unity\NPCGame\Assets\Plugins` (auto-copied by post-build target) |
| Engine-agnostic core | No Unity-specific code in NPCChat.Core |
| Ground tiles | Isometric Tiles Base, 132×83px, 2:1 isometric diamond |
| Building sprites | Isometric Medieval Town, 210×244px, 4 rotations (_0/_1/_2/_3) |
| Camera | Orthographic, isometric angle (x≈30°, y≈45°) |
