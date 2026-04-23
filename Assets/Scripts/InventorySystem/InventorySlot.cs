using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Gắn script này vào các UI Slot (Các ô trống của túi đồ ở cả MainInventory và Toolbar)
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Tham chiếu đến item đang nằm trong ô này (nếu null nghĩa là ô trống)
    public ItemInventoryBase itemInSlot;

    [Header("Hiệu Ứng Màu Sắc")]
    public Image slotImage; // Hình nền của Slot
    public Color normalColor = Color.white; // Màu bình thường (Trắng)
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Màu khi di chuột vào (Hơi xám)
    public Color selectedColor = new Color(0.6f, 1f, 0.6f, 1f); // Màu khi được chọn (Xanh nhạt)

    private bool isSelected = false;

    private void Awake()
    {
        if (slotImage == null) slotImage = GetComponent<Image>();
        if (slotImage != null) slotImage.color = normalColor;
    }

    // Cập nhật lại thông tin khi có item được thả vào hoặc lấy ra
    public void UpdateSlotState()
    {
        itemInSlot = GetComponentInChildren<ItemInventoryBase>();
    }

    // Khi di chuột VÀO Slot
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && slotImage != null)
        {
            slotImage.color = hoverColor;
        }
    }

    // Khi di chuột RA KHỎI Slot
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && slotImage != null)
        {
            slotImage.color = normalColor;
        }
    }

    // Khi BẤM CHUỘT (Click) vào Slot
    public void OnPointerClick(PointerEventData eventData)
    {
        // Gọi Manager để đổi trạng thái ô đang được chọn
        if (InventoryManager.Instant != null)
        {
            InventoryManager.Instant.SelectSlot(this);
        }
    }

    // Hàm dùng để đổi màu khi được Manager ra lệnh chọn/bỏ chọn
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (slotImage != null)
        {
            slotImage.color = isSelected ? selectedColor : normalColor;
        }
    }
}
