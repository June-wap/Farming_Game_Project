using UnityEngine;

// CropState: Enum định nghĩa 6 trạng thái của một ô đất trong game nông trại.
// State Machine (Máy trạng thái) giúp kiểm soát rõ ràng ô đất đang ở giai đoạn nào.
//
// Luồng chuyển trạng thái bình thường:
//   Empty → Tilled → Planted → Harvestable → (thu hoạch) → Tilled
//
// Luồng khi bị khô hạn:
//   Planted → (không tưới 2 ngày) → Wilted → (nhổ bỏ) → Tilled
public enum CropState
{
    Empty,       // Ô đất trống — chưa làm gì (mặc định ban đầu)
    Tilled,      // Đã cuốc đất — đất tơi xốp, sẵn sàng nhận hạt giống
    Watered,     // Đất đã tưới nhưng chưa gieo hạt (trạng thái dự phòng)
    Planted,     // Đã gieo hạt — đang chờ lớn (cần tưới mỗi ngày)
    Harvestable, // Cây đã trưởng thành — sẵn sàng thu hoạch
    Wilted       // Cây bị héo vì thiếu nước — không thu hoạch được, cần nhổ bỏ
}

// CropData: Class lưu trữ toàn bộ thông tin của MỘT ô đất.
// Không kế thừa MonoBehaviour → là Data Class thuần túy, không gắn lên GameObject.
// CropManager quản lý một Dictionary<Vector3Int, CropData> để tra cứu theo tọa độ.
[System.Serializable] // Cho phép Unity hiển thị trong Inspector và hỗ trợ JSON serialization
public class CropData
{
    // Tọa độ ô trên Tilemap — dùng làm khóa (key) trong Dictionary
    // Vector3Int: x = cột, y = hàng, z = thường = 0 với map 2D
    public Vector3Int cellPosition;

    // Trạng thái hiện tại của ô đất — xem enum CropState ở trên
    public CropState state;
    
    // Cờ đánh dấu ô đất có đang được tưới nước không
    // true → cây sẽ lớn lên trong chu kỳ tiếp theo
    // false → cây sẽ bắt đầu đếm ngày khô hạn
    public bool isWatered = false;

    // Bộ đếm phút ingame kể từ khi gieo hạt
    // Cộng 1 mỗi phút ingame (= 1 giây thực) khi cây được tưới nước
    // Khi đạt timeToHarvestMinutes → chuyển sang Harvestable
    public int plantedTimeInMinutes = 0; 

    // Ngưỡng thời gian để thu hoạch (tính bằng phút ingame)
    // Mặc định 120 phút ingame = 120 giây thực ≈ 2 phút chơi
    public int timeToHarvestMinutes = 120;
    
    // Đếm số ngày (mỗi chu kỳ ngày = N phút ingame) mà não thiếu nước
    // Khi daysWithoutWater >= 2 → cây chuyển sang Wilted
    public int daysWithoutWater = 0; 

    // Constructor: tạo 1 ô đất mới tại tọa độ pos, trạng thái mặc định là Empty
    public CropData(Vector3Int pos)
    {
        cellPosition = pos;
        state = CropState.Empty;
        isWatered = false;
    }
}
