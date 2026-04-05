using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;

public class PlayerFarmController : MonoBehaviour
{
    public Tilemap tm_Ground;
    public TileBase tb_Ground;
    public Tilemap tm_Field;

    public TileBase tb_Field;

    public Tilemap tm_Ruong;

    public TileBase tb_Ruong;
    

    public void HandleFarmAction()
    {
        // Implementation for handling farm actions
        if (Input.GetKeyDown(KeyCode.C))       
        {
            //Dao dat
            // Handle the farm action
            Debug.Log("Farm action triggered!");
             // Example: Change the tile at the player's position to a farm tile
            Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
            Debug.Log("Cell Position: " + cellPosition);

            TileBase currentTile = tm_Ground.GetTile(cellPosition);
            if (currentTile == tb_Ground)
            {
                tm_Ground.SetTile(cellPosition, tb_Field);
                Debug.Log("Tile changed to Field at position: " + cellPosition);
            }else

            {
                Debug.Log("Current tile is not Ground. No change made.");
            }

        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
            Debug.Log("Cell Position: " + cellPosition);

            TileBase currentTile = tm_Ground.GetTile(cellPosition);
            if (currentTile == tb_Field)
            {
                tm_Ground.SetTile(cellPosition, tb_Ruong);
                Debug.Log("Tile changed to Ruong at position: " + cellPosition);
            }
            else
            {
                Debug.Log("Current tile is not Field. No change made.");
            }
        }   
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);

            Debug.Log("Cell Position: " + cellPosition);

            TileBase currentTile = tm_Field.GetTile(cellPosition);

            if (currentTile != null)
            {
                tm_Field.SetTile(cellPosition, null); 
            }
            else
            {
                Debug.Log("No tile to remove at position: " + cellPosition);
            }
        }
        if (Input.GetKeyDown(KeyCode.X))  // Thêm phím X cho thu hoạch
        {
            Vector3Int cellPosition = tm_Ground.WorldToCell(transform.position);
            Debug.Log("Cell Position: " + cellPosition);

            TileBase currentTile = tm_Ground.GetTile(cellPosition);

            if (currentTile == tb_Ruong)
            {

                // Thay đổi tile trở lại thành Field sau khi thu hoạch
                tm_Ground.SetTile(cellPosition, tb_Field);
                Debug.Log("Harvested: Tile changed back to Field at position: " + cellPosition);

                //lay item va them vao inventory

                IvenItems harvestedItem = new IvenItems();

                harvestedItem.itemName = "Flower";
                harvestedItem.itemDescription = "A beautiful flower harvested from the field.";
                
                Debug.Log("You have harvested a Flower: A beautiful flower harvested from the field.");
                // Gọi phương thức để thêm item vào inventory
                RecyclableScrollerIventory  inventory = FindObjectOfType<RecyclableScrollerIventory>();
                if (inventory != null)
                {
                    inventory.AddIventoryItem(harvestedItem);
                    Debug.Log("Harvested item added to inventory.");
                }
                else
                {
                    Debug.Log("Inventory not found. Cannot add harvested item.");
                }


                //Nếu tìm thấy inventory, thêm item vào đó
                

            }
            else
            {
                Debug.Log("Current tile is not Ruong. Cannot harvest.");
            }
        }
    }

    
    void Update()
    {
        HandleFarmAction();
    }


}