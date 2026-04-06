# 🏆 BÁCH KHOA TOÀN THƯ MÃ NGUỒN (ULTIMATE SCRIPT ENCYCLOPEDIA)

Dưới đây là sơ đồ và lời giải thích **tất cả** các tệp lệnh (Scripts) có mặt trong dự án Nông trại của bạn. Việc phân rã từng file sẽ giúp bạn nắm rõ mạch máu của trò chơi đang chảy như thế nào.

---

## 🗺️ PHẦN 1: HỆ THỐNG ĐIỀU HƯỚNG VÀ DI CHUYỂN
*Là xương sống giúp máy quay, nhân vật và NPC (động vật) hoạt động vặn xoắn với nhau trong không gian 2D Grid.*

### 1. `playercontroler.cs` & `Player_Movment_Mouse.cs`
- **Vai trò**: Linh hồn của nhân vật. Nhận ngón tay/chuột phím của người dùng để điều khiển Avatar di chuyển.
- **Logic**: Sử dụng hệ thống `Vector2` và `Rigidbody2D` để thay đổi Vận tốc (Velocity) mượt mà mà không xuyên tường. Trong code này chứa các lệnh gọi Component `Animator`, giúp đổi hình dạng Nhân vật từ Đứng im (Idle) sang Đang thả bước (Walk) hoặc đang cuốc đất dựa theo tọa độ (X/Y).
- **Tip Học Tập**: Animation di chuyển 2D luôn cấu thành từ 2 biến trục tung và hoành. Gửi 2 biến này vào Animator là nó sẽ tự chuyển frame ảnh.

### 2. `AnimalWander.cs`
- **Vai trò**: Cung cấp "Trí tuệ nhân tạo (A.I) Nông dân" cho mấy con Bò, Gà, Cừu trên bản đồ.
- **Logic**: Thường sử dụng `Random.Range` trộn với `Time.deltaTime`. Cứ cách 3 - 5 giây, con vật tự bốc ngẫu nhiên một tọa độ quanh nó, rồi kích hoạt lệnh `transform.Translate()` bò tới đó một cách từ từ đi dạo.
- **Tip Học Tập**: Đi kèm `AnimalWander.cs` bao giờ cũng có biến đếm lùi `waitTime`. Nếu `waitTime > 0` thì con động vật Đứng im (Idle), đếm lùi về 0 thì Chọn hướng đi mới -> Khôi phục lại waitTime.

### 3. `SceneController.cs` & `mapexit.cs`
- **Vai trò**: Hệ thống Không Gian Dịch Chuyển (Portal). Đưa người chơi đi từ Map này (Village) qua Map kia (Graveyard).
- **Logic của `mapexit.cs`**: Đặt trên cánh cổng Vô hình ở rìa Map. Nó dùng hàm bắt va chạm Vật Lý `OnTriggerEnter2D`. Khi gót chân Player chạm vào Cổng, nó đọc chuỗi (string) tên Map muốn tới và gọi `SceneController.LoadScene()`.
- **Logic của `SceneController`**: Xóa đi sân khấu kịch cũ, nạp sân khấu kịch mới. Đặc biệt, ta đã tích hợp việc Save toàn bộ Tilemap Nông trại lên Đám Mây MỖI LẦN LoadScene! Tránh mất data.

---

## 👩‍🌾 PHẦN 2: HỆ THỐNG KIẾN TRÚC LÕI (THE MANAGERS)
*Những tệp lệnh chạy ngầm xuyên suốt, không có hình hài hiện lên màn hình nhưng quyết định toàn bộ luật chơi.*

### 1. `TimeManager.cs` (Đồng hồ vạn năng)
- **Vai trò**: Máy đếm nhịp tịnh tiến thời gian Nông trại.
- **Logic**: 
  Nó là một *Singleton* (`public static Instance`), 1 class làm vua trên cả trò chơi. Nó dùng `Time.deltaTime` chạy đếm dây. Cứ đủ `1f` thì xuất bản sự kiện (Action Event) ra toàn cõi.
