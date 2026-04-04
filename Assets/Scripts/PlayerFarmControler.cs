using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using Unity.VisualScripting;

public class PlayerFarmControler : MonoBehaviour
{
    public Tilemap tm_Ground;
    public TileBase tb_Ground;
    public Tilemap tm_Field;

    public TileBase tb_Field;

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

            TileBase crrentTile = tm_Ground.GetTile(cellPosition);
            if (crrentTile == tb_Ground)
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
            //Trong Hoa
            Debug.Log("Planting action triggered!");
             // Example: Change the tile at the player's position to a plant tile
        }
    }

    void Update()
    {
        HandleFarmAction();
    }
}