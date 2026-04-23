using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ─── CẤU HÌNH 1 LAYER TRONG INSPECTOR ────────────────────────────────────────
// Mỗi TilemapLayerEntry đại diện cho 1 Tilemap layer CẦN LƯU FIREBASE.
// Chỉ khai báo các layer thay đổi trong gameplay (ví dụ: Ruong, Field).
// Layer tĩnh (Ground, Road, Tree...) không cần khai báo.
//
// Cách dùng nhanh (không kéo tay Tilemap):
//   1. Để trống field "tilemap"
//   2. Nhập layerName ĐÚNG với tên GameObject trong Hierarchy (phân biệt hoa thường)
//   3. Gán Grid vào field "Tilemap Root" của TileMapManager
//   → Awake() sẽ tự tìm và gán Tilemap component phù hợp
[Serializable]
public class TilemapLayerEntry
{
    [Tooltip("Tên định danh layer — phải DUY NHẤT và KHỚP với tên GameObject trong Hierarchy.\nVí dụ: Ruong, Field, Seed")]
    public string layerName;

    [Tooltip("Tilemap component tương ứng.\n" +
             "Để TRỐNG nếu muốn auto-detect từ Tilemap Root theo layerName.\n" +
             "Gán tay nếu tên GameObject khác layerName.")]
    public Tilemap tilemap;

    [Tooltip("Danh sách TẤT CẢ TileBase có thể xuất hiện trong layer này.\n" +
             "Index của mỗi tile trong mảng này được dùng để lưu Firebase.\n" +
             "QUAN TRỌNG: Đừng đổi thứ tự sau khi đã có dữ liệu trên Firebase!")]
    public TileBase[] tiles;
}

