using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// TileMapManager: Quét toàn bộ tile trong Tilemap và ghi lên Firebase.
// Kéo thả FiseBaseDatabaseManager vào Inspector để kết nối được Firebase.
public class TileMapManager : MonoBehaviour
{
    [Header("Tilemap cần quét")]
    public Tilemap tilemap_Ground;

    // ✅ FIX Bug #3: Thêm reference đến FiseBaseDatabaseManager để gọi Firebase.
    // Kéo GameObject chứa FiseBaseDatabaseManager vào ô này trong Inspector.
    [Header("Kết nối Firebase")]
    [SerializeField] private FiseBaseDatabaseManager dbManager;

    // Tên map hiện tại — dùng để tạo đường dẫn Firebase: Users/{userId}/Maps/{mapName}
    // Ví dụ: "Home", "Graveyard", "Village" — đặt trong Inspector
    [SerializeField] private string mapName = "Home";

    private Map map;

    public void Start()
    {
        map = new Map();
        WriteAllTileMapToFireBase();
    }

    public void WriteAllTileMapToFireBase()
    {
        // ✅ Guard: Kiểm tra sớm — tránh crash UnassignedReferenceException
        if (tilemap_Ground == null)
        {
            Debug.LogError("[TileMapManager] ❌ Chưa gán Tilemap Ground vào Inspector! Hãy kéo Tilemap vào ô 'Tilemap Ground'.");
            return;
        }

        // Bước 1: Quét toàn bộ ô trong bounds của Tilemap, tạo List<TilemapDetail>
        List<TilemapDetail> tilemapDetails = new List<TilemapDetail>();

        for (int x = tilemap_Ground.cellBounds.min.x; x < tilemap_Ground.cellBounds.max.x; x++)
        {
            for (int y = tilemap_Ground.cellBounds.min.y; y < tilemap_Ground.cellBounds.max.y; y++)
            {
                // Chỉ lưu những ô thực sự có tile (bỏ qua ô trống)
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (tilemap_Ground.GetTile(cellPos) != null)
                {
                    TilemapDetail tm_detail = new TilemapDetail(x, y, TilemapState.Ground);
                    tilemapDetails.Add(tm_detail);
                }
            }
        }

        // ✅ FIX Bug #2: Trước đây là `new Map(tilemap)` — biến `tilemap` không tồn tại.
        // Đúng phải truyền vào List<TilemapDetail> đã xây ở vòng for phía trên.
        map = new Map(tilemapDetails);

        // Bước 2: Chuyển Map thành JSON (JsonUtility.ToJson bên trong Map.ToString())
        string json = map.ToString();
        Debug.Log($"[TileMapManager] Đã quét {tilemapDetails.Count} tiles. JSON sẵn sàng ghi Firebase.");

        // ✅ FIX Bug #3: Thực sự gửi dữ liệu lên Firebase.
        // Trước đây chỉ có Debug.Log — Firebase không nhận được gì!
        if (dbManager != null)
        {
            // Lấy userId từ PlayerPrefs (do FiseBaseDatabaseManager đã lưu sẵn với key "USER_ID")
            string userId = PlayerPrefs.GetString("USER_ID", SystemInfo.deviceUniqueIdentifier);
            string path = $"Users/{userId}/Maps/{mapName}";

            dbManager.WriteDatabaseToPath(path, json);
            Debug.Log($"[TileMapManager] Đang ghi lên Firebase tại: {path}");
        }
        else
        {
            Debug.LogError("[TileMapManager] ❌ Chưa gán FiseBaseDatabaseManager vào Inspector! Dữ liệu KHÔNG được lưu.");
        }
    }
}
