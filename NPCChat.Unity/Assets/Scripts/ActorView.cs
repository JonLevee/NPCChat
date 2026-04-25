using NPCChat.Core.WorldClasses;
using UnityEngine;

/// <summary>
/// Attached to each player/NPC GameObject. WorldView creates one per moveable object
/// and calls Sync() each frame with the latest position from the simulation snapshot.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ActorView : MonoBehaviour
{
    public ObjectHandle Handle { get; private set; }
    public bool IsPlayer { get; private set; }

    private SpriteRenderer _sr;
    private Grid _grid;
    private Vector3 _targetWorld;

    // How fast the visual snaps toward the sim position (units/sec).
    // Set high so it tracks tightly on the default 20ms tick.
    [SerializeField] private float _moveSpeed = 10f;

    public void Init(ObjectHandle handle, bool isPlayer, Sprite sprite, Grid grid)
    {
        Handle  = handle;
        IsPlayer = isPlayer;
        _grid   = grid;
        _sr     = GetComponent<SpriteRenderer>();
        _sr.sprite = sprite;
        // Sort order: actors render above ground tiles.
        _sr.sortingOrder = 10;
    }

    /// <summary>Called by WorldView each frame with the latest sim position.</summary>
    public void Sync(NPCChat.Core.WorldClasses.Bounds bounds)
    {
        _targetWorld = IsometricUtil.BoundsCenterToWorld(bounds, _grid);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, _targetWorld, _moveSpeed * Time.deltaTime);
    }
}
