using NPCChat.Core.WorldClasses;
using UnityEngine;

/// <summary>
/// Attached to each building GameObject. Positioned once by WorldView at scene start.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BuildingView : MonoBehaviour
{
    public void Init(Bounds bounds, Sprite sprite, Grid grid)
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        // Render above ground tiles, below actors.
        sr.sortingOrder = 5;

        transform.position = IsometricUtil.BoundsCenterToWorld(bounds, grid);

        // Scale the sprite to visually fill the building's footprint.
        // One grid cell = one tile width in world units; get that from the Grid.
        Vector3 cellWorld = grid.GetCellCenterWorld(Vector3Int.one)
                          - grid.GetCellCenterWorld(Vector3Int.zero);
        float unitX = Mathf.Abs(cellWorld.x);
        float unitY = Mathf.Abs(cellWorld.y);

        float targetW = bounds.Width  * unitX;
        float targetH = bounds.Height * unitY;

        if (sr.sprite != null)
        {
            float spriteW = sr.sprite.bounds.size.x;
            float spriteH = sr.sprite.bounds.size.y;
            if (spriteW > 0 && spriteH > 0)
                transform.localScale = new Vector3(targetW / spriteW, targetH / spriteH, 1f);
        }
    }
}
