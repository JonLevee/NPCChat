# NPCChat — Planning

## Current Phase
**Phase 13: Unity Prototype — Small Town**
Goal: render the small town in Unity with player click-to-move, a couple NPCs, couple buildings. NPCChat.Core runs the simulation; Unity is the game client. Use Kenney 2D isometric assets (prototyping style — lighting/shaders deferred).

## Completed (this session)
- CopyToUnity target updated to copy *.dll;*.pdb;*.json (PDBs needed for cross-framework debugging); old PostBuild target removed
- world.yaml split into 6 files: items.yaml, factions.yaml, quests.yaml, layout.yaml, npcs.yaml, loot.yaml
- YamlWorldLoader.LoadFromFile now resolves includes: recursively and merges all defs before Apply()
- WPF editor rewired: BuildDemoTown + Load both use YamlWorldLoader instead of AddSmallTown()
- DataRootPath added to UserSettingsRepository (defaults to Data/ next to exe)
- YAML files linked as Content in editor csproj → copy to output Data/ folder at build
- YAML files copy to Assets/StreamingAssets/Data/ in Unity via csproj target (user implemented)
- All 8 Unity scripts written: SimulationBootstrap, IsometricUtil, WorldView, ActorView, BuildingView, PlayerInput, GroundGenerator, UI/InteractionMenu, UI/DialoguePanel
- Architecture decision: YAML is source of truth; Unity is pure display/input; no double-authoring

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
- Reversed prior "procedural map" decision — Unity Tilemap editor (WYSIWYG) replaces procedural generation
- Decided on hybrid architecture: Unity Tilemap for visual layout, YAML for game data
- Decided on YAML folder hierarchy with loader-resolved includes (no post-build merge)

## Next Steps (ordered)
1. **Delete stale NPCGame\ subfolder** — confirm with user, then remove NPCChat.Unity\NPCGame\
2. **Unity Editor scene setup** — create hierarchy: Bootstrap GO, Grid+Tilemap (Isometric Z As Y), WorldView GO, Canvas+UI panels
3. **Identify tile sprites** — visually open landscapeTiles_NNN.png to find grass and dirt tile indices
4. **Wire Inspector references** — sprites (grass, dirt, building, player, NPC), InteractionPopup prefab, DialoguePanel links
5. **Create InteractionPopup prefab** — Panel > Title Text + Buttons VerticalLayoutGroup
6. **Hit Play and test** — YAML load → building/NPC spawn → player click-to-move → interaction menu → dialogue
7. **Tune Grid cell size** — adjust after first run based on PPU import setting

## Open Questions / Decisions Needed
- Which landscapeTiles_NNN.png = grass, which = dirt? (visual inspection needed)
- Grid cell size: depends on PPU import setting — likely (1.0, 0.5, 0.5) at PPU=132 or (1.32, 0.66, 0.66) at PPU=100
- NPCGame\ deletion: still pending user confirmation

## Key Architecture Decisions

| Decision | Detail |
|----------|--------|
| Unity project root | NPCChat.Unity\ (NOT NPCGame subfolder) |
| Map authoring | Unity Tilemap (WYSIWYG) for ground tiles only |
| Game data authoring | YAML — single source of truth for buildings, NPCs, items |
| Unity architecture | YAML is source of truth; Unity is pure display/input layer; no double-authoring |
| Building placement | Runtime-spawned from layout.yaml bounds; no manual Unity scene placement |
| YAML path in Unity | Application.streamingAssetsPath + "/Data/world.yaml" |
| YAML path in WPF | UserSettingsRepository.GetDataRootPath() → defaults to Data/ next to exe |
| Coordinate system | Sim: (0,0) top-left, Y down. Unity: negate Y for isometric cell. IsometricUtil handles conversion. |
| WPF Editor role | Simulation debugger only — colored polygon rendering sufficient |
| YAML organization | Folder hierarchy + loader-resolved includes: at startup |
| Save files | JSON captures runtime state; separate from YAML starting-world files |
| DLL location | NPCChat.Unity\Assets\Plugins (*.dll + *.pdb + *.json auto-copied by CopyToUnity target) |
| Engine-agnostic core | No Unity-specific code in NPCChat.Core |
| Ground tiles | Isometric Tiles Base, 132×83px, 2:1 isometric diamond |
| Building sprites | Isometric Medieval Town pieces (roofs, walls, doors, stairs) |
| Character sprites | Human sprites from Assets/Art/Sprites/Characters/Human/ |
| Camera | Orthographic, looking down -Z |
| Sprite import | ArtSpriteImporter.cs auto-sets Sprite type + Point filter for Assets\Art\ |
