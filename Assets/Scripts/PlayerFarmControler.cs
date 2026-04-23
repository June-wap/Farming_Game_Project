using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;


public class PlayerFarmControler : MonoBehaviour
{
    // ─── TILEMAPS ─────────────────────────────────────────────────────────────

    public Tilemap tm_Ground;
    public Tilemap tm_Field;    // Layer Ruong — hiển thị TilledSoil khi cuốc
    public Tilemap tm_Seed;     // Layer Seed   — hiển thị cây trồng
    public Tilemap tm_Water;    // Layer nước   — dùng cho hệ thống câu cá

    // ─── FARM ZONE (BoxCollider2D Trigger) ────────────────────────────────────
    [Header("Vùng Canh Tác Cố Định")]
    [Tooltip("Kéo GameObject có BoxCollider2D (IsTrigger=true) + Tag 'FarmZone' vào đây.")]
    public Collider2D farmZoneCollider;

    // Cờ: Player đang đứng trong vùng canh tác không?
    private bool _isInFarmZone = false;

    // ─── TILES ────────────────────────────────────────────────────────────────
    [Header("Tiles")]
    public TileBase tb_TilledSoil;  // Tile đất đã cuốc (màu nâu)

    // ─── THỜI GIAN ANIMATION ──────────────────────────────────────────────────
    [Header("Animation Timing")]
    public float tillAnimDuration    = 0.8f;
    public float harvestAnimDuration = 0.6f;

    // ─── INTERNAL ─────────────────────────────────────────────────────────────
    private Animator _animator;
    private bool     _isFishing = false;
    private bool     _isBusy    = false;
    public bool      IsBusy     => _isBusy;

