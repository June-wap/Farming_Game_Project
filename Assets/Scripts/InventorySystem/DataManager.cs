using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySaveWrapper
{
    public List<ItemSaveData> items = new List<ItemSaveData>();
}

// DataManager (Controller Layer): Load file save và tìm kiếm item data.
public class DataManager : Singleton<DataManager>
{
    [Header("Kho Dữ Liệu Các Loại Item")]
    [Tooltip("Kéo thả tất cả các cục ScriptableObject (ItemDataSO) vào đây")]
    [SerializeField] private List<ItemDataSO> allItemsDatabase;

    private void Start()
    {
        // Load kho đồ ngay khi bắt đầu
        LoadGame();
    }

    public ItemDataSO GetItemDataByID(string id)
    {
        return allItemsDatabase.Find(x => x.id == id);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("InventorySave"))
        {
            string encrypted = PlayerPrefs.GetString("InventorySave");
            try
            {
                // Giải mã AES
                string json = Security.Decrypt(encrypted);
                InventorySaveWrapper data = JsonUtility.FromJson<InventorySaveWrapper>(json);
                
                if (data != null && data.items != null)
                {
                    foreach (var itemSave in data.items)
                    {
                        ItemDataSO itemData = GetItemDataByID(itemSave.id);
                        if (itemData != null)
                        {
                            // Ra lệnh sinh ra vật phẩm
                            InventoryManager.Instant.setItemOnInventory(itemData, itemSave.quantity);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[DataManager] Lỗi giải mã hoặc file save bị hỏng: " + e.Message);
            }
        }
    }

    // --- PHẦN DEBUG: DÙNG ĐỂ TEST GAME ---
    private void Update()
    {
        // Nhấn phím P để tự động tạo ngẫu nhiên từ 1-5 vật phẩm có trong Database
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (allItemsDatabase != null && allItemsDatabase.Count > 0)
            {
                // Chọn ngẫu nhiên 1 loại vật phẩm trong danh sách bạn đã kéo vào
                ItemDataSO randomItem = allItemsDatabase[Random.Range(0, allItemsDatabase.Count)];
                int randomAmount = Random.Range(1, 6); // Random từ 1 đến 5
                
                bool isSuccess = InventoryManager.Instant.setItemOnInventory(randomItem, randomAmount);
                if (isSuccess)
                {
                    Debug.Log($"<color=green>[Test Spawn]</color> Đã sinh ra {randomAmount}x {randomItem.itemName}!");
                }
            }
            else
            {
                Debug.LogWarning("Chưa có ItemDataSO nào trong danh sách All Items Database của DataManager!");
            }
        }

        // Nhấn phím O để xoá file save (Làm sạch kho đồ)
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerPrefs.DeleteKey("InventorySave");
            PlayerPrefs.Save();
            Debug.Log("<color=red>[Test Spawn]</color> Đã xóa toàn bộ file save Inventory. Hãy khởi động lại game để thấy kết quả.");
        }
    }
}
