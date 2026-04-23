using UnityEngine;

// Đặt Script này lên một vật thể Giường ngủ (Bed), gán BoxCollider2D (tích IsTrigger)
public class BedInteract : MonoBehaviour
{
    private bool isPlayerNear = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("[Bed] Lại gần giường, bấm phím E để đi ngủ...");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    private void Update()
    {
        // Nếu Player đứng cạnh giường và bấm phím E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[Bed] Player đang chìm vào giấc ngủ...");
            
            // Gọi bức màn đen từ SceneController
            if (SceneController.Instance != null)
            {
                SceneController.Instance.StartSleepFade(() => 
                {
                    // --- ĐOẠN CODE NÀY CHẠY KHI MÀN HÌNH TỐI ĐEN NHẤT ---

                    // 1. Ép thời gian nhảy vọt sang Sáng Ngày Hôm Sau
                    if (TimeManager.Instance != null)
                    {
                        TimeManager.Instance.SkipToNextDay();
                    }

                    // 2. Châm đầy lại ống máu Thể lực (100% Full)
                    if (StaminaManager.Instance != null)
                    {
                        StaminaManager.Instance.RestoreStamina(9999);
                    }
                });
            }
            else 
            {
                // Fallback nếu không có SceneController
                if (TimeManager.Instance != null) TimeManager.Instance.SkipToNextDay();
                if (StaminaManager.Instance != null) StaminaManager.Instance.RestoreStamina(9999);
            }
        }
    }
}
