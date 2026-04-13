using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ─── CẤU HÌNH 1 LAYER TRONG INSPECTOR ────────────────────────────────────────
// Mỗi TilemapLayerEntry đại diện cho 1 Tilemap layer trong scene.
// Thêm bao nhiêu layer tuỳ ý trong Inspector của TileMapManager.
[Serializable]
public class TilemapLayerEntry
{
    [Tooltip("Tên định danh layer — phải DUY NHẤT trong 1 scene.\nVí dụ: Ground, Decoration, Water, Trees")]
    public string layerName;

    [Tooltip("Tilemap component tương ứng trong Hierarchy")]
    public Tilemap tilemap;

    [Tooltip("Danh sách TẤT CẢ TileBase có thể xuất hiện trong layer này.\n" +
             "Index của mỗi tile trong mảng này được dùng để lưu Firebase.\n" +
             "QUAN TRỌNG: Đừng đổi thứ tự sau khi đã có dữ liệu trên Firebase!")]
    public TileBase[] tiles;
}

// ─── TILEEMAP MANAGER ─────────────────────────────────────────────────────────
// Quét nhiều Tilemap layer → lưu/tải từ Firebase theo từng layer riêng biệt.
//
// Cấu trúc dữ liệu Firebase:
//   Users/{userId}/Maps/{mapName}/{layerName}   ← JSON của TilemapLayerSaveData
//
// Cách dùng:
//   • Vào scene mới → Start() tự động gọi LoadAllLayersFromFirebase()
//   • Khi thoát scene → SceneChanger gọi SaveAllLayersToFirebase()
//   • Player cuốc/gieo/thu hoạch → chỉ cần SetTile trên Tilemap, không cần báo Manager
public class TileMapManager : MonoBehaviour
{
    [Header("Kết nối Firebase")]
    [SerializeField] private FiseBaseDatabaseManager dbManager;

    [Header("Tên map — phải khớp tên Scene trong Build Settings")]
    [SerializeField] public string mapName = "Home";

    [Header("Danh sách các Layer Tilemap cần lưu/tải")]
    [Tooltip("Thêm từng layer vào đây. Mỗi layer có tên riêng, Tilemap riêng, và danh sách TileBase riêng.")]
    public TilemapLayerEntry[] layers;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    void Start()
    {
        // Khi vừa vào scene: tải dữ liệu từ Firebase để vẽ lại bản đồ
        LoadAllLayersFromFirebase();
    }

    // ─── LƯU TẤT CẢ LAYER LÊN FIREBASE ──────────────────────────────────────
    // Gọi hàm này trước khi chuyển scene (từ SceneChanger) hoặc khi muốn lưu thủ công.
    // Mỗi layer được lưu thành 1 node riêng trên Firebase để dễ đọc và tránh JSON quá lớn.
    public void SaveAllLayersToFirebase()
    {
        if (!ValidateReferences()) return;

        int totalTilesSaved = 0;

        foreach (TilemapLayerEntry entry in layers)
        {
            if (!ValidateLayerEntry(entry)) continue;

            // Quét toàn bộ ô trong cellBounds của layer này
            TilemapLayerSaveData layerData = new TilemapLayerSaveData
            {
                layerName = entry.layerName
            };

            BoundsInt bounds = entry.tilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = entry.tilemap.GetTile(pos);
                    if (tile == null) continue;

                    // Tra cứu index của tile trong mảng tiles[] đã cấu hình trong Inspector
                    int tileIndex = FindTileIndex(entry.tiles, tile);
                    if (tileIndex < 0)
                    {
                        Debug.LogWarning($"[TileMapManager] Tile tại ({x},{y}) không có trong tiles[] của layer '{entry.layerName}'. Bỏ qua.");
                        continue;
                    }

                    layerData.cells.Add(new TilemapCellData { x = x, y = y, tileIndex = tileIndex });
                }
            }

            // Ghi từng layer lên 1 path riêng: Users/{userId}/Maps/{mapName}/{layerName}
            string json = JsonUtility.ToJson(layerData);
            string path = BuildLayerPath(mapName, entry.layerName);
            dbManager.WriteDatabaseToPath(path, json);

