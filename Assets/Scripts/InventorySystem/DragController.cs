using UnityEngine;
using UnityEngine.EventSystems;

// DragController (Controller Layer): Xử lý thả đồ bằng Raycast, không dùng Distance
public class DragController : Singleton<DragController>
{
    [Header("Tham chiếu UI")]
    public Transform DragLayer;

    public void HandleItemDrop(NormalItem draggedItem, Transform originalParent, PointerEventData eventData)
    {
        InventorySlot targetSlot = null;
        
        // Nhờ PointerEventData của Unity Event System để lấy chính xác object đang nằm dưới chuột
        if (eventData.pointerEnter != null)
        {
            targetSlot = eventData.pointerEnter.GetComponent<InventorySlot>();
            if (targetSlot == null)
            {
                targetSlot = eventData.pointerEnter.GetComponentInParent<InventorySlot>();
            }
        }

        if (targetSlot != null)
        {
            if (targetSlot.itemInSlot != null && targetSlot.itemInSlot.gameObject != draggedItem.gameObject)
            {
                // Ô đã có đồ: Check gộp
                ItemInventoryBase existingItem = targetSlot.itemInSlot;
                
                if (existingItem.itemData.id == draggedItem.itemData.id)
                {
                    int total = existingItem.Quantity + draggedItem.Quantity;
                    if (total <= existingItem.MaxCapacity)
                    {
                        existingItem.SetQuantity(total);
                        Destroy(draggedItem.gameObject); // Bỏ đi
                    }
                    else
                    {
                        existingItem.SetQuantity(existingItem.MaxCapacity);
                        draggedItem.SetQuantity(total - existingItem.MaxCapacity);
                        ReturnToOriginal(draggedItem, originalParent);
                    }
                }
                else
                {
                    // Tráo đổi vị trí
                    existingItem.transform.SetParent(originalParent);
                    existingItem.transform.localPosition = Vector3.zero;
                    
                    draggedItem.transform.SetParent(targetSlot.transform);
                }
            }
            else
            {
                // Ô trống: Thả vào
                draggedItem.transform.SetParent(targetSlot.transform);
            }
        }
        else
        {
            // Trượt ra ngoài UI, trả về chỗ cũ
            ReturnToOriginal(draggedItem, originalParent);
        }

        if (draggedItem != null)
        {
            draggedItem.transform.localPosition = Vector3.zero;
        }

        // Cập nhật lại thông tin state của Slot
        InventorySlot originalSlot = originalParent.GetComponent<InventorySlot>();
        if (originalSlot != null) originalSlot.UpdateSlotState();
        
        if (targetSlot != null) targetSlot.UpdateSlotState();

        InventoryManager.Instant.SaveInventory();
    }

    private void ReturnToOriginal(NormalItem item, Transform originalParent)
    {
        item.transform.SetParent(originalParent);
        item.transform.localPosition = Vector3.zero;
    }
}
