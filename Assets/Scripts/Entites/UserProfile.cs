using System;
using UnityEngine;
using System.Collections.Generic;

// ─── CÁC CẤU TRÚC PHỤ TRỢ ──────────────────────────────────────────

[Serializable]
public class InventoryItemData
{
    public string itemID; // Mã định danh của ItemDataSO
    public int amount;
}

[Serializable]
public class AnimalSaveData
{
    public string animalType;  // "Chicken", "Cow", v.v.
    public float posX;
    public float posY;
    public int hungerDays;     // Tích luỹ ngày đói
}

// UserProfile: Dữ liệu người chơi được lưu lên Firebase.
// QUAN TRỌNG: Dùng public FIELD (không phải property { get; set; })
// để JsonUtility.ToJson() có thể serialize được.
[Serializable]
public class UserProfile
{
    public string name;       // Tên người chơi
    public int gold;          // Vàng (dùng để mua công cụ, hạt giống cao cấp)
    public int money;         // Tiền thông thường
    public int currentDay;    // Ngày hiện tại trong game (đồng bộ với TimeManager)

    // Dữ liệu mở rộng
    public float playerPosX;  // Vị trí X của người chơi
    public float playerPosY;  // Vị trí Y của người chơi
    public int currentHour;   // Giờ trong game
    public int currentMinute; // Phút trong game

    // Túi đồ (Inventory)
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();

    // Động vật (Animals)
    public List<AnimalSaveData> animals = new List<AnimalSaveData>();

    // Constructor mặc định cho người chơi mới
    public UserProfile()
    {
        name        = "Farmer";
        gold        = 0;
        money       = 500;
        currentDay  = 1;
    }

    public UserProfile(string name, int gold, int money, int currentDay = 1)
    {
        this.name       = name;
        this.gold       = gold;
        this.money      = money;
        this.currentDay = currentDay;
    }

    // Serialize thành JSON — dùng JsonUtility chuẩn Unity (không cần Newtonsoft)
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
