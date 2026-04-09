using System.Collections.Generic;
using UnityEngine;

// Map: Wrapper chứa toàn bộ dữ liệu tile của 1 scene/map.
// [System.Serializable] + dùng FIELD (không phải property) để JsonUtility có thể serialize được.
[System.Serializable]
public class Map
{
    // ✅ FIX Bug #4: Đổi từ Property { get; set; } sang Field thường.
    // JsonUtility của Unity CHỈ serialize public field, KHÔNG serialize property.
    public List<TilemapDetail> tilemapDetails;

    public Map()
    {
        tilemapDetails = new List<TilemapDetail>();
    }

    public Map(List<TilemapDetail> tilemapDetails)
    {
        this.tilemapDetails = tilemapDetails;
    }

    // ✅ FIX Bug #1: ToString() phải trả về string.
    // Trước đây: return tilemapDetails?.Count ?? 0 → trả về int → COMPILE ERROR.
    // JsonConvert (Newtonsoft) cũng không có sẵn trong Unity.
    // → Dùng JsonUtility.ToJson() là chuẩn Unity, không cần package ngoài.
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }

    public int GetLength()
    {
        return tilemapDetails?.Count ?? 0;
    }
}