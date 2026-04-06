# 🧠 Project Memory: [Ultimate Farm Master]

📍 **Current Progress (Cập nhật đến 06/04/2026)**
- **World Building**: Hoàn thành 6 Map tiếng Anh: Home, Forest, Graveyard, TransitionZone, TownCenter, Village.
- **Core Mechanics**: 
  - Di chuyển nhân vật (Animation đầy đủ). Hệ thống Grid Snapping 32x32 pixels.
  - Tách biệt logic quản lý Nông trại và Player thông qua các cấu trúc Managers (TimeManager, CropManager, ToolManager, DataPersistenceManager).

---

## 🚀 CÁC MODULE ĐÃ TRIỂN KHAI VÀ GIẢI NGHĨA

### 1. Hệ thống Thời Gian & Quản Lý Cây Trồng (Module A)
- **`TimeManager.cs`**: Tạo khối động cơ Singleton chạy ngầm (DontDestroyOnLoad). Đếm giờ theo tỷ lệ **1 giây thực = 1 phút ingame**.
  - 👉 **Vì sao chọn cách này?**: Gỡ bỏ hàm `Update()` nặng nề trên từng cây trồng đơn lẻ. Tối ưu hóa 100% tài nguyên CPU do cây cỏ chỉ lớn lên thông qua `Event Action` cứ mỗi phút trôi qua.
- **`CropData.cs`**: Khai báo 6 trạng thái State Machine chuẩn (`Empty, Tilled, Watered, Planted, Harvestable, Wilted`). 
  - 👉 **Logic sinh tồn**: Tích hợp biến cờ `isWatered` (có nước). Cây chỉ được cộng giờ trưởng thành (đủ 120 Tick ~ 2 phút thực để lớn) KHI nó có nước. Nếu trải qua **48 giây thực tế** (cây chạy đếm time héo úa bằng `Time.deltaTime`) mà không được tưới, cây sẽ bị chết héo (`Wilted`).
- **`CropManager.cs`**: Sử dụng `Dictionary<Vector3Int, CropData>` để định vị các chậu cây siêu nhanh dựa trên điểm toạ độ 3D lưới của Tilemap.

### 2. Hệ thống Công cụ Mũi Nhọn (Module B)
- **`IvenItems.cs`**: Đại tu thư viện Items bằng Enum `ItemCategory` (Nông cụ, Hạt giống, Hàng hoá) và định danh ngầm `string toolId`.
  - 👉 **Vì sao lại dùng thẻ ID ngầm?**: Chống "Code cứng" xét theo Tên món đồ. Tránh tai nạn sập game vì check chữ "Bình Tưới", lỡ sau này muốn ra mắt đa ngôn ngữ đổi tên vật phẩm thành "Watering Can" game vẫn xử lý mượt.
- **`ToolManager.cs`**: Nắm chốt chặng "Đang trang bị đồ gì trong tay?".
  - Gắn event vào Click của Inventory Card (`CellItemData.cs`), click vô túi đồ là đẩy món hàng lên người.
- **`PlayerFarmControler.cs`**: Áp dụng cơ chế **Logic Gating** (Khóa nút):
  - Bấm `[C]` để cuốc -> Check coi trang bị vũ khí `toolId == hoe` không, nếu không thì nghỉ đào.
  - Bấm `[V]` để gieo hạt -> Check coi có cầm `ItemCategory.Seed` không. Gieo xong gọi toolManager hàm `ConsumeEquippedItem()` để trừ mất 1 bịch hạt giống trong balo.
  - Bấm `[F]` -> Chỉ được xịt nước khi trang bị `water_can`.

### 3. Đồng Bộ Trạng Thái Đám Mây (Module D)
- **`GameSaveData.cs` (Wrapper)**: Biến mảng lớn Dictionary và hệ thống Túi Đồ thành mảng List có thể hiểu được bằng hàm nén `JsonUtility`.
  - 👉 **Vì sao lại tạo Data Wrapper?**: Unity và Firebase không thể Serialize trực tiếp được C# Hashmap / Dictionary gốc. Vứt chúng vô List bọc lại là cách truyền dữ liệu chuẩn chỉ nhất.
- **`DataPersistenceManager.cs`**: Trực tiếp nắm `RootReference` của Firebase Reatime DB. Bắn toàn bộ file Save JSON của từng khu vực vào mảng Node `/Users/DefaultPlayer/Maps/[Tên Map]`. Chạy luồng Back-ground (`ContinueWithOnMainThread`) để không bị khựng hình game lúc đang Push Data/Tải Data.
- **Tự động lưu (`SceneController.cs`)**: Khi đặt chân chui vô cánh cổng dịch chuyển, Map hiện tại lập tức Back-up lên mây.
- **Tự động tải (`PlayerFarmControler.cs`)**: Vừa đẻ ra ở map mới, Player lập tức gửi mã gọi cửa, load dữ liệu Firebase đổ đè đồ họa vào Tilemap hiện hữu.

