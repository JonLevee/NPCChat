using System.Collections.Generic;
using NPCChat.Core.WorldClasses;
using UnityEngine;

/// <summary>
/// Scene orchestrator. Runs after SimulationBootstrap.Start() finishes loading YAML.
/// Spawns a GameObject for every building and actor in the loaded world,
/// then syncs actor positions from simulation snapshots every frame.
/// </summary>
public class WorldView : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Grid _grid;
    [SerializeField] private InteractionMenu _interactionMenu;

    [Header("Sprites")]
    [SerializeField] private Sprite _buildingSprite;
    [SerializeField] private Sprite _playerSprite;
    [SerializeField] private Sprite _npcSprite;

    private WorldData _world;
    private readonly Dictionary<ObjectHandle, ActorView> _actorViews = new();

    private void Start()
    {
        _world = SimulationBootstrap.Instance.World;
        SpawnWorld();
    }

    private void SpawnWorld()
    {
        var objects = _world.EnumerateWorldObjects();
        var player  = _world.GetPlayer();

        foreach (var obj in objects)
        {
            switch (obj)
            {
                case WorldObjectStatic building:
                    SpawnBuilding(building);
                    break;

                case WorldObjectMoveable moveable:
                    SpawnActor(moveable, moveable.Handle == player?.Handle);
                    break;
            }
        }
    }

    private void SpawnBuilding(WorldObjectStatic building)
    {
        var go = new GameObject($"Building_{building.Bounds.Left}_{building.Bounds.Top}");
        go.transform.SetParent(transform);
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<BuildingView>().Init(building.Bounds, _buildingSprite, _grid);
    }

    private void SpawnActor(WorldObjectMoveable moveable, bool isPlayer)
    {
        var name   = isPlayer ? "Player" : (moveable.Character?.Name ?? $"NPC_{moveable.Handle}");
        var sprite = isPlayer ? _playerSprite : _npcSprite;

        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.AddComponent<SpriteRenderer>();
        var view = go.AddComponent<ActorView>();
        view.Init(moveable.Handle, isPlayer, sprite, _grid);
        view.Sync(moveable.Bounds);
        go.transform.position = IsometricUtil.BoundsCenterToWorld(moveable.Bounds, _grid);

        _actorViews[moveable.Handle] = view;

        if (isPlayer)
            go.AddComponent<PlayerInput>().Init(_world, _grid);
    }

    private void Update()
    {
        if (_world == null) return;

        // Sync actor positions from simulation snapshot (thread-safe read).
        var positions = _world.SnapshotMoveablePositions();
        foreach (var (handle, bounds) in positions)
        {
            if (_actorViews.TryGetValue(handle, out var view))
                view.Sync(bounds);
        }

        // Update interaction overlays.
        if (_interactionMenu != null)
        {
            var snapshots = _world.SnapshotInteractions();
            _interactionMenu.Refresh(snapshots, _grid);
        }
    }
}
