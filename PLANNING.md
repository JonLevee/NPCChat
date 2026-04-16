# NPCChat Unity Integration — Planning

**Branch:** `unity-audit`

## Completed

### Infrastructure
- **Post-build DLL copy** — `NPCChat.Core.csproj` has a `CopyToUnity` target that fires after every `netstandard2.1` build. Copies all output DLLs (including NuGet dependencies via `CopyLocalLockFileAssemblies=true`) to `NPCChat.Unity/NPCGame/Assets/Plugins`. Verified: 14 DLLs copy correctly.
- **`.gitignore` updated for Unity** — Unity-specific ignores scoped to `NPCChat.Unity/NPCGame/`. Added `!NPCChat.Unity/**/*.meta` override to ensure Unity GUID meta files are committed despite the global `*.meta` ignore rule.
- **`netstandard2.1` migration** — `NPCChat.Core` targets both `net8.0` and `netstandard2.1`. All unit tests pass.

## Open / Next Up

1. **Review `QuestRewardCommand.cs`** — Modified on `unity-audit` branch (shows in `git status`); not yet reviewed. Likely part of the audit changes from a prior session.

2. **Unity bootstrap / entry point** — No Unity-side C# has been written yet. Need a `MonoBehaviour` (or similar entry point) that:
   - Instantiates `NPCChat.Core` services via DI
   - Builds a world (procedurally — see below)
   - Drives the simulation tick each Unity frame (or fixed update)
   - Reads `InteractionSnapshot[]` for rendering

3. **Procedural map generation** — Decided to use procedural generation for Unity maps (not YAML-authored world files). Implementation not started. Should replace the `WorldDataBuilder` template approach used by the Editor.

4. **Unity rendering layer** — After bootstrap: render the world (tiles, NPCs, player) using Unity's 2D or 3D primitives. Not yet designed.

## Key Decisions

| Decision | Detail |
|----------|--------|
| Map authoring | Procedural (no YAML maps in Unity) |
| DLL location | `NPCChat.Unity/NPCGame/Assets/Plugins` |
| Engine-agnostic core | No Unity-specific code in `NPCChat.Core` |
| Unity replaces WPF Editor | WPF Editor remains for internal tooling; Unity is the gameplay runtime |
