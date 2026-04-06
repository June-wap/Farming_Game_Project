using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Quản lý trạng thái của từng ô trồng trọt trên Tilemap.
/// Kéo component này vào một GameObject trong Scene và gán vào
/// trường cropManager của PlayerFarmController.
/// </summary>
public class CropManager : MonoBehaviour
{
    // ─── CROP SETTINGS ────────────────────────────────────────────────────────
    [Header("Crop Settings")]
    [Tooltip("Tên cây trồng mặc định khi chưa có hệ thống giống cụ thể.")]
    public string defaultCropName = "Flower";

    [Tooltip("Mô tả cây trồng mặc định.")]
    public string defaultCropDescription = "Hoa thu hoạch từ ruộng.";

    [Tooltip("Thời gian (phút ingame) để một cây đến ngưỡng thu hoạch.")]
    public int minutesToHarvest = 120;

    // ─── INTERNAL STATE ───────────────────────────────────────────────────────
    // Key = vị trí ô (cell), Value = dữ liệu cây tại ô đó
    private Dictionary<Vector3Int, CropData> _crops = new Dictionary<Vector3Int, CropData>();

    // ─── PUBLIC API (called by PlayerFarmController) ──────────────────────────

    /// <summary>Đăng ký một ô đất vừa được cuốc.</summary>
    public void TillCell(Vector3Int cell)
    {
        if (!_crops.TryGetValue(cell, out CropData data))
        {
            data = new CropData(cell);
            _crops[cell] = data;
        }
        data.state = CropState.Tilled;
        data.isWatered = false;
        Debug.Log($"[CropManager] Ô {cell} → Tilled");
    }

    /// <summary>Gieo hạt vào ô đất đã cuốc.</summary>
    public void PlantSeed(Vector3Int cell)
    {
        if (!_crops.TryGetValue(cell, out CropData data))
        {
            // Tự động tạo nếu chưa có (an toàn khi bypass)
            data = new CropData(cell);
            _crops[cell] = data;
        }
        data.state = CropState.Planted;
        data.plantedTimeInMinutes = 0;
        data.timeToHarvestMinutes = minutesToHarvest;
        data.daysWithoutWater = 0;
        Debug.Log($"[CropManager] Ô {cell} → Planted");
    }

    /// <summary>
    /// Tưới nước cho ô chỉ định.
    /// Trả về true nếu có cây để tưới, false nếu không.
    /// </summary>
    public bool WaterCell(Vector3Int cell)
    {
        if (_crops.TryGetValue(cell, out CropData data) && data.state == CropState.Planted)
        {
            data.isWatered = true;
            data.daysWithoutWater = 0;
            Debug.Log($"[CropManager] Ô {cell} → Watered");
            return true;
        }
        return false;
    }

    /// <summary>Kiểm tra xem cây tại ô có thể thu hoạch không.</summary>
    public bool IsHarvestable(Vector3Int cell)
    {
        if (_crops.TryGetValue(cell, out CropData data))
            return data.state == CropState.Harvestable;
        return false;
    }

    /// <summary>Lấy tên cây tại ô (dùng khi thu hoạch).</summary>
    public string GetCropName(Vector3Int cell)
    {
        // Mở rộng sau: tra cứu từ ScriptableObject / database.
        return defaultCropName;
    }

    /// <summary>Lấy mô tả cây tại ô (dùng khi thu hoạch).</summary>
    public string GetCropDescription(Vector3Int cell)
    {
        return defaultCropDescription;
    }

    /// <summary>Thu hoạch cây tại ô — xóa dữ liệu khỏi danh sách, đặt ô về Tilled.</summary>
    public void HarvestCell(Vector3Int cell)
    {
        if (_crops.TryGetValue(cell, out CropData data))
        {
            data.state = CropState.Tilled;
            data.isWatered = false;
            data.plantedTimeInMinutes = 0;
            Debug.Log($"[CropManager] Ô {cell} → Harvested, đặt lại thành Tilled");
        }
    }

    /// <summary>Xóa hoàn toàn dữ liệu cây tại ô (dùng khi xóa tile thủ công).</summary>
    public void ClearCell(Vector3Int cell)
    {
        if (_crops.Remove(cell))
            Debug.Log($"[CropManager] Ô {cell} → Cleared");
    }

    // ─── GROWTH TICK ──────────────────────────────────────────────────────────
    /// <summary>
    /// Gọi hàm này từ TimeManager mỗi phút ingame để cây lớn lên.
    /// Tên gọi: OnGameMinuteTick()
    /// </summary>
    public void OnGameMinuteTick()
    {
        foreach (var pair in _crops)
        {
            CropData data = pair.Value;

            if (data.state == CropState.Planted)
            {
                if (data.isWatered)
                {
                    data.plantedTimeInMinutes++;
                    data.isWatered = false; // Cần tưới lại mỗi ngày

                    if (data.plantedTimeInMinutes >= data.timeToHarvestMinutes)
                    {
                        data.state = CropState.Harvestable;
                        Debug.Log($"[CropManager] Ô {pair.Key} → Harvestable!");
                    }
                }
                else
                {
                    data.daysWithoutWater++;
                    if (data.daysWithoutWater >= 2)
                    {
                        data.state = CropState.Wilted;
                        Debug.LogWarning($"[CropManager] Ô {pair.Key} → Wilted (khô héo)!");
                    }
                }
            }
        }
    }
}
