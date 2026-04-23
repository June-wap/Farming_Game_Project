using UnityEngine;
using System.Collections.Generic;
using System.Text;

// InventoryManager (Controller Layer): Quản lý các ô chứa, đóng gói dữ liệu JSON lưu game.
public class InventoryManager : Singleton<InventoryManager>
{
    [Header("Danh Sách Ô Chứa (Slots)")]
    // Đã chuyển thành List<InventorySlot> để dễ kiểm tra trạng thái có đồ hay không
    public List<InventorySlot> slots = new List<InventorySlot>();
    
    [Header("Prefab Vật Phẩm")]
    public GameObject normalItemPrefab; // Phải có gắn NormalItem và Image component

    [Header("Giao Diện (UI)")]
    [Tooltip("Kéo thả MainInventoryGroup vào đây để bật tắt")]
    public GameObject mainInventoryPanel;

    // Biến lưu trữ ô Slot đang được người chơi bấm chọn
    [HideInInspector] public InventorySlot currentSelectedSlot;

    private void Update()
    {
        // Bấm phím B để bật/tắt túi đồ chính
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (mainInventoryPanel != null)
            {
                // Nếu đang bật thì tắt, nếu đang tắt thì bật
                mainInventoryPanel.SetActive(!mainInventoryPanel.activeSelf);
            }
            else
            {
                Debug.LogWarning("Chưa gán giao diện túi đồ (MainInventoryGroup) vào InventoryManager!");
            }
        }
    }

    // Hàm gọi khi người chơi click vào một ô Slot
    public void SelectSlot(InventorySlot slot)
    {
        // 1. Tắt hiệu ứng viền sáng/chọn ở ô cũ
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
        }

        // 2. Gán ô mới là ô đang chọn
        currentSelectedSlot = slot;
        
        // 3. Bật hiệu ứng sáng cho ô mới
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(true);
            
            // Log ra console để bạn test (có thể xoá dòng này sau)
            if (slot.itemInSlot != null) {
                Debug.Log($"Đã chọn ô có vật phẩm: {slot.itemInSlot.itemData.itemName}");
            } else {
                Debug.Log("Đã chọn một ô trống.");
            }
        }
    }

    private void Start()
    {
        // Tự động tìm tất cả các slot trong scene nếu chưa gán tay
        if (slots.Count == 0)
        {
            // Tìm tất cả các slot trong màn hình (Bao gồm cả Toolbar và MainInventory)
            InventorySlot[] foundSlots = Object.FindObjectsByType<InventorySlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            slots = new List<InventorySlot>(foundSlots);
        }
    }

    public bool setItemOnInventory(ItemDataSO data, int amount)
    {
        // Ưu tiên 1: Tìm ô đang có sẵn item cùng loại và CÒN CHỖ TRỐNG để cộng dồn
        foreach (InventorySlot slot in slots)
        {
            if (slot.itemInSlot != null && slot.itemInSlot.itemData.id == data.id)
            {
                if (slot.itemInSlot.Quantity < slot.itemInSlot.MaxCapacity)
                {
                    int spaceLeft = slot.itemInSlot.MaxCapacity - slot.itemInSlot.Quantity;
                    if (amount <= spaceLeft)
                    {
                        slot.itemInSlot.AddQuantity(amount);
                        SaveInventory();
                        return true;
                    }
                    else
                    {
                        // Nếu số lượng cần thêm lớn hơn chỗ trống, lấp đầy ô này rồi tiếp tục tìm ô khác cho phần dư
                        slot.itemInSlot.SetQuantity(slot.itemInSlot.MaxCapacity);
                        amount -= spaceLeft;
                    }
                }
            }
        }

        // Ưu tiên 2: Tìm ô trống hoàn toàn để đặt phần item còn dư vào
        if (amount > 0)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.itemInSlot == null)
                {
                    GameObject newItem = Instantiate(normalItemPrefab, slot.transform);
                    
                    // Sửa lỗi sai lệch vị trí: Đưa item về chính giữa và reset tỷ lệ
                    newItem.transform.localPosition = Vector3.zero;
                    newItem.transform.localScale = Vector3.one;
                    
                    // Sửa lỗi kích thước: Bắt item dãn ra vừa vặn với Slot
                    RectTransform rect = newItem.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;
                    }

                    NormalItem itemComp = newItem.GetComponent<NormalItem>();
                    itemComp.Setup(data, amount);
                    
                    slot.UpdateSlotState(); // Cập nhật lại biến itemInSlot cho slot này
                    SaveInventory();
                    return true;
                }
            }
        }
        
        Debug.LogWarning("[InventoryManager] Túi đồ đã đầy, không thể chứa thêm: " + data.itemName);
        return false;
    }

    public void clearSlot(InventorySlot slot)
    {
        foreach (Transform child in slot.transform)
        {
            Destroy(child.gameObject);
        }
        slot.UpdateSlotState();
    }

    public void autoMerge()
    {
        // Sắp xếp và gộp đồ (Sẽ làm ở bước sau nếu bạn cần)
        Debug.Log("Auto Merged");
    }

    public void autoMerge2()
    {
        Debug.Log("Auto Sorted");
    }

    public string PackageInventoryData()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"items\":[");
        bool isFirst = true;
        
        foreach (InventorySlot slot in slots)
        {
            // Cập nhật lại chắc chắn trạng thái slot trước khi lưu
            slot.UpdateSlotState();
            
            if (slot.itemInSlot != null)
            {
                if (!isFirst) sb.Append(",");
                sb.Append(slot.itemInSlot.dataToString());
                isFirst = false;
            }
        }
        sb.Append("]}");
        return sb.ToString();
    }

    public void SaveInventory()
    {
        string json = PackageInventoryData();
        string encrypted = Security.Encrypt(json);
        PlayerPrefs.SetString("InventorySave", encrypted);
        PlayerPrefs.Save();
    }
}
