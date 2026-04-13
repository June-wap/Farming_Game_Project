using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// CropManager: Quản lý toàn bộ ô trồng trọt trong game.
// Singleton DontDestroyOnLoad — tồn tại xuyên suốt tất cả Map.
//
// GROWTH STAGE SYSTEM:
//   Mỗi cây trải qua N stage trực quan trước khi thu hoạch.
//   Số stage = growthStageTiles.Length
//   Thời gian/stage = minutesToHarvest / (N - 1)
//
// Ví dụ với 4 tile stages, 120 phút thu hoạch:
//   Stage 0 (0-39 phút):   🌱 Mầm nhỏ
//   Stage 1 (40-79 phút):  🌿 Cây con
//   Stage 2 (80-119 phút): 🌾 Cây lớn
//   Stage 3 (120 phút):    🌻 Sẵn sàng → Harvestable
//
// Vòng đời: Empty → Tilled → Planted(stage 0→N) → Harvestable → Tilled
//           Planted → (không tưới 2 ngày) → Wilted
public class CropManager : MonoBehaviour
{
    // ─── SINGLETON ────────────────────────────────────────────────────────────
    public static CropManager Instance;

    // ─── TILE VISUALS ─────────────────────────────────────────────────────────
    [Header("Tile Visuals")]
    [Tooltip("Tile đất đã cuốc — hiển thị sau TillCell() và sau thu hoạch.")]
    public TileBase tilledTile;

    [Tooltip("Tile khi cây bị héo (không tưới nước 2 ngày).")]
    public TileBase wiltedTile;

    [Header("Growth Stage Tiles — kéo tile theo thứ tự phát triển")]
    [Tooltip("Mảng tile hình ảnh cây từ lúc mới gieo đến lúc thu hoạch.\n" +
             "Index 0 = mầm mới mọc, Index cuối = cây sẵn sàng thu hoạch.\n" +
             "Cần ít nhất 2 tile. Nhiều tile hơn = tăng trưởng mượt hơn.")]
    public TileBase[] growthStageTiles;

    // ─── CROP SETTINGS ────────────────────────────────────────────────────────
    [Header("Crop Settings")]
    [Tooltip("Tên cây trồng mặc định (hiện ra khi thu hoạch vào inventory).")]
    public string defaultCropName = "Flower";

    [Tooltip("Mô tả cây trồng mặc định.")]
    public string defaultCropDescription = "Hoa thu hoạch từ ruộng.";

    [Tooltip("Số phút ingame để cây đến ngưỡng thu hoạch (1 phút ingame = 1 giây thực).\n" +
             "Ví dụ: 12 = 12 giây thực ~ rất nhanh để dev/test.")]
    public int minutesToHarvest = 12;

    // ─── INTERNAL ─────────────────────────────────────────────────────────────
    // Dictionary<Vector3Int, CropData>: tra cứu theo vị trí ô trong O(1)
    private Dictionary<Vector3Int, CropData> _crops = new Dictionary<Vector3Int, CropData>();

    // Tilemap đang active — PlayerFarmController cập nhật mỗi khi vào scene mới
    private Tilemap _activeTilemap;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Được gọi từ PlayerFarmController.Start() mỗi khi vào Scene mới
    public void SetActiveTilemap(Tilemap tilemap)
    {
        _activeTilemap = tilemap;
        Debug.Log("[CropManager] Active Tilemap cập nhật: " + tilemap.name);
    }

    // ─── PUBLIC API ───────────────────────────────────────────────────────────

    // TillCell: Cuốc ô đất → trạng thái Tilled
    public void TillCell(Vector3Int cell)
    {
        if (!_crops.TryGetValue(cell, out CropData data))
        {
            data = new CropData(cell);
            _crops[cell] = data;
        }
        data.state     = CropState.Tilled;
        data.isWatered = false;
        SetTile(cell, tilledTile);
        Debug.Log($"[CropManager] Ô {cell} → Tilled");
    }

    // PlantSeed: Gieo hạt vào ô Tilled → khởi tạo Growth Stage
    // Trả về false nếu ô chưa cuốc
    public bool PlantSeed(Vector3Int cell)
    {
        if (!_crops.TryGetValue(cell, out CropData data) || data.state != CropState.Tilled)
        {
            Debug.Log($"[CropManager] Ô {cell} chưa được cuốc, không thể gieo hạt.");
            return false;
        }

        // Khởi tạo dữ liệu tăng trưởng
        data.state                = CropState.Planted;
        data.plantedTimeInMinutes = 0;
        data.timeToHarvestMinutes = minutesToHarvest;
        data.daysWithoutWater     = 0;
        data.currentStage         = 0;

        // Tính số stage và thời gian mỗi stage dựa trên mảng tile
        data.totalStages = (growthStageTiles != null && growthStageTiles.Length > 0)
                           ? growthStageTiles.Length
                           : 1;

        // minutesPerStage = tổng thời gian / (số stage - 1)
        // (không tính stage cuối vì stage cuối = Harvestable)
        data.minutesPerStage = data.totalStages > 1
                               ? minutesToHarvest / (data.totalStages - 1)
                               : minutesToHarvest;

        // Hiển thị tile stage đầu tiên (mầm mới mọc)
        SetTile(cell, GetStageTile(0));
        Debug.Log($"[CropManager] Ô {cell} → Planted | {data.totalStages} stages, {data.minutesPerStage} phút/stage");
        return true;
    }

    // WaterCell: Tưới nước cho ô Planted
    public bool WaterCell(Vector3Int cell)
    {
        if (_crops.TryGetValue(cell, out CropData data) && data.state == CropState.Planted)
        {
            data.isWatered        = true;
            data.daysWithoutWater = 0;
            Debug.Log($"[CropManager] Ô {cell} → Watered 💧");
            return true;
        }
        return false;
    }

