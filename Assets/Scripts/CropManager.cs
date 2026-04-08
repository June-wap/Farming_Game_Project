using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// CropManager: Quản lý toàn bộ ô trồng trọt trong game.
// Là Singleton DontDestroyOnLoad — tồn tại xuyên suốt tất cả 6 Map.
//
// Tại sao dùng Dictionary thay vì List?
// → Dictionary cho phép truy xuất dữ liệu theo tọa độ ô (Vector3Int) trong O(1)
//   thay vì phải duyệt qua toàn bộ List để tìm ô phù hợp.
//
// Vòng đời 1 ô đất:
//   Empty → Tilled → Planted → (tưới nước mỗi ngày) → Harvestable → (thu hoạch) → Tilled
//                             ↘ (khô 2 ngày) → Wilted
public class CropManager : MonoBehaviour
{
    // ─── TILE GRAPHICS ─────────────────────────────────────────────────────────
    // Các Tile dùng để thay đổi hình ảnh trên Tilemap khi trạng thái ô thay đổi
    [Header("Tile Visuals - Kéo các Tile vào đây")]
    public TileBase tilledTile;          // Hình ảnh đất đã cuốc (đất tơi xốp)
    public TileBase plantedTile;         // Hình ảnh khi đã gieo hạt (mầm nhỏ)
    public TileBase harvestableTile;     // Hình ảnh khi cây trưởng thành (quả lớn)
    public TileBase wiltedTile;          // Hình ảnh khi cây héo (màu vàng úa)

    // ─── CROP SETTINGS ─────────────────────────────────────────────────────────
    [Header("Crop Settings")]
    [Tooltip("Tên cây trồng mặc định.")]
    public string defaultCropName = "Flower";

    [Tooltip("Mô tả cây trồng mặc định.")]
    public string defaultCropDescription = "Hoa thu hoạch từ ruộng.";

    [Tooltip("Số phút ingame để cây đến ngưỡng thu hoạch (1 phút ingame = 1 giây thực).")]
    public int minutesToHarvest = 120;

    // ─── INTERNAL ──────────────────────────────────────────────────────────────
    // Từ điển quản lý tất cả ô đất đang hoạt động
    // Key   = vị trí ô trên Tilemap (ví dụ: x=3, y=5, z=0)
    // Value = toàn bộ thông tin của ô đó (trạng thái, có nước không, tuổi cây...)
    private Dictionary<Vector3Int, CropData> _crops = new Dictionary<Vector3Int, CropData>();

    // Tilemap của Scene hiện tại — dùng để thay đổi hình ảnh tile khi trạng thái cây thay đổi
    // PlayerFarmController sẽ truyền Tilemap vào qua SetActiveTilemap()
    private Tilemap _activeTilemap;

