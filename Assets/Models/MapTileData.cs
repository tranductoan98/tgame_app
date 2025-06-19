using System;

[Serializable]
public class MapTileData {
    public int id;
    public int mapId;
    public int imageId;
    public int x, y, w, h;
    public int renderX, renderY;
    public bool isWalkable;
    public string type;
    public int zorder;
}