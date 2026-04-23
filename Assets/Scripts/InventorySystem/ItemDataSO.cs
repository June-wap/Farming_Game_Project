using UnityEngine;

// ItemDataSO (Data Layer): ScriptableObject lưu thông tin tĩnh của item (ID, Tên, Hình, Sức chứa).
[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemDataSO : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite image;
    public int maxCapacity = 99; // Số lượng cộng dồn tối đa trên 1 ô
}
