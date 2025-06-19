using System;

[Serializable]
public class MapInfo
{
    public int id;
    public string name;
    public int width;
    public int height;
    public int tileWidth;
    public int tileHeight;
    public string type;
    public string description;
    public bool isDefault;
}