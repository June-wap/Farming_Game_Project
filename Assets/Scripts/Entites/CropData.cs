using UnityEngine;

public enum CropState
{
    Empty,
    Tilled,
    Watered,       // Dùng khi đất đã được tưới, chưa gieo hạt
    Planted,       // Dùng khi đã gieo hạt
    Harvestable,
    Wilted
}

[System.Serializable]
public class CropData
{
    public Vector3Int cellPosition;
    public CropState state;
    
    // Cờ đánh dấu có đang ngậm nước hay không
    public bool isWatered = false;

    // Thời gian trôi qua cho sự lớn lên (chỉ tăng khi isWatered = true)
    public int plantedTimeInMinutes = 0; 
    public int timeToHarvestMinutes = 120; // 120 phút ingame = 2 phút ngoài đời
    
    // Đếm Số lượng ngày (Game Days) bị khô hạn. Vượt 2 ngày sẽ chết héo.
    public int daysWithoutWater = 0; 
    
    public CropData(Vector3Int pos)
    {
        cellPosition = pos;
        state = CropState.Empty;
        isWatered = false;
    }
}
