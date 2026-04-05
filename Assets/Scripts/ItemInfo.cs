using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    [System.Serializable]
    public class IvenItems
    {
        public string itemName;
        public Sprite itemIcon;
        public int quantity;
    }

    public IvenItems itemdata;
    
}
