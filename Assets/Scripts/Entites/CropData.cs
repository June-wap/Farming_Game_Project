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
    
    // Đếm thời gian khô hạn. Nếu vượt 48 giây trơn (2 game days) thì héo.
    public float unwateredTimeInSeconds = 0f; 
    public float timeToWiltSeconds = 48f; 
    
    public CropData(Vector3Int pos)
    {
        cellPosition = pos;
        state = CropState.Empty;
        isWatered = false;
    }
}
