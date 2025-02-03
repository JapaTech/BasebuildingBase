using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPosition 
{
   public static Vector3 GridPositionFronWorldPoint(Vector3 worldPos, int gridScale)
   {
        var x = Mathf.Round(worldPos.x / gridScale) * gridScale;
        var y = Mathf.Round(worldPos.y / gridScale) * gridScale;
        var z = Mathf.Round(worldPos.z / gridScale) * gridScale;

        return new Vector3(x, y, z);
   }
}
