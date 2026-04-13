using System;
using System.Collections.Generic;
using UnityEngine;

// ─── DỮ LIỆU 1 Ô TILE ────────────────────────────────────────────────────────
// Lưu tọa độ (x,y) và index của TileBase trong mảng tiles[] của layer tương ứng.
// Dùng tileIndex thay vì tên tile để tiết kiệm kích thước JSON.
[Serializable]
public class TilemapCellData
{
    public int x;
    public int y;
    public int tileIndex; // vị trí trong mảng TilemapLayerEntry.tiles[]
}

// ─── DỮ LIỆU 1 LAYER TILEMAP ─────────────────────────────────────────────────
// Mỗi layer (Ground, Decoration, Water...) được lưu thành 1 object riêng.
[Serializable]
public class TilemapLayerSaveData
{
    public string layerName;
    public List<TilemapCellData> cells = new List<TilemapCellData>();
}
