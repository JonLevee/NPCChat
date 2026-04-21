# NPCChat — Planning

## Current Phase
**Phase 13: Unity Prototype — Small Town**
Goal: render the small town in Unity with player WASD + mouse movement, a couple NPCs, couple buildings. NPCChat.Core runs the simulation; Unity is the game client. Use Kenney 2D isometric assets (prototyping style — lighting/shaders deferred).

## Completed (this session)
- Verified Kenney art packs already extracted in KennyAssets\
- Copied relevant packs into NPCChat.Unity\Assets\Art\ with correct folder structure
- Manually set all PNGs to Sprite (2D and UI) type in Unity Editor
- Wrote Assets\Editor\ArtSpriteImporter.cs — auto-sets Sprite type + Point filter + no mipmaps for future imports under Assets\Art\
- Confirmed post-build DLL copy targets correct path NPCChat.Unity\Assets\Plugins
- Unity project recreated at NPCChat.Unity\ root (old NPCGame\ subfolder is stale leftover)

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
- Unity project created; NPCChat.Core DLL + dependencies auto-copied to Assets\Plugins via post-build target
- netstandard2.1 migration complete; all tests pass
- Reversed prior "procedural map" decision — Unity Tilemap editor (WYSIWYG) replaces procedural generation
- Decided on hybrid architecture: Unity Tilemap for visual layout, YAML for game data
- Decided on YAML folder hierarchy with loader-resolved includes (no post-build merge)

## Next Steps (ordered)
1. **Delete stale NPCGame\ subfolder** — confirm with user, then remove NPCChat.Unity\NPCGame\ (old project leftover)
2. **YAML folder reorganization** — split world.yaml into Data\ hierarchy; add `includes:` resolution to YamlWorldLoader
3. **Wire YAML to WPF editor** — replace AddSmallTown() button with YamlWorldLoader path; add asset root path to UserSettings; verify simulation still works
4. **Unity isometric Tilemap scene** — configure isometric grid (132×83px tiles), import Isometric Tiles Base, lay out town ground tiles using Tile Palette
5. **Import building sprites** — Isometric Medieval Town pack; place buildings visually in Unity scene
6. **Unity YAML loader scripts** — MonoBehaviour to load Data\ → WorldData → start simulation (YamlDotNet DLL already in Assets\Plugins)
7. **Unity GameObjects** — player + NPC prefabs driven by simulation positions each tick; WASD + mouse input

## Open Questions / Decisions Needed
- Confirm deletion of stale NPCChat.Unity\NPCGame\ subfolder
- Which Kenney Miniature style for characters? (Prototype orange placeholders are available now)
- Authoring convention for pathfinding seam: placing a building in Unity Tilemap requires also adding its bounds to layout.yaml
- Asset root path config: stored in UserSettings (persistent), picker button in WPF editor, default to NPCChat.Unity\Assets\

## Key Architecture Decisions

| Decision | Detail |
|----------|--------|
| Unity project root | NPCChat.Unity\ (NOT NPCGame subfolder) |
| Map authoring | Unity Tilemap (WYSIWYG) — reversed prior "procedural" decision |
| Game data authoring | YAML (NPC defs, item placements, quest data, building obstacle bounds) |
| WPF Editor role | Simulation debugger only — colored polygon rendering sufficient |
| YAML organization | Folder hierarchy + loader-resolved `includes:` at startup |
| Save files | JSON captures runtime state; separate from YAML starting-world files |
| DLL location | NPCChat.Unity\Assets\Plugins (auto-copied by post-build target) |
| Engine-agnostic core | No Unity-specific code in NPCChat.Core |
| Ground tiles | Isometric Tiles Base, 132×83px, 2:1 isometric diamond |
| Building sprites | Isometric Medieval Town pieces (roofs, walls, doors, stairs) |
| Character sprites | Isometric Miniature Prototype (orange placeholders) for now |
| Camera | Orthographic, isometric angle |
| Sprite import | ArtSpriteImporter.cs auto-sets Sprite type + Point filter for Assets\Art\ |
