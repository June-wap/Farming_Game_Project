using System;
using UnityEngine;

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
