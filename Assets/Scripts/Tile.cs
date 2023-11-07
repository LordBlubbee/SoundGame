using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Swamp = 0,
    River = 1,
    Plains = 2,
    House = 3,
}

[System.Serializable]
public class Tile
{
    public Tile(int x, int y)
    {
        Position = new Vector2Int(x, y);
    }

    public bool Visited = false;
    public bool HasEntity = false;
    public TileType Type = TileType.Plains;

    public List<Entity> EntitiesInTile = new();
    public Vector2Int Position;
}