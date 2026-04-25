using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Fills the ground Tilemap at scene start:
///   - outer area: grass
///   - town area (derived from building bounds + margin): dirt
///
/// Assign grass and dirt sprites in the Inspector. The script creates
/// runtime Tile instances so no pre-authored Tile assets are needed.
/// </summary>
public class GroundGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap _tilemap;

    [Header("Tile Sprites — assign landscapeTiles_NNN sprites here")]
    [SerializeField] private Sprite _grassSprite;
    [SerializeField] private Sprite _dirtSprite;

    [Header("Map Size (in simulation grid cells)")]
    [SerializeField] private int _mapWidth  = 40;
    [SerializeField] private int _mapHeight = 40;

    [Header("Town Area (dirt patch)")]
    [SerializeField] private int _townLeft   = 0;
    [SerializeField] private int _townTop    = 0;
    [SerializeField] private int _townRight  = 30;
    [SerializeField] private int _townBottom = 25;

    private void Start()
    {
        if (_tilemap == null) { Debug.LogError("GroundGenerator: Tilemap not assigned."); return; }
        if (_grassSprite == null || _dirtSprite == null) { Debug.LogError("GroundGenerator: Sprites not assigned."); return; }

        var grassTile = MakeTile(_grassSprite);
        var dirtTile  = MakeTile(_dirtSprite);

        for (int sy = 0; sy < _mapHeight; sy++)
        {
            for (int sx = 0; sx < _mapWidth; sx++)
            {
                // Sim grid uses Y-down; tilemap cell uses Y-up (negated).
                var cell = IsometricUtil.SimToCell(sx, sy);
                bool inTown = sx >= _townLeft && sx < _townRight
                           && sy >= _townTop  && sy < _townBottom;
                _tilemap.SetTile(cell, inTown ? dirtTile : grassTile);
            }
        }
    }

    private static Tile MakeTile(Sprite sprite)
    {
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = sprite;
        return t;
    }
}