    // ─── VÒNG ĐỜI ────────────────────────────────────────────────────────────

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Khi chuyển Map, Tilemap ở Map cũ bị xóa, ta cần tự động tìm lại Tilemap ở Map mới
        AutoFindFarmElements();
    }

    private void Start()
    {
        AutoFindFarmElements();
    }

    private void AutoFindFarmElements()
    {
        // Reset reference cũ
        tm_Ground = null;
        tm_Field = null;
        tm_Seed = null;
        tm_Water = null;
        farmZoneCollider = null;

        // 1. Quét tìm tất cả Tilemap trong Scene mới
        Tilemap[] allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        foreach (var tm in allTilemaps)
        {
            string tmName = tm.gameObject.name.ToLower();
            if (tmName.Contains("ground")) tm_Ground = tm;
            else if (tmName.Contains("ruong") || tmName.Contains("field")) tm_Field = tm;
            else if (tmName.Contains("seed")) tm_Seed = tm;
            else if (tmName.Contains("water")) tm_Water = tm;
        }

        // 2. Vì tm_Field (Ruong) đồng thời là Farm Zone (chứa BoxCollider2D), ta lấy thẳng từ nó luôn
        if (tm_Field != null)
        {
            farmZoneCollider = tm_Field.GetComponent<Collider2D>();
        }

        // 3. Báo cáo lại cho CropManager
        if (CropManager.Instance != null && tm_Seed != null)
        {
            CropManager.Instance.SetActiveTilemap(tm_Seed);
        }
        
        Debug.Log($"[PlayerFarmControler] Auto-Find Tilemaps ở Map mới hoàn tất. Seed: {tm_Seed != null}");
    }

    private void Update()
    {
        HandleFarmAction();
    }

    // ─── XỬ LÝ PHÍM ──────────────────────────────────────────────────────────
    private void HandleFarmAction()
    {
        if (_isFishing)
        {
            bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
                            Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0.1f;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.G) || isMoving)
                CancelFishing();
            return;
        }

        if (_isBusy) return;

        if (Input.GetKeyDown(KeyCode.C)) StartCoroutine(TillRoutine());
        if (Input.GetKeyDown(KeyCode.V)) StartCoroutine(PlantRoutine());
        if (Input.GetKeyDown(KeyCode.X)) StartCoroutine(HarvestRoutine());
        if (Input.GetKeyDown(KeyCode.G)) HandleFish();
        if (Input.GetKeyDown(KeyCode.M)) HandleDebugReset();
    }

    // ─── PHÁT HIỆN PLAYER VÀO / RA VÙNG CANH TÁC ────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FarmZone"))
        {
            _isInFarmZone = true;
            Debug.Log("[Farm] ✅ Đã bước vào vùng canh tác!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("FarmZone"))
        {
            _isInFarmZone = false;
            Debug.Log("[Farm] 🚪 Đã bước ra khỏi vùng canh tác.");
        }
    }

    // ─── [C] CUỐC ĐẤT ─────────────────────────────────────────────────────────
    private IEnumerator TillRoutine()
    {
        // 1. Kiểm tra vùng canh tác
        if (!_isInFarmZone)
        {
            Debug.LogWarning("[Farm] ❌ Phải đứng trong vùng canh tác mới cuốc được!");
            yield break;
        }

        // 2. Kiểm tra nền đất
        Vector3Int groundCell = tm_Ground != null
            ? tm_Ground.WorldToCell(transform.position)
            : Vector3Int.zero;

        if (tm_Ground != null && !tm_Ground.HasTile(groundCell))
        {
            Debug.LogWarning("[Farm] Không có nền đất để cuốc.");
            yield break;
        }

        // 3. Chỉ chặn nếu ô đã là TilledSoil (đã cuốc rồi)
        Vector3Int fieldCell = tm_Field != null
            ? tm_Field.WorldToCell(transform.position)
            : groundCell;

        if (tm_Field != null && tb_TilledSoil != null &&
            tm_Field.GetTile(fieldCell) == tb_TilledSoil)
        {
            Debug.Log("[Farm] Ô đất này đã được cuốc rồi.");
            yield break;
        }

        // 4. Kiểm tra thể lực
        if (StaminaManager.Instance != null && !StaminaManager.Instance.UseStamina(10))
        {
            Debug.LogWarning("[Farm] Không đủ thể lực để cuốc!");
            yield break;
        }

        // 5. Thực thi
        _isBusy = true;
        _animator?.SetTrigger("doTill");
        yield return new WaitForSeconds(tillAnimDuration);

        if (tm_Field != null && tb_TilledSoil != null)
            tm_Field.SetTile(fieldCell, tb_TilledSoil);

        CropManager.Instance?.TillCell(fieldCell);
        Debug.Log($"[Farm] Cuốc đất thành công tại {fieldCell}");

        _isBusy = false;
    }

    // ─── [V] GIEO HẠT ─────────────────────────────────────────────────────────
    private IEnumerator PlantRoutine()
    {
        Vector3Int fieldCell = tm_Field != null
            ? tm_Field.WorldToCell(transform.position)
            : GetPlayerCell();

        // Phải có đất cuốc mới gieo được
        if (tm_Field == null || tm_Field.GetTile(fieldCell) != tb_TilledSoil)
        {
            Debug.Log("[Farm] Phải cuốc đất trước khi gieo hạt!");
            yield break;
        }

        // TODO: Kiểm tra hạt giống trong Inventory mới của bạn ở đây
        // Ví dụ: if (!MyInventory.HasSeed()) yield break;
        string cropNameToGrow = "seed_rice"; // Tạm thời hardcode, thay bằng item từ Inventory mới

        _isBusy = true;
        _animator?.SetTrigger("doPlant");
        yield return new WaitForSeconds(0.5f);

        bool planted = CropManager.Instance?.PlantSeed(fieldCell, cropNameToGrow) ?? false;
        if (planted)
            Debug.Log($"[Farm] Gieo hạt thành công! Cây sẽ lớn thành: {cropNameToGrow}");

        _isBusy = false;
    }

    // ─── [X] THU HOẠCH ────────────────────────────────────────────────────────
    private IEnumerator HarvestRoutine()
    {
        Vector3Int seedCell = tm_Seed != null
            ? tm_Seed.WorldToCell(transform.position)
            : GetPlayerCell();

        if (CropManager.Instance == null || !CropManager.Instance.IsHarvestable(seedCell))
        {
            Debug.Log("[Farm] Không có cây để thu hoạch hoặc cây chưa chín.");
            yield break;
        }

        _isBusy = true;
        _animator?.SetTrigger("doHarvest");
        yield return new WaitForSeconds(harvestAnimDuration);

        string cropName = CropManager.Instance.GetCropName(seedCell) ?? "Crop";
        CropManager.Instance.HarvestCell(seedCell);

        // Xóa vết đất cuốc sau khi nhổ cây
        Vector3Int fieldCell = tm_Field != null
            ? tm_Field.WorldToCell(transform.position)
            : seedCell;
        if (tm_Field != null) tm_Field.SetTile(fieldCell, null);

        // Thêm nông sản thu hoạch được vào Inventory
        if (DataManager.Instant != null && InventoryManager.Instant != null)
        {
            // Tìm dữ liệu của cây trồng dựa vào ID (chính là biến cropName)
            ItemDataSO itemData = DataManager.Instant.GetItemDataByID(cropName);
            if (itemData != null)
            {
                InventoryManager.Instant.setItemOnInventory(itemData, 1);
                Debug.Log($"[Farm] ✅ Thu hoạch và cất vào túi: 1x {itemData.itemName}!");
            }
            else
            {
                Debug.LogWarning($"[Farm] Thu hoạch được '{cropName}' nhưng chưa có ItemDataSO nào trong DataManager mang ID này!");
            }
        }

        _isBusy = false;
    }

    // ─── [G] CÂU CÁ ───────────────────────────────────────────────────────────
    private void HandleFish()
    {
        if (_isFishing || _isBusy) return;

        if (StaminaManager.Instance != null && !StaminaManager.Instance.UseStamina(5))
        {
            Debug.LogWarning("[Farm] Không đủ thể lực để câu cá!");
            return;
        }

        _isFishing = true;
        _animator?.SetTrigger("doFish");
        _animator?.SetBool("isFishing", true);
        StartCoroutine(FishingRoutine());
    }

    private IEnumerator FishingRoutine()
    {
        yield return new WaitForSeconds(1f);

        // Kiểm tra có gần nước không
        bool nearWater = false;
        if (tm_Water != null)
        {
            Vector3Int p = GetPlayerCell();
            Vector3Int[] neighbors = { p, p + Vector3Int.up, p + Vector3Int.down,
                                          p + Vector3Int.left, p + Vector3Int.right };
            foreach (var n in neighbors)
                if (tm_Water.HasTile(n)) { nearWater = true; break; }
        }

        float waitTime = Random.Range(3f, 8f);
        yield return new WaitForSeconds(waitTime);

        _animator?.SetTrigger("fishBite");
        _animator?.SetBool("isFishing", false);
        _isFishing = false;

        if (nearWater)
        {
            string[] possibleFish = { "food_egg", "food_fish_raw 1", "food_fish_raw 2" };
            string caughtFish = possibleFish[Random.Range(0, possibleFish.Length)];
            int amount = Random.Range(1, 3);

            // Thêm cá câu được vào Inventory
            if (DataManager.Instant != null && InventoryManager.Instant != null)
            {
                ItemDataSO fishData = DataManager.Instant.GetItemDataByID(caughtFish);
                if (fishData != null)
                {
                    InventoryManager.Instant.setItemOnInventory(fishData, amount);
                    Debug.Log($"[Farm] 🐟 CÂU ĐƯỢC và cất vào túi: {amount}x {fishData.itemName}!");
                }
                else
                {
                    Debug.LogWarning($"[Farm] Câu được '{caughtFish}' nhưng chưa có ItemDataSO nào trong DataManager mang ID này!");
                }
            }
        }
    }

    private void CancelFishing()
    {
        StopAllCoroutines();
        _isFishing = false;
        _isBusy    = false;
        _animator?.SetBool("isFishing", false);
    }

    // ─── [M] DEBUG RESET ──────────────────────────────────────────────────────
    private void HandleDebugReset()
    {
        Vector3Int cell = GetPlayerCell();
        if (tm_Field != null) tm_Field.SetTile(cell, null);
        if (tm_Seed  != null) tm_Seed.SetTile(cell, null);
        CropManager.Instance?.ClearCell(cell);
        Debug.Log($"[Farm] 🔄 Reset ô {cell}");
    }

    // ─── HELPER ───────────────────────────────────────────────────────────────
    private Vector3Int GetPlayerCell()
    {
        return tm_Ground != null
            ? tm_Ground.WorldToCell(transform.position)
            : Vector3Int.zero;
    }
}
