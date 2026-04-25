# NPCChat


next:
-----

## scenarios

| Who        |   |   |
|------------|---|---|
| blacksmith |   |   |
|            |   |   |

 

-   	blacksmith (weapons)

-   		shop

-   			offers to sell equipment

-   			can make equipment for cost and time

-   			has hints about appropriate level quests

-   			shop hours 9-5

-   			occasionally needs to go get firewood, ingots, leather, bone

-   				gone for 10 minutes

-   			modes:

-   				forging:  2h common - 1w legendary

-   					heating

-   					hammering

-   					quenching -\> heating

-   					wrapping

-   				fetching resources

-   				at counter

-   			looking for a good assistant

-   		residence

-   			goes home in the evening

-   			can interrupt him at home for additional gold

-   	blacksmith (armor)

-   		shop

-   			offers to sell equipment (same)

-   			can make equipment for cost and time (same)

-   			has hints about appropriate level quests (same)

-   			shop hours 9-5 (same)

-   			occasionally needs to go get firewood, ingots, leather, cotton, silk

-   				gone for 10 minutes (assistant)

-   			modes:

-   				forging:  2h common - 1w legendary (same)

-   					heating

-   					hammering

-   					quenching -\> heating

-   					wrapping

-   				fetching resources

-   			has assistant

-   		residence is above shop (otherwise same)

-   			goes home in the evening

-   			can interrupt him at home for additional gold

-   	trader (general store)

-   		shop

-   			offers to sell equipment (general, plus poor quality weapons and armor)

-   			has hints about appropriate level quests

-   			shop hours 9-5

-   			occasionally needs to go get firewood, ingots, leather, bone

-   				gone for 10 minutes

-   			modes:

-   				at counter

-   				walking around store

-   				fetching goods

-   			has 2 assistants

-   		residence

-   			goes home in the evening (same)

-   town gossip

-    

-   		

-    

-    

irritability
trust
greed
secrecy
stress
diplomacy
resentment
familiarity
friendliness
sociability
authority
curiosity
vigilance
morale
gratitude
nostalgia
paranoia


# Other
Tip: Run /terminal-setup to enable convenient terminal integration like Shift + Enter for new line and more
rename /wrap-up to /save for clarity
Auto-update failed · Try claude doctor or npm i -g @anthropic-ai/claude-code


 Unity Editor setup (do these once)
                                                                                                                                                                            1. Configure the scene hierarchy
                                                                                                                                                                            In the Hierarchy, create this structure:

  Bootstrap          (empty GO)  ← add SimulationBootstrap.cs
  Grid               (Grid component: Layout = Isometric Z As Y, Cell Size = 1.32, 0.83, 1)
    Ground           (Tilemap + Tilemap Renderer components)
      ← add GroundGenerator.cs here
  WorldView          (empty GO)  ← add WorldView.cs
  Canvas             (Canvas, Canvas Scaler, Graphic Raycaster; Render Mode = Screen Space Camera)
    InteractionPanel ← add InteractionMenu.cs here
    DialoguePanel    ← add DialoguePanel.cs here (see UI layout below)
  EventSystem        (auto-created with Canvas, required for click blocking)

  2. Camera setup

  Select Main Camera:
  - Projection: Orthographic
  - Size: 8 (adjust to taste)
  - Position: (0, 0, -10)
  - Rotation: (0, 0, 0)
  - Background: solid black or dark color

  3. Identify your tile sprites

  Open a few landscapeTiles_NNN.png files in Windows Explorer to find:
  - A flat grass tile — try landscapeTiles_000 first
  - A flat dirt/sand tile — try landscapeTiles_016 or landscapeTiles_001

  For buildings pick any grey_house or grey_floor sprite from Tiles/Buildings/.
  For characters use Human_0_Idle0 from Sprites/Characters/Human/.

  4. Wire up Inspector references

  WorldView (on WorldView GO):
  - Grid → drag the Grid GO
  - Building Sprite → drag a building tile sprite
  - Player Sprite → drag Human_0_Idle0
  - Npc Sprite → drag a different human idle sprite
  - Interaction Menu → drag InteractionPanel

  GroundGenerator (on Ground GO):
  - Tilemap → drag the Ground Tilemap GO
  - Grass Sprite → drag your grass tile
  - Dirt Sprite → drag your dirt tile
  - Town Left/Top/Right/Bottom → 0, 0, 30, 25 (matches building area in layout.yaml)

  InteractionMenu (on InteractionPanel):
  - World Canvas → drag the Canvas GO
  - Dialogue Panel → drag DialoguePanel
  - Camera → drag Main Camera

  DialoguePanel: set up children (Title Text, Line Text, Choices layout) — or create a simple UI with a Panel > Title Text + Line Text + Choices VerticalLayoutGroup.

  5. Popup prefab for InteractionMenu

  Create a small UI prefab (right-click Canvas → UI → Panel, name it InteractionPopup):
  InteractionPopup (Panel)
    Title (Text)
    Buttons (VerticalLayoutGroup)
  Drag this into InteractionMenu's Popup Prefab field.

  6. Grid cell size — tuning

  The exact cell size depends on your PPU import setting. In ArtSpriteImporter.cs you can check/set PPU. If sprites are imported at 132 PPU: set Grid cell size to (1.0,
  0.5, 0.5). If at 100 PPU: use (1.32, 0.66, 0.66).

  ---
  That gives you all 7 items:
  - YAML loading: SimulationBootstrap — one GO, no other config
  - Ground layer: GroundGenerator — assign two sprites, hit Play
  - Buildings + NPCs + Player: WorldView spawns everything from simulation state
  - NPC movement: simulation drives it automatically
  - PC movement: left-click enqueues MoveCommand via PlayerInput
  - Interaction menus: InteractionMenu reads SnapshotInteractions() each frame
  - Dialogue: DialoguePanel reuses the exact same DialogueSession the WPF editor uses