### 4. Hệ Thống Sự Kiện & Vòng Quay Mùa Màng (Module E)
- **`GameEventManager.cs`**: Lõi kết nối mạng nhện (Event Bus) của toàn cõi. Lắng nghe và Bắn các tín hiệu vô tuyến ngầm (`OnItemCollected`, `OnNewDayStarted`).
- **`TimeSystem.cs`**: Cỗ máy đếm lịch Độc lập. Đếm qua **24 phút đời thực** là chuyển sang 1 Ngày Game Mới. Nó lập tức kích hoạt loa `OnNewDayStarted`, đồng thời tải và ghi (Sync) chỉ số `currentDay` lên cục mây Firebase.
- **`HarvestWheelManager.cs`**: Minigame Gacha Vòng Quay đánh bạc đốt tài nguyên. Yêu cầu Item mồi (Ví dụ: Hoa Nghĩa Trang) để làm thẻ Ticket. Hàm xoay Wheel xài lệnh Toán `Mathf.Lerp` và công thức Ease-out (Phanh từ từ) tạo rung cảm thị giác. Xoay trúng lỏi nhét đồ thụ động bằng Sóng Radio `TriggerItemCollected` (Balo tự nghe và tự cất đồ luôn khỏi cần dây dưa kết nối code 2 màn hình).
- **Phẫu thuật Thảm Hoạ Cây Khô (`CropManager.cs`)**: Phế bỏ thuật toán Héo Củ Chuối trước đây (chạy vòng `Update()` đo đạc `Time.deltaTime` kéo FPS giảm). Đổi qua Event-driven: Cây cối đều tắt não bộ đi ngủ, tới khi Mốc Ngày Mới đập cửa thì Cây mới bật dậy xét coi bị Khát bao ngày. Hạn hán đủ 2 ngày (`daysWithoutWater == 2`) -> Nạn chết héo bùng nổ `Wilted`.

---

## 🛠 TECHNICAL SOLUTIONS (Các lỗi đã khắc phục rốt ráo)
- **Bug / Lỗi nguy cơ**: Hệ thống Đám mây có lúc load dữ liệu về nhưng Graphic báo mảng Map trả về đối tượng `Null`. Nguyên do là hệ thống Singleton màng ngầm (`CropManager`) không thể biết chính xác bạn đang ở khu nào để mà đi tìm đúng cái Tilemap đất nền (`tm_Ground`). 
- **Giải pháp xử trí**: Loại bỏ việc "Tự đi dò tìm TileMap". Mình yêu cầu `PlayerFarmControler` ở Map đang đứng làm nhân viên vận chuyển, gọi tên DataPersistenceManager bắt nạt nó "Tải Data Firebase xuống đi, ta đứng ở Scene hiện tại tao sẽ đưa cái Tilemap của ta vào tham số (Callback) cho ngươi dán gạch Graphic".

- **Trở ngại: Tester bị lock chức năng khi chưa có Inventory Tool**: Khi dev tính năng gieo hạt, nếu bắt buộc phải thiết kế xong Prefabs Tool để test thì cực kỳ làm chậm tiến độ dò bug vòng đời cây `120s`.
- **Giải pháp xử trí**: Mã hóa tính năng "Cửa sau" cực mạnh (Cheat Code) - Bổ sung biến cờ `bypassToolRequirement = true` công khai bên ngoài cửa sổ Inspector của `PlayerFarmControler`. Nếu tick sáng, hệ thống ngầm cho phép gọi chéo Data (Bấm C cuốc đất bằng không khí, bấm V đẻ ra mầm) mà không rớt game. Khi Release Game chỉ việc tắt Tick False đi.

---

## 📅 NEXT GOALS (Trạm dừng tiếp theo)
* **Shop System**: Xây dựng NPC hoặc bảng mua bán lấy Hoa Nghĩa Trang bán ra lấy tiền mua hạt giống.
* **UI Toolbar**: Tạo 1 Ô Toolbar bé ở góc màn hình game nhận sự kiện từ `ToolManager.OnItemEquipped` nhằm thông báo cho Player biết "Ngài đang cầm cái Cuốc hay Đang cầm hạt giống?".

💡 **Cách sử dụng file này hiệu quả:**
Mỗi khi bắt đầu một buổi làm việc mới với AI, hãy copy dán nội dung `Memory.md` này và chèn vào câu lệnh: *"Đây là trạng thái hiện tại của dự án tôi, hãy đọc để nắm bắt ngữ cảnh trước khi làm tính năng mới."*