- **Học tập**: Nếu xài `Update` ở TimeManager thì tối ưu, nếu dùng nó ở *từng cái cây* thì game lag thấu trời (Vì gọi 1 con bò đếm giờ đỡ hơn 10,000 cái cây đếm giờ).

### 2. `CropManager.cs` & `CropData.cs` (Kỹ sư nông nghiệp)
- **`CropData.cs` (Thực thể Giống loài)**: Không kế thừa MonoBavior (không treo lên quái, không Update), nó chỉ là một tờ Giấy Khai Sinh. Nó đóng gói trạng thái (State) của 1 ô đất gồm: Tọa độ cỏ, Khát nước không (`isWatered`), và Phút sống bao nhiêu.
- **`CropManager.cs`**: Nắm cái Cuốn Danh Bạ bách khoa `Dictionary<ToạĐộ, GiấyKhaiSinh>`. Khi Đồng hồ `TimeManager` tíc tắc 1 nhịp, `CropManager` lôi danh bạ ra cộng hết tuổi thọ cây lên +1. 
  - **Vì sao xài Dictionary?**: Dictionary như một cái rổ chia khoang. Khi người chơi giậm nát 1 ô đất tại tọa độ X:3, Y:5, hệ thống sẽ chọc đúng ô `[3,5]` trên Dictionary lôi cái cây lên cắt tiết với tốc độ O(1) (tức là tốc độ xử lý tính theo Mili giây nháy mắt).

### 3. `DataPersistenceManager.cs` & `GameSaveData.cs` (Xương sống Lưu Trữ)
- **`GameSaveData.cs` (Vỏ kẹo JSON)**: Nếu bạn ném mớ hổ lốn Unity vào Firebase, Firebase sẽ nhổ ra (Lỗi Error Crash). Phải xé lẻ Dictionary ra thành các List thông số (Class `MapSaveData`), rồi nén nhọ lại bằng `JsonUtility`.
- **`DataPersistenceManager.cs`**: 
  - Lấy địa chỉ IP Cây thư mục Firebase (`dbReference`). 
  - Dùng kiến trúc Mạng Giao Cầu (Bất đồng bộ - `ContinueWithOnMainThread`). Hàm này giúp máy chủ cứ từ từ kéo ảnh kéo số về, trong khi đó nhân vật player trong game vẫn có thể múa kiếm di chuyển nhảy lên nhảy xuống không bị dật lắc (Freezing Screen).

### 4. `PlayerFarmControler.cs` (Tay Cào Đất Cừ Khôi)
- **Vai trò**: Đọc lệnh trên ngón tay người chơi (A, S, D, C, V, F).
- **Học tập Grid Snap**: Game Pixel 32x32 không dùng tọa độ trôi dạt Float. Hàm `tm_Ground.WorldToCell` sẽ ép tọa độ Float (số lẻ 2.3) thành con số chẵn tắp (2, 2). Giúp mảnh cày trúng chính giữa rãnh đất như Harvest Moon, không mọc lệch.
- **Bypass Cờ Tính Năng**: Cờ `bypassToolRequirement` cho thấy Tư duy DEV - Code không lồi lõm chặn đường sống của Kỹ sư Kiểm thử phần mềm (Testing). Khóa Gating (Điều kiện vật phẩm) có thể nhả ra cho DEV làm việc dễ dàng bằng 1 click rỗng!.

