using PolyAndCode.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CellItemData : MonoBehaviour, ICell
{
    //UI 
    public Text nameLabel;
    public Text desLabel;

    private IvenItems _contactInfo;

    private int _cellIndex;

    public void ConfigureCell(IvenItems ivenItems, int cellIndex)
    {
        _contactInfo = ivenItems;
        _cellIndex = cellIndex;

        nameLabel.text = ivenItems.itemName;
        desLabel.text = ivenItems.itemDescription;
    }

    // Gắn thuộc tính này vào OnClick() Event của Button trên UI Prefab (Image.prefab)
    public void OnEquipButtonClicked()
    {
        if (_contactInfo != null)
        {
            if (ToolManager.Instance != null)
            {
                ToolManager.Instance.EquipItem(_contactInfo);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy ToolManager. Cần tạo GameObject chứa ToolManager trong Scene!");
            }
        }
    }
}