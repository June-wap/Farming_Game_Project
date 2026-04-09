using UnityEngine;

[System.Serializable]
public enum TilemapState
{
    Ground,
    Water,
    Tree,
    Rock,
    Animal,
}

// TilemapDetail: Lưu thông tin 1 ô tile (tọa độ x, y và loại tile).
// QUAN TRỌNG: Phải dùng public field (KHÔNG dùng { get; set; } property)
// vì JsonUtility.ToJson() của Unity chỉ đọc được field, không đọc được property.
[System.Serializable]
public class TilemapDetail
{
    // ✅ FIX Bug #4: Đổi từ Property { get; set; } sang Field thường.
    // Trước đây JsonUtility.ToJson() trả về "{}" — json rỗng hoàn toàn.
    public int x;
    public int y;
    public TilemapState tilemapState;

    public TilemapDetail()
    {
    }

    public TilemapDetail(int x, int y, TilemapState tilemapState)
    {
        this.x = x;
        this.y = y;
        this.tilemapState = tilemapState;
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
