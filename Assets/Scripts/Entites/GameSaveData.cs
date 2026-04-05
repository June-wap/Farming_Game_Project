using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<IvenItems> items = new List<IvenItems>();
}

[Serializable]
public class MapSaveData
{
    public string mapName;
    public List<CropData> crops = new List<CropData>();
}
