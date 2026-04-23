using UnityEngine;
using System;

// Wrapper hỗ trợ parse JSON
[Serializable]
public class ItemSaveData
{
    public string id;
    public int quantity;
}

// ItemInventoryBase (Data Layer): Đại diện cho 1 item đang tồn tại trong kho (Có chứa số lượng hiện tại).
public abstract class ItemInventoryBase : MonoBehaviour
{
    public ItemDataSO itemData;
    
    protected int _quantity;
    protected int _maxCapacity;

    public int Quantity => _quantity;
    public int MaxCapacity => _maxCapacity;

    public virtual void Setup(ItemDataSO data, int amount)
    {
        itemData = data;
        _quantity = amount;
        _maxCapacity = data.maxCapacity;
    }

    public virtual void AddQuantity(int amount)
    {
        _quantity += amount;
    }

    public virtual void SetQuantity(int amount)
    {
        _quantity = amount;
    }

    // Đóng gói dữ liệu item này thành chuỗi JSON
    public string dataToString()
    {
        ItemSaveData data = new ItemSaveData
        {
            id = itemData.id,
            quantity = _quantity
        };
        return JsonUtility.ToJson(data); // Tối ưu hoá bằng Unity JSON chuẩn (Thay SimpleJSON cho nhẹ)
    }
}
