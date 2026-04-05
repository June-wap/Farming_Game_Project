using UnityEngine;
using System;

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance;

    [Header("Current Status")]
    public IvenItems currentlyEquippedItem;

    // Sự kiện bắn ra khi đổi đồ (Để UI nhận biết nếu cần hiển thị Icon món đồ dưới góc màn hình)
    public Action<IvenItems> OnItemEquipped;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Chạy xuyên các Map
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gắn (Equip) một Item từ Inventory vào tay nhân vật
    /// </summary>
    /// <param name="item">Vật phẩm được chọn</param>
    public void EquipItem(IvenItems item)
    {
        currentlyEquippedItem = item;
        Debug.Log($"Đã trang bị: {item.itemName} (Category: {item.category})");
        
        OnItemEquipped?.Invoke(item);
    }
    
    /// <summary>
    /// Helper: Tiêu thụ hạt giống khi gieo trồng
    /// </summary>
    public void ConsumeEquippedItem()
    {
        if (currentlyEquippedItem != null && currentlyEquippedItem.category == ItemCategory.Seed)
        {
            currentlyEquippedItem.quantity--;
            Debug.Log($"Đã dùng 1 {currentlyEquippedItem.itemName}. Còn lại: {currentlyEquippedItem.quantity}");
            
            // Nếu dùng hết (quantity <= 0), gỡ trang bị (hoặc xóa khỏi túi đồ - xử lý logic bên Inventory)
            if (currentlyEquippedItem.quantity <= 0)
            {
                Debug.Log($"Hết {currentlyEquippedItem.itemName}!");
                currentlyEquippedItem = null;
                // TODO: Sync để Recyclable Scroller tự động remove card này ra khỏi List
            }
        }
    }
}
