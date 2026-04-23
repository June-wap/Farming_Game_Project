using UnityEngine;

// CropState: Máy trạng thái của 1 ô đất trong game nông trại.
//
// Luồng bình thường:
//   Empty → Tilled → Planted → (tưới mỗi ngày, tăng stage) → Harvestable → Tilled
//
// Luồng khô hạn:
//   Planted → (không tưới 2 ngày) → Wilted → (nhổ bỏ [M]) → Tilled
public enum CropState
{
    Empty,       // Ô đất trống — chưa làm gì
    Tilled,      // Đã cuốc đất — sẵn sàng nhận hạt giống
    Watered,     // Dự phòng (đất tưới chưa gieo)
    Planted,     // Đã gieo hạt — đang lớn lên theo stage
    Harvestable, // Đã trưởng thành — sẵn sàng thu hoạch
    Wilted       // Héo vì thiếu nước — cần nhổ bỏ
}

// CropData: Dữ liệu của MỘT ô đất.
// Không kế thừa MonoBehaviour — là Data Class thuần túy.
// CropManager dùng Dictionary<Vector3Int, CropData> để tra cứu theo tọa độ O(1).
[System.Serializable]
public class CropData
{
    // ─── VỊ TRÍ & LOẠI CÂY ──────────────────────────────────────────────────────────────
    public Vector3Int cellPosition; // Tọa độ ô trên Tilemap
    public string cropName;         // Tên nông sản sẽ thu hoạch được (vd: Lua, Ngo, CaChua)

    // ─── TRẠNG THÁI ──────────────────────────────────────────────────────────
    public CropState state;         // Trạng thái hiện tại của ô đất
    public bool isWatered = false;  // Có được tưới trong chu kỳ hiện tại không

    // ─── TĂNG TRƯỞNG THEO STAGE ──────────────────────────────────────────────
    // Growth Stage System: cây phát triển qua nhiều stage trực quan.
    // Stage 0 = mầm mới mọc, Stage (totalStages-1) = sẵn sàng thu hoạch.
    //
    // Ví dụ với growthStageTiles.Length = 4 và timeToHarvest = 120 phút:
    //   minutesPerStage = 120 / (4-1) = 40 phút/stage
    //   Stage 0 [0-39 phút]:   tile mầm nhỏ
    //   Stage 1 [40-79 phút]:  tile cây con
    //   Stage 2 [80-119 phút]: tile cây lớn
    //   Stage 3 [120 phút]:    → chuyển Harvestable
    public int currentStage       = 0;   // Stage hiện tại (0 = mới gieo)
    public int totalStages        = 3;   // Tổng số stage (gán bởi CropManager)
    public int minutesPerStage    = 40;  // Phút ingame để qua 1 stage

    // ─── ĐẾM THỜI GIAN ───────────────────────────────────────────────────────
    public int plantedTimeInMinutes  = 0;   // Phút ingame tích lũy kể từ khi gieo hạt
    public int timeToHarvestMinutes  = 120; // Ngưỡng thu hoạch (phút ingame)
    public int daysWithoutWater      = 0;   // Đếm ngày không được tưới → héo

    // Constructor
    public CropData(Vector3Int pos)
    {
        cellPosition = pos;
        state        = CropState.Empty;
        isWatered    = false;
        cropName     = "";
    }
}
