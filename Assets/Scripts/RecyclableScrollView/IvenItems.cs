using UnityEngine;

public class IvenItems
{
    public string itemName{ get; set; }
    public string itemDescription{ get; set; }

    public IvenItems()
    {
        
    }
    public IvenItems(string name, string description)
    {
        itemName = name;
        itemDescription = description;
    }

    public override string ToString()
    {
        return itemName + ": " + itemDescription;
    }
}