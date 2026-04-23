using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance;

    [Header("Cấu hình Thể Lực")]
    public int maxStamina = 100;
    public int currentStamina;
    
    [Header("Giao diện UI")]
    [Tooltip("Kéo thả thanh Image UI (thuộc tính Image Type: Filled) vào đây")]
    public Image staminaBar;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }

    // Hàm gọi khi làm việc nặng (trả về true nếu đủ sức)
    public bool UseStamina(int amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.LogWarning("[Stamina] Đuối sức rồi! Hãy ăn gì đó hoặc đi ngủ.");
            return false;
        }
    }
    
    // Hàm gọi khi ăn hoặc ngủ (hồi phục thể lực)
    public void RestoreStamina(int amount)
    {
        currentStamina += amount;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = (float)currentStamina / maxStamina;
        }
    }
}
