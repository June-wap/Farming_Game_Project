# Project Memory: Farming Game Project

## 📝 Tổng quan dự án
Dự án game nông trại 2D Top-down sử dụng Unity, tích hợp Firebase để lưu trữ dữ liệu và hệ thống Inventory/Farming nâng cao.

---

## ✅ Những việc ĐÃ HOÀN THÀNH

### 1. Hệ thống Di chuyển & Hoạt ảnh (Animation)
- **Fix Animation Bug:** Khắc phục lỗi nhân vật không chạy hoạt ảnh cuốc đất/trồng cây do mất hướng nhìn khi đứng yên.
- **Last Direction Memory:** Cập nhật `playercontroler.cs` và `Player_Movment_Mouse.cs` để nhớ hướng nhìn cuối cùng, giúp kích hoạt đúng Animation Trái/Phải.
- **Busy State:** Tích hợp biến `IsBusy` trong `PlayerFarmControler` để khóa di chuyển khi đang thực hiện hành động farming.

### 2. Hệ thống Inventory (Túi đồ)
- **Drag & Drop:** Hoàn thiện logic kéo thả vật phẩm sử dụng `EventSystem` và `CanvasGroup`.
- **UI Feedback:** Thêm hiệu ứng đổi màu khi di chuột vào ô vật phẩm (Hover) và chọn vật phẩm (Selected).
- **Shortcut:** Phím **B** để bật/tắt nhanh túi đồ chính.

### 3. Hệ thống UI & HUD
- **Clock UI:** Tạo script `TimeUI.cs` đồng bộ 100% với `TimeManager`. Hiển thị định dạng 12h (AM/PM).
- **Gold UI:** Tạo script `GoldUI.cs` kết nối với `UserDataManager` để tự động cập nhật số tiền khi có thay đổi từ Firebase.
- **Stamina Bar:** Cấu hình thanh thể lực dạng `Filled` (Vertical) để rút cạn từ trên xuống dưới thay vì bóp méo hình ảnh.
- **Stamina Tooltip:** Script `StaminaHoverUI.cs` giúp ẩn/hiện con số thể lực chi tiết khi di chuột vào thanh bar.

### 4. Hệ thống Dữ liệu & Firebase
- **UserDataManager:** Quản lý Gold, Money, Tên người chơi và Ngày hiện tại.
- **Firebase Sync:** Tự động lưu và tải profile người chơi từ Database.

---

## 🛠 Những tệp đã CHỈNH SỬA / TẠO MỚI

| Tên File | Trạng thái | Chức năng chính |
| :--- | :--- | :--- |
| `playercontroler.cs` | Đã sửa | Di chuyển bàn phím, nhớ hướng nhìn, khóa khi Busy. |
| `Player_Movment_Mouse.cs` | Đã sửa | Di chuyển chuột, nhớ hướng nhìn, khóa khi Busy. |
| `PlayerFarmControler.cs` | Đã sửa | Quản lý hành động farming, cung cấp biến `IsBusy`. |
| `TimeUI.cs` | **Tạo mới** | Hiển thị đồng hồ, định dạng AM/PM, Day count. |
| `GoldUI.cs` | **Tạo mới** | Hiển thị tiền vàng, tự động cập nhật từ UserDataManager. |
| `StaminaHoverUI.cs` | Đã sửa | Hiển thị số liệu thể lực khi hover chuột. |
| `NormalItem.cs` | Đã sửa | Xử lý logic kéo thả vật phẩm. |
| `InventorySlot.cs` | Đã sửa | Hiển thị ô vật phẩm và hiệu ứng hover/click. |

---

## 🚀 Những việc ĐANG THỰC HIỆN
- **Lắp ráp UI:** Hoàn thiện việc sắp xếp các lớp Image cho Stamina Bar (Lớp Frame và lớp Fill).
- **Setup Scene:** Thêm các Scene mới (như `Insite_Home`) vào Build Settings để tránh lỗi chuyển cảnh.
- **Kết nối Logic:** Đảm bảo các hành động thu hoạch/mua bán trừ tiền đúng vào `UserDataManager` để UI cập nhật theo.

---
*Cập nhật lần cuối: 23/04/2026*