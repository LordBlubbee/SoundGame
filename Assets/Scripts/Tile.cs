using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Standard = 0,
    House = 1,
    River = 2,
    Woods = 3,
}

[System.Serializable]
public class Tile
{
    public Tile(int x, int y)
    {
        Position = new Vector2Int(x, y);
    }

    public bool HasEntity = false;
    public TileType Type = TileType.Standard;

    public List<Entity> EntitiesInTile = new();
    public Vector2Int Position;
}