            totalTilesSaved += layerData.cells.Count;
            Debug.Log($"[TileMapManager] Layer '{entry.layerName}': lưu {layerData.cells.Count} tiles → {path}");
        }

        Debug.Log($"[TileMapManager] ✅ Lưu xong. Tổng: {totalTilesSaved} tiles, {layers.Length} layers.");
    }

    // ─── TẢI TẤT CẢ LAYER TỪ FIREBASE ───────────────────────────────────────
    // Mỗi layer được tải song song (async) — không blocking.
    public void LoadAllLayersFromFirebase()
    {
        if (!ValidateReferences()) return;

        Debug.Log($"[TileMapManager] Đang tải {layers.Length} layer(s) từ Firebase cho map '{mapName}'...");

        foreach (TilemapLayerEntry entry in layers)
        {
            if (!ValidateLayerEntry(entry)) continue;

            // Cần capture biến entry để dùng trong lambda (tránh closure bug)
            TilemapLayerEntry capturedEntry = entry;
            string path = BuildLayerPath(mapName, entry.layerName);

            dbManager.ReadDatabaseToPath(path, (jsonData) =>
            {
                // Không có dữ liệu Firebase → giữ nguyên tilemap mặc định của scene
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.Log($"[TileMapManager] Layer '{capturedEntry.layerName}': không có dữ liệu Firebase, dùng tilemap mặc định.");
                    return;
                }

                // Parse JSON → TilemapLayerSaveData
                TilemapLayerSaveData layerData = JsonUtility.FromJson<TilemapLayerSaveData>(jsonData);
                if (layerData == null || layerData.cells == null)
                {
                    Debug.LogError($"[TileMapManager] Layer '{capturedEntry.layerName}': ❌ Không thể parse JSON.");
                    return;
                }

                // Xóa layer hiện tại và vẽ lại từ dữ liệu Firebase
                capturedEntry.tilemap.ClearAllTiles();
                int drawn = 0;

                foreach (TilemapCellData cell in layerData.cells)
                {
                    if (cell.tileIndex < 0 || cell.tileIndex >= capturedEntry.tiles.Length)
                    {
                        Debug.LogWarning($"[TileMapManager] tileIndex {cell.tileIndex} vượt quá mảng tiles[] của layer '{capturedEntry.layerName}'.");
                        continue;
                    }

                    TileBase tile = capturedEntry.tiles[cell.tileIndex];
                    if (tile != null)
                    {
                        capturedEntry.tilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), tile);
                        drawn++;
                    }
                }

                Debug.Log($"[TileMapManager] ✅ Layer '{capturedEntry.layerName}': vẽ {drawn}/{layerData.cells.Count} tiles.");
            });
        }
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    // Tìm index của tile trong mảng tiles[] của entry.
    // Trả về -1 nếu không tìm thấy.
    private int FindTileIndex(TileBase[] tiles, TileBase target)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == target) return i;
        }
        return -1;
    }

    // Path Firebase cho từng layer: Users/{userId}/Maps/{mapName}/{layerName}
    private string BuildLayerPath(string targetMapName, string layerName)
    {
        return $"Users/{dbManager.UserId}/Maps/{targetMapName}/{layerName}";
    }

    private bool ValidateReferences()
    {
        if (dbManager == null)
        {
            Debug.LogError("[TileMapManager] ❌ Chưa gán FiseBaseDatabaseManager vào Inspector!");
            return false;
        }
        if (layers == null || layers.Length == 0)
        {
            Debug.LogError("[TileMapManager] ❌ Chưa cấu hình Layer nào trong Inspector!");
            return false;
        }
        return true;
    }

    private bool ValidateLayerEntry(TilemapLayerEntry entry)
    {
        if (entry.tilemap == null)
        {
            Debug.LogWarning($"[TileMapManager] Layer '{entry.layerName}': chưa gán Tilemap, bỏ qua.");
            return false;
        }
        if (entry.tiles == null || entry.tiles.Length == 0)
        {
            Debug.LogWarning($"[TileMapManager] Layer '{entry.layerName}': mảng tiles[] rỗng, bỏ qua.");
            return false;
        }
        return true;
    }
}
