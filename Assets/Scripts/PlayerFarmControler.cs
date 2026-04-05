using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerFarmController : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tm_Ground;
    public TileBase tb_Ground;
    
    // Tuỳ chỉnh references từ Editor: kéo thả Field vào Planted / Tilled trong CropManager
    
    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.LoadGame(currentScene, 
                (mapData) => 
                {
                    if (CropManager.Instance != null && mapData != null && mapData.crops != null)
                    {
                        CropManager.Instance.RestoreCropsFromSave(mapData.crops, tm_Ground);
                    }
                }, 
                (invData) =>
                {
                    RecyclableScrollerIventory inv = FindObjectOfType<RecyclableScrollerIventory>();
                    if (inv != null && invData != null && invData.items != null)
                    {
                        inv.RestoreFromSave(invData.items);
                    }
                });
        }
    }

    void Update()
    {
        HandleFarmAction();
    }

    public void HandleFarmAction()
    {
        if (tm_Ground == null || CropManager.Instance == null) return;
        
        // Cầm cái gì trên tay?
        IvenItems equipped = ToolManager.Instance != null ? ToolManager.Instance.currentlyEquippedItem : null;
        
        // Phím C: Đào đất (Tilling)
        if (Input.GetKeyDown(KeyCode.C))       
        {
            if (equipped != null && (equipped.itemName.ToLower().Contains("cuốc") || equipped.toolId == "hoe"))
            {
                Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
                CropManager.Instance.ExecuteTilling(cellPosition, tm_Ground, tb_Ground);
            }
            else
            {
                Debug.Log("Bạn cần phải trang bị 'Cuốc' để thao tác đào đất!");
            }
        }
        
        // Phím V: Gieo hạt (Planting)
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (equipped != null && equipped.category == ItemCategory.Seed)
            {
                Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
                CropManager.Instance.ExecutePlanting(cellPosition, tm_Ground);
                
                // Trừ đi 1 Hạt giống trong tay
                if (ToolManager.Instance != null) {
                    ToolManager.Instance.ConsumeEquippedItem();
                }
            }
            else
            {
                Debug.Log("Bạn cần phải trang bị một loại 'Hạt giống' (Seed) để gieo trồng!");
            }
        }   

        // Phím M: Xoá đất trồng
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
            TileBase currentTile = tm_Ground.GetTile(cellPosition);
            if (currentTile != null && currentTile != tb_Ground)
            {
                tm_Ground.SetTile(cellPosition, tb_Ground); 
                Debug.Log("Huỷ bỏ ô đất đang trồng tại: " + cellPosition);
            }
        }

        // Phím X: Thu hoạch (Harvesting) / Dọn đồ héo
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Thường thì cày cuốc hay nhổ cỏ không cần tool chuyên biệt (nhổ bằng tay)
            Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
            CropManager.Instance.ExecuteHarvesting(cellPosition, tm_Ground);
        }
        
        // Phím F: Tưới Nước
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (equipped != null && (equipped.itemName.ToLower().Contains("tưới") || equipped.toolId == "water_can"))
            {
                Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
                if (CropManager.Instance != null && tm_Ground != null)
                {
                    CropManager.Instance.ExecuteWatering(cellPosition, tm_Ground);
                }
            }
            else
            {
                Debug.Log("Bạn cần phải trang bị 'Bình Tưới' để tưới nước cho cây!");
            }
        }
        
        // Helper: Nếu vừa vào Map, Player có thể nhấn nút để refresh graphic (Optional)
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Để sau này mở rộng Re-render map tiles khi đổi scene
        }
    }
}