### 5. Khí Mạch Tài Chính (`PlayerEconomy.cs`)
- **Vai trò**: Ngân hàng trung ương (Singleton). Lấp vào lỗ hổng thiết kế cũ bằng két sắt kỹ thuật số `currentMoney`.
- **Học tập Tối ưu UI (Event-Driven UI)**: Làm sao để màn hình UI hiện Số Vàng lập tức mỗi lúc ta mua đồ? Người "code thủ" gà mờ sẽ viết hàm `Update()` trên UI bắt nó soi ví tiền mỗi 60FPS -> Máy đơ lag. Ở đây mình dùng Tín hiệu Loa phường `Action<int> OnMoneyChanged`. 
  Cục Text UI chỉ việc Đăng ký làm thính giả. Khi Rút/Gửi tiền lách cách, PlayerEconomy bấm nút `Invoke()`, lập tức UI nhận tin vẽ lại chữ rồi tắt máy Tối Ưu 100% vĩnh viễn!.
- **Bảo chứng chống Cheat**: Hàm nạp Vàng vào Mạng `SaveMoneyToFirebase()` luôn nổ ra ngay Giây phút Vòng Quay Gacha Cắn Tiền. Ngăn chặn triệt để thói quen Tắt Game Nhanh (Force Quit) của các "Cậu ấm" lúc quay xịt màn nhện. Tiền đã ra khỏi ví là đi lên mây luôn!

---

## 🎒 PHẦN 3: HỆ THỐNG CUỐN CHIẾU UI (INVENTORY)
*Hệ thống kéo thả (Scroll) cực nặng cần được Tối ưu PolyAndCode.*

### 1. `IvenItems.cs` (Trường Kho Vũ Khí)
- Là tập nhãn mác vật phẩm. Bổ sung `Enum Category` (Phân loại item) & `toolId`. Việc này biến mọi file tên tĩnh (Tên Món: Bình Thuốc) thành khái niệm Định Ranh (Id: potion_lvl1). Sau này tích hợp ngôn ngữ JP/EN vào game, Code không vỡ.

### 2. `ToolManager.cs`
- Nông dân thời đại mới không thò tay vào Balo móc rổ lên vai (quá tốn RAM render Model 3D liên tục mỏi lưng).
- Khi bạn gắn Tool, `ToolManager` lưu một Bản Vi Kiến Trúc (`currentlyEquippedItem`). Sau đó Game sẽ ngầm hiểu mỗi thao tác cào cuốc của bạn có dính theo đặc tính của Vũ Khí đó hay không (Rất tiết kiệm tài nguyên Server!). Lệnh `ConsumeEquippedItem()` hỗ trợ cắt trừ phí tồn kho.

### 3. `CellItemData.cs` & `RecyclableScrollerIventory.cs`
- Nếu hòm đồ có 900 ngàn món vũ khí, hàm Instantiate đẻ giao diện sẽ đốt rụi phần cứng máy tính.
- Kiến trúc **Recyclable Scroll System (UI Cuốn Chiếu)**: Thực ra trên màn hình chỉ đẻ đúng "10 ô Rương Giao Diện Gỗ". Khi bạn Kéo chuột Vuốt Lên Vuốt Xuống, 10 Ô ở khung trên sẽ Dịch Chuyển xuống khung Dưới (Kiểu Rắn săn mồi uốn cong) và Xáo Data của Vũ Khí mới vào vỏ Giao diện ấy. Do đó Game 1 món đồ hay 10 tỷ món đồ thì điện thoại của Player vẫn nhẹ phơi phới! 
  -> **Logic Đỉnh cao**: Code của Cục Data và Cục Nén Hình Ảnh Tách Nhao ra hoàn toàn! Mảng `_contactList` chỉ chứa Chữ. Cột UI tự trỏ Data. 


> **Tổng Quan Trùng Điệp**: Mọi hệ thống trên Nông trại đều được xây xoay quanh 1 lõi: Tách rời Logic Game (Class/Data lơ lửng) và Hình Ảnh Render Đồ Họa (Tilemap/MonoBehaviour). Nó chính tạo ra thiết kế chuẩn mực "Model-View-Controller" (MVC) trứ danh của ngành lập trình phần mềm đương đại. Mãi Đỉnh!
