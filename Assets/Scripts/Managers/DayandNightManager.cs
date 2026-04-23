using UnityEngine;
using UnityEngine.Rendering.Universal; // Bắt buộc phải có để điều khiển Light 2D

public class DayNightManager : MonoBehaviour
{
    [Header("Cài đặt Ánh sáng")]
    public Light2D globalLight;

    [Header("Dải màu theo thời gian (0h -> 24h)")]
    // Bạn sẽ chỉnh Gradient này trên Inspector
    public Gradient lightColorGradient; 

    private void Update()
    {
        // 1. Kiểm tra xem TimeManager và GlobalLight đã có chưa
        if (TimeManager.Instance == null || globalLight == null) return;

        // 2. Lấy giờ hiện tại TỪ TIMEMANAGER (để đồng bộ 100% với UI và Cây trồng)
        // CurrentMinute trong TimeManager đang đóng vai trò là Giờ (0 - 23)
        int currentHour = TimeManager.Instance.CurrentMinute;

        // 3. Tính phần trăm thời gian trôi qua trong ngày (0.0 đến 1.0)
        // Ví dụ: 6h sáng = 6 / 24 = 0.25 (25%)
        float timePercent = currentHour / 24f;

        // 4. Đổi màu ánh sáng dựa trên dải màu (Gradient)
        globalLight.color = lightColorGradient.Evaluate(timePercent);
    }
}
