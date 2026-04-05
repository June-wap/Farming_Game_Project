using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CropManager : MonoBehaviour
{
    public static CropManager Instance;

    [Header("Tiles Configuration")]
    public TileBase tilledTile;      // Đất đã cuốc
    public TileBase wateredSoilTile; // Đất đã cuốc và được tưới
    public TileBase plantedTile;     // Đất đã gieo hạt (khô)
    public TileBase wateredPlantedTile; // Đất đã gieo hạt (ẩm)
    public TileBase harvestableTile; // Cây trồng trưởng thành
    public TileBase wiltedTile;      // Cây héo
    
    private Dictionary<Vector3Int, CropData> activeCrops = new Dictionary<Vector3Int, CropData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnGameMinutePassed += HandleMinuteTick;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnGameMinutePassed -= HandleMinuteTick;
        }
    }

    private void Update()
    {
        // Chạy loop bằng Update() chỉ để bắt thời gian realtime tính cho việc Héo (sẽ chỉ loop qua những cây planted mà khô)
        List<CropData> cropsToWilt = new List<CropData>();

        foreach (var kvp in activeCrops)
        {
            CropData crop = kvp.Value;
            if (crop.state == CropState.Planted && !crop.isWatered)
            {
                crop.unwateredTimeInSeconds += Time.deltaTime;
                if (crop.unwateredTimeInSeconds >= crop.timeToWiltSeconds)
                {
                    cropsToWilt.Add(crop);
                }
            }
        }

        // Apply wilted state ra khỏi vòng lặp Dictionary để tránh lỗi modified collection
        foreach (var crop in cropsToWilt)
        {
            crop.state = CropState.Wilted;
            Debug.Log($"Cây tại {crop.cellPosition} đã chết héo do không tưới nước quá 2 ngày game!");
            // Ta không can thiệp Tilemap tự động ở đây để đỡ bug reference scene,
            // Sẽ cần Refresh/Update Tile graphic vào Player nếu cần, hoặc giả định PlayerFarmController làm tươi map.
            // Để đơn giản, cứ mỗi lần tương tác ta cũng update Tile của nó. Hoặc nếu bạn muốn nó tự biến hình:
            // Tốt nhất nếu có Tilemap đang active, ta set nó. Nhưng CropManager không giữ Tilemap.
        }
    }

    private void HandleMinuteTick(int min, int hour, int day)
    {
        foreach (var kvp in activeCrops)
        {
            CropData crop = kvp.Value;
            
            // Cây CHỈ lớn khi được tưới nước
            if (crop.state == CropState.Planted && crop.isWatered)
            {
                crop.plantedTimeInMinutes++;
                
                if (crop.plantedTimeInMinutes >= crop.timeToHarvestMinutes)
                {
                    crop.state = CropState.Harvestable;
                    Debug.Log($"Cây tại {crop.cellPosition} đã có thể thu hoạch!");
                }
            }
        }
    }

    public void ExecuteTilling(Vector3Int cellPosition, Tilemap tm_Ground, TileBase tb_Ground)
    {
        if (tm_Ground == null) return;

        TileBase currentTile = tm_Ground.GetTile(cellPosition);
        if (currentTile == tb_Ground)
        {
            tm_Ground.SetTile(cellPosition, tilledTile);
            
            if (!activeCrops.ContainsKey(cellPosition))
            {
                activeCrops[cellPosition] = new CropData(cellPosition);
            }
            activeCrops[cellPosition].state = CropState.Tilled;
            activeCrops[cellPosition].isWatered = false;
        }
    }

    public void ExecuteWatering(Vector3Int cellPosition, Tilemap tm_Ground)
    {
        if (tm_Ground == null) return;

        if (activeCrops.TryGetValue(cellPosition, out CropData crop))
        {
            if (crop.state == CropState.Tilled || crop.state == CropState.Watered)
            {
                crop.state = CropState.Watered;
                crop.isWatered = true;
                tm_Ground.SetTile(cellPosition, wateredSoilTile);
            }
            else if (crop.state == CropState.Planted)
            {
                crop.isWatered = true;
                crop.unwateredTimeInSeconds = 0f; // Tưới nước reset lại đồng hồ héo
                tm_Ground.SetTile(cellPosition, wateredPlantedTile);
            }
            Debug.Log($"Đã tưới nước tại {cellPosition}!");
        }
    }

    public void ExecutePlanting(Vector3Int cellPosition, Tilemap tm_Ground)
    {
        if (tm_Ground == null) return;

        if (activeCrops.TryGetValue(cellPosition, out CropData crop))
        {
            if (crop.state == CropState.Tilled || crop.state == CropState.Watered)
            {
                // Giữ nguyên cờ isWatered nếu trước đó đất đã được tưới
                bool wasWatered = crop.isWatered;
                
                crop.state = CropState.Planted;
                crop.plantedTimeInMinutes = 0;
                crop.unwateredTimeInSeconds = 0f;
                
                tm_Ground.SetTile(cellPosition, wasWatered ? wateredPlantedTile : plantedTile);
            }
        }
    }

    public void ExecuteHarvesting(Vector3Int cellPosition, Tilemap tm_Ground)
    {
        if (tm_Ground == null) return;

        if (activeCrops.TryGetValue(cellPosition, out CropData crop))
        {
            if (crop.state == CropState.Harvestable)
            {
                ClearCropAndYield(crop, cellPosition, tm_Ground);
            }
            else if (crop.state == CropState.Wilted)
            {
                // Héo thì xóa đi không rớt ra gì
                crop.state = CropState.Tilled;
                crop.isWatered = false;
                tm_Ground.SetTile(cellPosition, tilledTile);
                Debug.Log("Đã dọn dẹp cây héo.");
            }
        }
    }
    
    
    private void ClearCropAndYield(CropData crop, Vector3Int cellPosition, Tilemap tm_Ground)
    {
        crop.state = CropState.Tilled;
        crop.isWatered = false;
        tm_Ground.SetTile(cellPosition, tilledTile);
        
        IvenItems harvestedItem = new IvenItems("Hoa Nghĩa Trang", "Có thể bán lấy tiền mua hạt giống.", 1, 50, ItemCategory.Produce, "produce_graveflower");
        RecyclableScrollerIventory inventory = FindObjectOfType<RecyclableScrollerIventory>();
        if (inventory != null)
        {
            inventory.AddIventoryItem(harvestedItem);
        }
    }

    // Các hàm cho Firebase
    public List<CropData> GetActiveCropsList()
    {
        return new List<CropData>(activeCrops.Values);
    }

    public void RestoreCropsFromSave(List<CropData> savedCrops, Tilemap tm_Ground)
    {
        activeCrops.Clear();
        
        if (savedCrops == null) return;
        
        foreach (var crop in savedCrops)
        {
            activeCrops[crop.cellPosition] = crop;
            
            // Xây đắp đồ hoạ lại như cũ
            if (tm_Ground != null)
            {
                TileBase targetTile = null;
                switch (crop.state)
                {
                    case CropState.Tilled: targetTile = tilledTile; break;
                    case CropState.Watered: targetTile = wateredSoilTile; break;
                    case CropState.Planted: targetTile = crop.isWatered ? wateredPlantedTile : plantedTile; break;
                    case CropState.Harvestable: targetTile = harvestableTile; break;
                    case CropState.Wilted: targetTile = wiltedTile; break;
                }
                
                if (targetTile != null)
                {
                    tm_Ground.SetTile(crop.cellPosition, targetTile);
                }
            }
        }
        Debug.Log("Đã phục hồi Grid Trồng Trọt từ Firebase!");
    }
}
