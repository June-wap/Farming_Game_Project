using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PolygonCollider2D))]
public class MapBoundaryAutoSetup : MonoBehaviour
{
    [Header("Tilemap chinh de do bounds")]
    public Tilemap groundTilemap;  

    [ContextMenu("Auto Setup Boundary")]
    public void SetupBoundary()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("[MapBoundary] Chua gan groundTilemap!");
            return;
        }

        // Lay bounds cua tilemap
        groundTilemap.CompressBounds();
        Bounds bounds = groundTilemap.localBounds;

        // 4 goc cua map
        Vector2[] points = new Vector2[]
        {
            new Vector2(bounds.min.x, bounds.min.y), 
            new Vector2(bounds.max.x, bounds.min.y), 
            new Vector2(bounds.max.x, bounds.max.y), 
            new Vector2(bounds.min.x, bounds.max.y)  
        };

        GetComponent<PolygonCollider2D>().SetPath(0, points);
        Debug.Log($"[MapBoundary] Da setup: {bounds.min} → {bounds.max}");
    }
}
