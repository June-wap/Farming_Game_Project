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
}