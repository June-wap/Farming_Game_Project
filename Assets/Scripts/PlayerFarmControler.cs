using UnityEngine;
using UnityEngine.Tilemaps;

// PlayerFarmControler: Xử lý tương tác nông trại của Player.
// Phím C: Cuốc đất | V: Gieo hạt | F: Tưới nước | X: Thu hoạch | M: Xóa tile (debug)
//
// Kết nối với CropManager (Singleton) để quản lý trạng thái và Growth Stage cây trồng.
// Growth Stage Tiles được quản lý trong CropManager.growthStageTiles — không cần ở đây.
public class PlayerFarmControler : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tm_Ground;

    [Header("Tile References")]
    public TileBase tb_Ground;  // Tile đất bình thường
    public TileBase tb_Field;   // Tile đất đã cuốc (Tilled)
    public TileBase tb_Ruong;   // Tile ruộng đã gieo hạt (placeholder Layer Ground)

    [Header("Dev / Debug")]
    [Tooltip("Tick TRUE khi dev để bỏ qua kiểm tra Tool. Tắt trước khi Release!")]
    public bool bypassToolRequirement = false;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Start()
    {
        // Báo cho CropManager biết Tilemap nào đang active trong scene này.
        // CropManager là Singleton DontDestroyOnLoad nên phải cập nhật mỗi khi
        // vào scene mới vì Tilemap khác nhau ở mỗi scene.
        if (CropManager.Instance != null)
            CropManager.Instance.SetActiveTilemap(tm_Ground);
        else
            Debug.LogWarning("[PlayerFarm] CropManager chưa tồn tại!");
    }

    void Update()
    {
        HandleFarmAction();
    }

    // ─── XỬ LÝ PHÍM NÔNG TRẠI ────────────────────────────────────────────────

    public void HandleFarmAction()
    {
        // ── [C] Cuốc đất (Tilling) ─────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.C))
        {
            Vector3Int cellPos    = GetPlayerCell();
            TileBase   currentTile = tm_Ground.GetTile(cellPos);

            if (currentTile == tb_Ground)
            {
                tm_Ground.SetTile(cellPos, tb_Field);
                CropManager.Instance?.TillCell(cellPos);
                Debug.Log("[Farm] Cuốc đất tại: " + cellPos);
            }
            else
            {
                Debug.Log("[Farm] Ô này không phải đất Ground, không thể cuốc.");
            }
        }

        // ── [V] Gieo hạt (Planting) ────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.V))
        {
            Vector3Int cellPos     = GetPlayerCell();
            TileBase   currentTile = tm_Ground.GetTile(cellPos);

            if (currentTile == tb_Field)
            {
                bool planted = CropManager.Instance?.PlantSeed(cellPos) ?? false;

                if (planted)
                {
                    // CropManager.PlantSeed() đã set tile stage 0 trong CropManager
                    // Chỉ cần đánh dấu layer Ground là tb_Ruong để PlayerController biết
                    tm_Ground.SetTile(cellPos, tb_Ruong);
                    Debug.Log("[Farm] Gieo hạt tại: " + cellPos);
                }
                else if (CropManager.Instance == null)
                {
                    // Dev mode — không có CropManager → chỉ đổi tile
                    tm_Ground.SetTile(cellPos, tb_Ruong);
                    Debug.Log("[Farm] (No CropManager) Gieo hạt tại: " + cellPos);
                }
            }
            else
            {
                Debug.Log("[Farm] Ô này chưa được cuốc. Hãy cuốc đất trước!");
            }
        }

        // ── [F] Tưới nước (Watering) ───────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3Int cellPos = GetPlayerCell();

            if (CropManager.Instance != null)
            {
                bool watered = CropManager.Instance.WaterCell(cellPos);
                Debug.Log(watered
                    ? "[Farm] Đã tưới nước tại: " + cellPos
                    : "[Farm] Không có cây Planted để tưới tại: " + cellPos);
            }
            else
            {
                Debug.Log("[Farm] (No CropManager) Tưới nước tại: " + cellPos);
            }
        }

        // ── [X] Thu hoạch (Harvesting) ─────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.X))
        {
            Vector3Int cellPos     = GetPlayerCell();
            TileBase   currentTile = tm_Ground.GetTile(cellPos);

            if (currentTile != tb_Ruong)
            {
                Debug.Log("[Farm] Không có ruộng để thu hoạch tại: " + cellPos);
                return;
            }

            // Kiểm tra cây đã đủ tuổi thu hoạch chưa (qua CropManager)
            bool canHarvest = CropManager.Instance == null || CropManager.Instance.IsHarvestable(cellPos);

            if (!canHarvest)
            {
                // Hiển thị tiến độ để player biết cần chờ bao lâu
                float progress = CropManager.Instance?.GetGrowthProgress(cellPos) ?? 0f;
                Debug.Log($"[Farm] Cây chưa đến lúc thu hoạch. Tiến độ: {progress * 100:F0}%");
                return;
            }

            // Thu hoạch thành công
            tm_Ground.SetTile(cellPos, tb_Field);
            CropManager.Instance?.HarvestCell(cellPos);

            // Lấy thông tin item từ CropManager
            string cropName = CropManager.Instance?.GetCropName(cellPos) ?? "Flower";
            string cropDesc = CropManager.Instance?.GetCropDescription(cellPos) ?? "Hoa thu hoạch từ ruộng.";

            // Thêm vào Inventory
            IvenItems harvestedItem = new IvenItems(cropName, cropDesc);
            RecyclableScrollerIventory inventory = FindObjectOfType<RecyclableScrollerIventory>();
            if (inventory != null)
            {
                inventory.AddIventoryItem(harvestedItem);
                Debug.Log("[Farm] Thu hoạch thành công: " + cropName);
            }
            else
            {
                Debug.LogWarning("[Farm] Không tìm thấy Inventory!");
            }
        }

        // ── [M] Xóa tile (Debug) ───────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3Int cellPos     = GetPlayerCell();
            TileBase   currentTile = tm_Ground.GetTile(cellPos);

            if (currentTile != null && currentTile != tb_Ground)
            {
                tm_Ground.SetTile(cellPos, tb_Ground);
                CropManager.Instance?.ClearCell(cellPos);
                Debug.Log("[Farm] (Debug) Reset tile về Ground tại: " + cellPos);
            }
            else
            {
                Debug.Log("[Farm] (Debug) Không có tile đặc biệt nào để xóa.");
            }
        }
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private Vector3Int GetPlayerCell()
    {
        return tm_Ground.WorldToCell(transform.position);
    }
}