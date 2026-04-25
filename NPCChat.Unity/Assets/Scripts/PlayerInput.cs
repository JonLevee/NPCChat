using NPCChat.Core.WorldClasses;
using UnityEngine;
using SystemPoint = System.Drawing.Point;

/// <summary>
/// Handles left-click → MoveCommand for the player.
/// Added dynamically to the Player GameObject by WorldView.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private WorldData _world;
    private Grid _grid;
    private ObjectHandle _playerHandle;

    public void Init(WorldData world, Grid grid)
    {
        _world  = world;
        _grid   = grid;
        var player = _world.GetPlayer();
        _playerHandle = player?.Handle ?? ObjectHandle.None;
    }

    private void Update()
    {
        if (_playerHandle == ObjectHandle.None) return;
        if (!Input.GetMouseButtonDown(0)) return;

        // Block movement clicks when pointer is over UI.
        if (UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var (simX, simY) = IsometricUtil.WorldToSim(worldPos, _grid);
        _world.EnqueueMoveCommand(new MoveCommand(_playerHandle, new SystemPoint(simX, simY)));
    }
}
