using System;
using System.Collections.Generic;

[Serializable]
public class MapSaveData
{
    public string mapName;
    public List<CropData> crops = new List<CropData>();
}
