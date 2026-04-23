using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class StaminaHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cục Text hiển thị Năng Lượng (VD: 250/250)")]
    public TMP_Text staminaText;

    private bool isHovering = false;

    private void Start()
    {
        // Vừa vào game thì giấu cái chữ đi
        if (staminaText != null)
        {
            staminaText.gameObject.SetActive(false);
        }
    }

    // Hàm gọi tự động khi Chuột LÀ TRÊN cục UI (Gắn script này)
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (staminaText != null)
        {
            staminaText.gameObject.SetActive(true);
        }
    }

    // Hàm gọi tự động khi Chuột LÌA KHỎI cục UI
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (staminaText != null)
        {
            staminaText.gameObject.SetActive(false);
        }
    }

    // Cập nhật điểm số liên tục nếu đang để chuột ở đó mà lại bấm nút vung cuốc
    private void Update()
    {
        if (isHovering && staminaText != null && StaminaManager.Instance != null)
        {
            staminaText.text = $"{StaminaManager.Instance.currentStamina} / {StaminaManager.Instance.maxStamina}";
        }
    }
}
