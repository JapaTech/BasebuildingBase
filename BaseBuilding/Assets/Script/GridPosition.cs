using UnityEngine;

public static class GridPosition 
{
    // Converts a world position to a grid-aligned position based on the specified grid scale
    public static Vector3 GridPositionFronWorldPoint(Vector3 worldPos, int gridScale)
   {
        // Round the X, Y, and Z coordinates to the nearest grid position
        var x = Mathf.Round(worldPos.x / gridScale) * gridScale;
        var y = Mathf.Round(worldPos.y / gridScale) * gridScale;
        var z = Mathf.Round(worldPos.z / gridScale) * gridScale;

        // Return the grid-aligned position as a Vector3
        return new Vector3(x, y, z);
   }
}
