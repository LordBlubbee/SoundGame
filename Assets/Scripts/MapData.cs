using System.Collections.Generic;
using UnityEngine;

public enum MapType
{
    ForestMap = 0,
    MineMap = 1,
}

public class MapData : ScriptableObject
{
    private readonly Dictionary<MapType, Tile[,]> mapCollection = new();

    public void SetMapDimensions(int width, int height)
    {
        forestMap = new Tile[width, height];
        mineMap = new Tile[width, height];
    }

    public bool MapExists(MapType type)
    {
        return mapCollection.ContainsKey(type);
    }

    public void SetMap(MapType type, Tile[,] map)
    {
        if (mapCollection.ContainsKey(type))
        {
            mapCollection[type] = map;
        }
        else
        {
            mapCollection.Add(type, map);
        }
    }

    public Tile[,] ReturnMap(MapType type)
    {
        if (!mapCollection.ContainsKey(type)) { return default; }

        return mapCollection[type];
    }

    public void ClearMapCollection(bool AreYouSure = false, bool AreYouReallySure = false)
    {
        if (!AreYouReallySure || !AreYouSure) { return; }

        mapCollection.Clear();
    }

    private Tile[,] forestMap;

    private Tile[,] mineMap;
}
