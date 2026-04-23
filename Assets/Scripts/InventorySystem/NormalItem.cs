using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// NormalItem (View Layer): Kế thừa các interface để bắt sự kiện UI chuột chuẩn của Unity
public class NormalItem : ItemInventoryBase, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;

    private Transform originalParent;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public override void Setup(ItemDataSO data, int amount)
    {
        base.Setup(data, amount);
        UpdateUI();
    }

    public override void AddQuantity(int amount)
    {
        base.AddQuantity(amount);
        UpdateUI();
    }

    public override void SetQuantity(int amount)
    {
        base.SetQuantity(amount);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemData != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = itemData.image;
                iconImage.enabled = true;
            }
            
            if (quantityText != null)
            {
                quantityText.text = _quantity > 1 ? _quantity.ToString() : "";
            }
        }
    }

    // ─── KÉO THẢ (DRAG & DROP) ───

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("<color=yellow>Đang bắt đầu kéo: </color>" + gameObject.name);
        originalParent = transform.parent;
        
        // Cập nhật lại Slot cũ là đang trống (trước khi di chuyển)
        InventorySlot currentSlot = originalParent.GetComponent<InventorySlot>();
        if (currentSlot != null)
        {
            currentSlot.itemInSlot = null; 
        }
        
        // Chuyển Item sang DragLayer
        if (DragController.Instant != null && DragController.Instant.DragLayer != null)
        {
            transform.SetParent(DragController.Instant.DragLayer);
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy DragController hoặc chưa kéo DragLayer vào DragController!");
        }
        
        // Xuyên thấu
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Sử dụng toạ độ trực tiếp từ Event Hệ thống thay vì Input.mousePosition để tương thích mọi Input System
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("<color=green>Đã buông chuột thả vật phẩm!</color>");
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (DragController.Instant != null)
        {
            DragController.Instant.HandleItemDrop(this, originalParent, eventData);
        }
    }

    // ─── TOOLTIP ───

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Chuột đang trỏ vào vật phẩm: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Chờ làm TooltipManager
    }
}
