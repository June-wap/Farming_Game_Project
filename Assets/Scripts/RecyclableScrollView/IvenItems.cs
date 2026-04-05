using UnityEngine;

public enum ItemCategory
{
    Tool,
    Seed,
    Produce,
    Consumable
}

public class IvenItems
{
    public string itemName{ get; set; }
    public string itemDescription{ get; set; }
    public int quantity { get; set; }
    public int price { get; set; }
    
    // Thuộc tính phân loại cho Module B
    public ItemCategory category { get; set; }
    public string toolId { get; set; } // Ví dụ: "hoe", "water_can", "seed_flower"

    public IvenItems()
    {
        quantity = 1;
        price = 0;
        category = ItemCategory.Produce;
        toolId = "";
    }
    
    public IvenItems(string name, string description, int qty = 1, int prc = 0, ItemCategory cat = ItemCategory.Produce, string id = "")
    {
        itemName = name;
        itemDescription = description;
        quantity = qty;
        price = prc;
        category = cat;
        toolId = id;
    }

    public override string ToString()
    {
        return $"[{category}] {itemName} (x{quantity}): {itemDescription}";
    }
}
