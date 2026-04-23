using UnityEngine;
using TMPro; // Chứa TextMeshPro
using UnityEngine.UI;

// Script quản lý giao diện Đồng hồ
public class TimeUI : MonoBehaviour
{
    [Header("Hiển thị Chữ (Text)")]
    [Tooltip("Text để hiện Giờ (Ví dụ: 08:00)")]
    public TextMeshProUGUI timeText; 
    
    [Tooltip("Text để hiện Ngày (Ví dụ: Ngày 1)")]
    public TextMeshProUGUI dayText;  

    [Header("Hiển thị Hình ảnh (Tùy chọn)")]
    [Tooltip("Kim đồng hồ để xoay (nếu có)")]
    public Transform clockHand; 
    
    [Tooltip("Image hiển thị Mặt trời / Mặt trăng (nếu có)")]
    public Image dayNightIcon;  
    public Sprite sunSprite;
    public Sprite moonSprite;

    private void Start()
    {
        // Đăng ký lắng nghe sự kiện từ TimeManager để tự động cập nhật
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnGameMinuteTick += UpdateTimeUI;
            TimeManager.Instance.OnNewDayStarted += UpdateTimeUI;
            
            // Cập nhật ngay lần đầu
            UpdateTimeUI();
        }
        else
        {
            Debug.LogWarning("[TimeUI] Không tìm thấy TimeManager, sẽ tự động thử lại sau 1 giây!");
            Invoke(nameof(Start), 1f);
        }
    }

    private void OnDestroy()
    {
        // Huỷ đăng ký khi UI bị xoá để tránh lỗi bộ nhớ
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnGameMinuteTick -= UpdateTimeUI;
            TimeManager.Instance.OnNewDayStarted -= UpdateTimeUI;
        }
    }

    private void UpdateTimeUI()
    {
        if (TimeManager.Instance == null) return;

        // Lưu ý: Trong TimeManager hiện tại, CurrentMinute đếm từ 0-23, tương đương với 24 GIỜ trong ngày.
        int hour = TimeManager.Instance.CurrentMinute; 
        int day = TimeManager.Instance.CurrentDay;

        // 1. Cập nhật Text Giờ (Định dạng 12h AM/PM cho giống game của bạn)
        if (timeText != null)
        {
            string ampm = hour < 12 ? "am" : "pm";
            int displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12; // 0h hoặc 12h trưa đều hiển thị là 12
            
            timeText.text = $"{displayHour}.00 {ampm}"; // Kết quả: "6.00 am", "2.00 pm"
        }

        // 2. Cập nhật Text Ngày
        if (dayText != null)
        {
            dayText.text = $"Day {day}"; // Đổi thành chữ Day cho hợp tone UI của bạn
        }

        // 3. Xoay kim đồng hồ (nếu có)
        // 1 ngày 24 tiếng = xoay 360 độ -> 1 tiếng = 15 độ
        if (clockHand != null)
        {
            float rotationAngle = (hour / 24f) * -360f;
            // Xoay kim theo trục Z
            clockHand.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }

        // 4. Đổi icon Mặt trời / Mặt trăng (Từ 6h sáng đến 18h tối là Mặt trời)
        if (dayNightIcon != null && sunSprite != null && moonSprite != null)
        {
            bool isDaytime = hour >= 6 && hour <= 18;
            dayNightIcon.sprite = isDaytime ? sunSprite : moonSprite;
        }
    }
}
