using UnityEngine;

[System.Serializable]
public enum TilemapState
{   
    Ground,
    Water,
    Tree,
    Rock,

    Animal,

}
[System.Serializable]
public class TilemapDetail
{
    public int x { get; set; }
    public int y { get; set; }

    public TilemapState tilemapState { get; set; }

    public TilemapDetail()
    {
        
    }

    public TilemapDetail(int x, int y, TilemapState tilemapState)
    {
        this.x = x;
        this.y = y;
        this.tilemapState = tilemapState;
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