    // ─── SINGLETON ─────────────────────────────────────────────────────────────
    public static CropManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển Map
        }
        else
        {
            Destroy(gameObject); // Xóa bản trùng
        }
    }

    // SetActiveTilemap: Được gọi từ PlayerFarmController.Start() mỗi khi vào Scene mới.
    // Vì CropManager là Singleton tồn tại xuyên Map, nó cần biết Tilemap nào đang active
    // để vẽ tile đúng chỗ.
    public void SetActiveTilemap(Tilemap tilemap)
    {
        _activeTilemap = tilemap;
    }

    // ─── PUBLIC API ─────────────────────────────────────────────────────────────

    // TillCell: Cuốc ô đất — chuyển trạng thái sang Tilled và hiển thị tile đất tơi
    public void TillCell(Vector3Int cell)
    {
        // Nếu ô chưa có trong từ điển → tạo mới
        if (!_crops.TryGetValue(cell, out CropData data))
        {
            data = new CropData(cell);
            _crops[cell] = data;
        }
        data.state = CropState.Tilled;
        data.isWatered = false;
        SetTile(cell, tilledTile); // Thay đổi hình ảnh tile trên bản đồ
        Debug.Log($"[CropManager] Ô {cell} → Tilled");
    }

    // PlantSeed: Gieo hạt vào ô đã cuốc (phải là Tilled mới gieo được)
    // Trả về true nếu thành công, false nếu ô chưa được cuốc
    public bool PlantSeed(Vector3Int cell)
    {
        // Kiểm tra: ô phải tồn tại trong từ điển VÀ đang ở trạng thái Tilled
        if (!_crops.TryGetValue(cell, out CropData data) || data.state != CropState.Tilled)
        {
            Debug.Log($"[CropManager] Ô {cell} chưa được cuốc, không thể gieo hạt.");
            return false;
        }
        data.state = CropState.Planted;
        data.plantedTimeInMinutes = 0;           // Reset bộ đếm tuổi cây
        data.timeToHarvestMinutes = minutesToHarvest; // Gán thời gian cần để thu hoạch
        data.daysWithoutWater = 0;               // Reset bộ đếm ngày khô hạn
        SetTile(cell, plantedTile);
        Debug.Log($"[CropManager] Ô {cell} → Planted");
        return true;
    }

    // WaterCell: Tưới nước cho ô đang có cây (Planted)
    // Trả về true nếu tưới thành công
    public bool WaterCell(Vector3Int cell)
    {
        // Chỉ tưới được nếu ô đang ở trạng thái Planted
        if (_crops.TryGetValue(cell, out CropData data) && data.state == CropState.Planted)
        {
            data.isWatered = true;        // Đánh dấu đã tưới
            data.daysWithoutWater = 0;    // Reset bộ đếm ngày khô → không bị héo nữa
            Debug.Log($"[CropManager] Ô {cell} → Watered");
            return true;
        }
        return false;
    }

    // IsHarvestable: Kiểm tra cây tại ô có thể thu hoạch chưa
    public bool IsHarvestable(Vector3Int cell)
    {
        return _crops.TryGetValue(cell, out CropData data) && data.state == CropState.Harvestable;
    }

    // GetCropName / GetCropDescription: Lấy tên và mô tả item sẽ rớt ra khi thu hoạch
    // Sau này có thể mở rộng: đọc từ ScriptableObject để hỗ trợ nhiều loại cây khác nhau
    public string GetCropName(Vector3Int cell) => defaultCropName;
    public string GetCropDescription(Vector3Int cell) => defaultCropDescription;

    // HarvestCell: Thu hoạch cây — lấy sản phẩm rồi đặt ô về Tilled để trồng lại
    public void HarvestCell(Vector3Int cell)
    {
        if (_crops.TryGetValue(cell, out CropData data))
        {
            data.state = CropState.Tilled;
            data.isWatered = false;
            data.plantedTimeInMinutes = 0;
            SetTile(cell, tilledTile);     // Hiển thị lại đất tơi
            Debug.Log($"[CropManager] Ô {cell} → Harvested, đặt lại Tilled");
        }
    }

    // ClearCell: Xóa hoàn toàn dữ liệu và tile (dùng khi nhấn [M] để dọn ô)
    public void ClearCell(Vector3Int cell)
    {
        _crops.Remove(cell);     // Xóa khỏi từ điển
        SetTile(cell, null);     // Xóa tile trên bản đồ (trở về tile gốc)
        Debug.Log($"[CropManager] Ô {cell} → Cleared");
    }

    // ─── GROWTH TICK ────────────────────────────────────────────────────────────

    // OnGameMinuteTick: Gọi từ TimeManager mỗi phút ingame để cập nhật trạng thái tất cả cây.
    // Không chạy trong Update() để tránh kiểm tra tất cả ô đất 60 lần/giây!
    public void OnGameMinuteTick()
    {
        foreach (var pair in _crops)
        {
            CropData data = pair.Value;
            Vector3Int cell = pair.Key;

            // Chỉ xử lý ô đang ở trạng thái Planted
            if (data.state == CropState.Planted)
            {
                if (data.isWatered)
                {
                    // Cây được tưới nước → tăng tuổi lên 1 phút
                    data.plantedTimeInMinutes++;
                    data.isWatered = false; // Cần tưới lại cho chu kỳ tiếp theo

                    // Kiểm tra xem cây đã đủ tuổi để thu hoạch chưa
                    if (data.plantedTimeInMinutes >= data.timeToHarvestMinutes)
                    {
                        data.state = CropState.Harvestable;
                        SetTile(cell, harvestableTile); // Đổi hình ảnh sang cây trưởng thành
                        Debug.Log($"[CropManager] Ô {cell} → Harvestable!");
                    }
                }
                else
                {
                    // Cây không được tưới → tăng đếm ngày khô hạn
                    data.daysWithoutWater++;

                    // Nếu khô 2 ngày liên tiếp → cây chết héo
                    if (data.daysWithoutWater >= 2)
                    {
                        data.state = CropState.Wilted;
                        SetTile(cell, wiltedTile); // Đổi hình ảnh sang cây héo
                        Debug.LogWarning($"[CropManager] Ô {cell} → Wilted (khô héo)!");
                    }
                }
            }
        }
    }

    // ─── HELPER ─────────────────────────────────────────────────────────────────

    // SetTile: Hàm tiện ích để thay đổi tile trên Tilemap một cách an toàn
    // Kiểm tra null trước để tránh lỗi khi _activeTilemap chưa được gán
    private void SetTile(Vector3Int cell, TileBase tile)
    {
        if (_activeTilemap != null)
            _activeTilemap.SetTile(cell, tile);
    }
}
