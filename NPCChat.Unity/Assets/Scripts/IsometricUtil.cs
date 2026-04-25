using NPCChat.Core.WorldClasses;
using UnityEngine;

/// <summary>
/// Converts between simulation grid coordinates and Unity world positions.
///
/// Simulation: (0,0) = top-left, X right, Y down (integer grid).
/// Unity Isometric Grid: Y increases upward, so we negate sim Y.
/// All conversions go through the scene's Grid component so the
/// tile dimensions are driven by whatever is configured in the Inspector.
/// </summary>
public static class IsometricUtil
{
    /// <summary>Simulation grid → tilemap cell (negates Y for isometric).</summary>
    public static Vector3Int SimToCell(int simX, int simY)
        => new Vector3Int(simX, -simY, 0);

    /// <summary>Simulation grid → Unity world position (via Grid component).</summary>
    public static Vector3 SimToWorld(int simX, int simY, Grid grid)
        => grid.GetCellCenterWorld(SimToCell(simX, simY));

    /// <summary>Simulation Bounds center → Unity world position.</summary>
    public static Vector3 BoundsCenterToWorld(Bounds b, Grid grid)
        => SimToWorld(b.Left + b.Width / 2, b.Top + b.Height / 2, grid);

    /// <summary>Unity world position → simulation grid coordinates (inverse).</summary>
    public static (int simX, int simY) WorldToSim(Vector3 worldPos, Grid grid)
    {
        var cell = grid.WorldToCell(worldPos);
        return (cell.x, -cell.y);
    }
}
