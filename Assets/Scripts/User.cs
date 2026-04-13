using UnityEngine;

// User: Class dữ liệu người chơi (legacy — giữ lại để tương thích).
// Dùng UserProfile (Entites/UserProfile.cs) cho hệ thống mới.
// QUAN TRỌNG: Phải dùng public FIELD (không phải property { get; set; })
// để JsonUtility.ToJson() serialize được.
[System.Serializable]
public class User
{
    public string name;   // ✅ Field thay vì property
    public int    gold;
    public int    money;

    public User() { }

    public User(string name, int gold, int money)
    {
        this.name  = name;
        this.gold  = gold;
        this.money = money;
    }

    // ✅ Dùng JsonUtility thay vì JsonConvert (Newtonsoft không có trong Unity)
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