// ─── TILEEMAP MANAGER ─────────────────────────────────────────────────────────
// Quét các Tilemap layer được khai báo → lưu/tải từ Firebase.
//
// Tính năng Auto-Detect:
//   Gán Grid (hoặc bất kỳ parent nào) vào "Tilemap Root".
//   Với mỗi entry có tilemap == null, Manager sẽ tự tìm Tilemap
//   có tên GameObject trùng với layerName trong toàn bộ cây con của Root.
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
    [Tooltip("Không bắt buộc gán tay. Code sẽ tự động dùng FiseBaseDatabaseManager.Instance")]
    [SerializeField] private FiseBaseDatabaseManager dbManager;

    private FiseBaseDatabaseManager DB
    {
        get
        {
            if (FiseBaseDatabaseManager.Instance != null)
                return FiseBaseDatabaseManager.Instance;
            if (dbManager != null)
                return dbManager;
            return FindObjectOfType<FiseBaseDatabaseManager>();
        }
    }

    [Header("Tên map — phải khớp tên Scene trong Build Settings")]
    [SerializeField] public string mapName = "Home";

    [Header("Auto-Detect Tilemap từ Hierarchy")]
    [Tooltip("Gán GameObject 'Grid' (hoặc parent chứa tất cả Tilemap) vào đây.\n" +
             "Manager sẽ tự tìm Tilemap theo layerName trong toàn bộ cây con.\n" +
             "Để trống nếu muốn tự gán Tilemap tay cho từng entry.")]
    [SerializeField] private Transform tilemapRoot;

    [Header("Danh sách Layer CẦN LƯU FIREBASE")]
    [Tooltip("CHỈ khai báo layer thay đổi trong gameplay (ví dụ: Ruong).\n" +
             "Layer tĩnh (Ground, Road, Tree...) không cần thêm vào đây.")]
    public TilemapLayerEntry[] layers;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Tự động resolve các Tilemap chưa được gán tay
        if (tilemapRoot != null)
            AutoResolveTilemaps();
    }

    private void Start()
    {
        // Khi vừa vào scene: tải dữ liệu từ Firebase để vẽ lại bản đồ
        LoadAllLayersFromFirebase();
    }

    // ─── AUTO-DETECT: TỰ TÌM TILEMAP THEO TÊN ───────────────────────────────
    // Với mỗi entry chưa có tilemap, tìm Tilemap con có tên GameObject == layerName.
    // Nếu có nhiều Tilemap trùng tên, ưu tiên cái đầu tiên tìm được và log cảnh báo.
    private void AutoResolveTilemaps()
    {
        if (layers == null || layers.Length == 0) return;

        // Lấy toàn bộ Tilemap trong cây con (kể cả inactive)
        Tilemap[] allTilemaps = tilemapRoot.GetComponentsInChildren<Tilemap>(includeInactive: true);

        if (allTilemaps.Length == 0)
        {
            Debug.LogWarning($"[TileMapManager] Không tìm thấy Tilemap nào dưới '{tilemapRoot.name}'. " +
                             "Kiểm tra lại Tilemap Root.");
            return;
        }

        // Xây dictionary tên → Tilemap để tra cứu nhanh
        // Nếu trùng tên: giữ cái đầu tiên, log cảnh báo
        var nameToTilemap = new Dictionary<string, Tilemap>(StringComparer.Ordinal);
        foreach (Tilemap tm in allTilemaps)
        {
            string goName = tm.gameObject.name;
            if (!nameToTilemap.ContainsKey(goName))
            {
                nameToTilemap[goName] = tm;
            }
            else
            {
                Debug.LogWarning($"[TileMapManager] Có 2+ Tilemap tên '{goName}' dưới '{tilemapRoot.name}'. " +
                                 "Auto-detect dùng cái đầu tiên, kết quả có thể sai. Hãy gán tay.");
            }
        }

        int resolved = 0;
        foreach (TilemapLayerEntry entry in layers)
        {
            // Đã gán tay → bỏ qua, tôn trọng lựa chọn của designer
            if (entry.tilemap != null) continue;

            if (string.IsNullOrEmpty(entry.layerName))
            {
                Debug.LogWarning("[TileMapManager] Có entry không có layerName. Bỏ qua auto-detect cho entry này.");
                continue;
            }

            if (nameToTilemap.TryGetValue(entry.layerName, out Tilemap found))
            {
                entry.tilemap = found;
                resolved++;
                Debug.Log($"[TileMapManager] ✅ Auto-linked: layer '{entry.layerName}' → {GetHierarchyPath(found.transform)}");
            }
            else
            {
                Debug.LogWarning($"[TileMapManager] ⚠ Không tìm thấy Tilemap tên '{entry.layerName}' " +
                                 $"dưới '{tilemapRoot.name}'. Kiểm tra lại tên (phân biệt HOA/thường). " +
                                 $"Các Tilemap có sẵn: [{string.Join(", ", nameToTilemap.Keys)}]");
            }
        }

        Debug.Log($"[TileMapManager] Auto-detect hoàn tất: {resolved}/{layers.Length} layer(s) được resolve tự động.");
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
                        Debug.LogWarning($"[TileMapManager] Tile tại ({x},{y}) không có trong tiles[] " +
                                         $"của layer '{entry.layerName}'. Bỏ qua ô này.");
                        continue;
                    }

                    layerData.cells.Add(new TilemapCellData { x = x, y = y, tileIndex = tileIndex });
                }
            }

            // Ghi từng layer lên 1 path riêng: Users/{userId}/Maps/{mapName}/{layerName}
            string json = JsonUtility.ToJson(layerData);
            string path = BuildLayerPath(mapName, entry.layerName);
            DB.WriteDatabaseToPath(path, json);

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

            DB.ReadDatabaseToPath(path, (jsonData) =>
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
                        Debug.LogWarning($"[TileMapManager] tileIndex {cell.tileIndex} vượt quá mảng tiles[] " +
                                         $"của layer '{capturedEntry.layerName}'.");
                        continue;
                    }

                    TileBase tile = capturedEntry.tiles[cell.tileIndex];
                    if (tile != null)
                    {
                        capturedEntry.tilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), tile);
                        drawn++;

                        // --- ĐỒNG BỘ DATA VÀO CROP MANAGER ---
                        if (CropManager.Instance != null)
                        {
                            Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);
                            if (capturedEntry.layerName == "Ruong")
                            {
                                CropManager.Instance.SyncTilledState(pos);
                            }
                            else if (capturedEntry.layerName == "Seed")
                            {
                                CropManager.Instance.SyncSeedState(pos, cell.tileIndex);
                            }
                        }
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
        string uid = DB != null ? DB.UserId : "UNKNOWN_USER";
        // Trong trường hợp LoginScreen cấp UID vào PlayerPrefs hoặc FirebaseAuth
        if (Firebase.Auth.FirebaseAuth.DefaultInstance != null && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }
        return $"Users/{uid}/Maps/{targetMapName}/{layerName}";
    }

    // Trả về đường dẫn Hierarchy đầy đủ để dễ debug (ví dụ: Grid/Ruong)
    private string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null && t.parent != tilemapRoot)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    private bool ValidateReferences()
    {
        if (DB == null)
        {
            Debug.LogError("[TileMapManager] ❌ Chưa gán FiseBaseDatabaseManager và không tìm thấy Instance!");
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
            Debug.LogWarning($"[TileMapManager] Layer '{entry.layerName}': " +
                             "không tìm thấy Tilemap (auto-detect thất bại và chưa gán tay). Bỏ qua.");
            return false;
        }
        if (entry.tiles == null || entry.tiles.Length == 0)
        {
            Debug.LogWarning($"[TileMapManager] Layer '{entry.layerName}': mảng tiles[] rỗng. " +
                             "Hãy thêm TileBase assets vào Inspector. Bỏ qua.");
            return false;
        }
        return true;
    }
}