    // IsHarvestable: Cây đã đủ tuổi thu hoạch chưa
    public bool IsHarvestable(Vector3Int cell)
    {
        return _crops.TryGetValue(cell, out CropData data) && data.state == CropState.Harvestable;
    }

    // HarvestCell: Thu hoạch → đặt ô về Tilled để trồng lại
    public void HarvestCell(Vector3Int cell)
    {
        if (_crops.TryGetValue(cell, out CropData data))
        {
            data.state                = CropState.Tilled;
            data.isWatered            = false;
            data.plantedTimeInMinutes = 0;
            data.currentStage         = 0;
            SetTile(cell, tilledTile);
            Debug.Log($"[CropManager] Ô {cell} → Harvested ✅ → Tilled");
        }
    }

    // ClearCell: Xóa hoàn toàn dữ liệu ô (dùng khi nhấn [M] debug)
    public void ClearCell(Vector3Int cell)
    {
        _crops.Remove(cell);
        SetTile(cell, null);
        Debug.Log($"[CropManager] Ô {cell} → Cleared");
    }

    // GetCropName / GetCropDescription: Hỗ trợ mở rộng nhiều loại cây sau này
    public string GetCropName(Vector3Int cell)        => defaultCropName;
    public string GetCropDescription(Vector3Int cell) => defaultCropDescription;

    // GetProgress: Trả về tiến độ tăng trưởng 0.0→1.0 (dùng cho UI Progress Bar)
    public float GetGrowthProgress(Vector3Int cell)
    {
        if (!_crops.TryGetValue(cell, out CropData data)) return 0f;
        if (data.state == CropState.Harvestable) return 1f;
        if (data.timeToHarvestMinutes <= 0) return 0f;
        return Mathf.Clamp01((float)data.plantedTimeInMinutes / data.timeToHarvestMinutes);
    }

    // ─── GROWTH TICK (mỗi phút ingame = 1 giây thực) ─────────────────────────

    // Gọi từ TimeManager.OnGameMinuteTick — khi cây được tưới thì tăng tuổi
    // và cập nhật tile theo stage hiện tại.
    public void OnGameMinuteTick()
    {
        foreach (var pair in _crops)
        {
            CropData   data = pair.Value;
            Vector3Int cell = pair.Key;

            if (data.state != CropState.Planted) continue;
            if (!data.isWatered) continue; // Không được tưới → không lớn phút này

            // Tăng tuổi cây 1 phút
            data.plantedTimeInMinutes++;

            // Tính stage mới dựa trên thời gian tích lũy
            int newStage = (data.minutesPerStage > 0)
                           ? Mathf.Clamp(data.plantedTimeInMinutes / data.minutesPerStage, 0, data.totalStages - 1)
                           : data.totalStages - 1;

            // Cập nhật tile nếu stage thay đổi
            if (newStage != data.currentStage)
            {
                data.currentStage = newStage;
                SetTile(cell, GetStageTile(newStage));
                Debug.Log($"[CropManager] Ô {cell} → Stage {newStage}/{data.totalStages - 1} 🌱");
            }

            // Đủ thời gian → cây trưởng thành, sẵn sàng thu hoạch
            if (data.plantedTimeInMinutes >= data.timeToHarvestMinutes)
            {
                data.state = CropState.Harvestable;
                // Tile cuối trong growthStageTiles = hình cây có quả
                SetTile(cell, GetStageTile(data.totalStages - 1));
                Debug.Log($"[CropManager] Ô {cell} → HARVESTABLE! 🌻 ({data.plantedTimeInMinutes} phút)");
            }

            // Reset cờ tưới — cần tưới lại cho phút tiếp theo
            data.isWatered = false;
        }
    }

    // ─── DAY TICK (mỗi ngày game = 24 phút ingame = 24 giây thực) ────────────

    // Gọi từ TimeManager.OnNewDayStarted — kiểm tra cây nào chưa được tưới trong ngày
    public void OnNewDayStarted()
    {
        foreach (var pair in _crops)
        {
            CropData   data = pair.Value;
            Vector3Int cell = pair.Key;

            if (data.state != CropState.Planted) continue;

            if (!data.isWatered)
            {
                data.daysWithoutWater++;
                Debug.Log($"[CropManager] Ô {cell} → Khô ngày thứ {data.daysWithoutWater} 🌵");

                if (data.daysWithoutWater >= 2)
                {
                    data.state = CropState.Wilted;
                    SetTile(cell, wiltedTile);
                    Debug.LogWarning($"[CropManager] Ô {cell} → WILTED! 💀 ({data.daysWithoutWater} ngày)");
                }
            }
            else
            {
                // Đã được tưới → reset bộ đếm khô hạn
                data.daysWithoutWater = 0;
            }
        }

        Debug.Log($"[CropManager] Ngày mới: kiểm tra {_crops.Count} ô đất.");
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    // Lấy tile tương ứng với stage index
    // Trả về null nếu mảng trống hoặc index out of bounds
    private TileBase GetStageTile(int stageIndex)
    {
        if (growthStageTiles == null || growthStageTiles.Length == 0) return null;
        int safeIndex = Mathf.Clamp(stageIndex, 0, growthStageTiles.Length - 1);
        return growthStageTiles[safeIndex];
    }

    // Đặt tile lên Tilemap — an toàn, kiểm tra null trước
    private void SetTile(Vector3Int cell, TileBase tile)
    {
        if (_activeTilemap != null)
            _activeTilemap.SetTile(cell, tile);
    }
}
