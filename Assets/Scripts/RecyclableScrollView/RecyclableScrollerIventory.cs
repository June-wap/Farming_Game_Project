using UnityEngine;
using PolyAndCode.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class RecyclableScrollerIventory : MonoBehaviour , IRecyclableScrollRectDataSource
{
    [SerializeField] 

    RecyclableScrollRect _recyclableScrollRect; 

    [SerializeField] 
    private int _dataLength; 
    
    public GameObject inventoryGameObject;


    //Dummy data List 

    private List<IvenItems> _contactList = new List<IvenItems>(); 


    //Recyclable scroll rect's data source must be assigned in Awake. 

    private void Awake() 

    { 
        _recyclableScrollRect.DataSource = this; 

    } 

    public int GetItemCount() 

    { 

        return _contactList.Count; 

    }

///  

/// Called for a cell every time it is recycled 

/// Implement this method to do the necessary cell configuration. 

/// 


    public void SetCell(ICell cell, int index)
    { 

        //Casting to the implemented Cell 

        var item = cell as CellItemData; 

        item.ConfigureCell(_contactList[index],index); 
    } 

    public void Start()
    {
        List<IvenItems> items = new List<IvenItems>();
        for (int i = 0; i < 50; i++)
        {
            IvenItems ivenItems = new IvenItems();
            ivenItems.itemName = "Name" + i.ToString();
            ivenItems.itemDescription = "Description for " + ivenItems.itemName;
            items.Add(ivenItems);
        
        }  
        SetList(items);
        _recyclableScrollRect.ReloadData(); 
    }

    public void SetList(List<IvenItems> items)
    {
        _contactList = items;
    }

    private void Update()
    {
        if (Input .GetKeyDown(KeyCode.Space))
        {
            List<IvenItems> items = new List<IvenItems>();
            for (int i = 0; i < 50; i++)
            {
                IvenItems ivenItems = new IvenItems(name:"ca",description:"ca" );
                ivenItems.itemName = "New Name" + i.ToString();
                ivenItems.itemDescription = "New Description for " + ivenItems.itemName;
                items.Add(ivenItems);
            }
            SetList(items);
            _recyclableScrollRect.ReloadData();
        }
        if (Input.GetKeyDown(KeyCode.B)) //bag
        {
            inventoryGameObject.SetActive(!inventoryGameObject.activeSelf);
        }
    } 

    public void AddIventoryItem(IvenItems item)
    {
        _contactList.Add(item);
        _recyclableScrollRect.ReloadData();
    }
}
