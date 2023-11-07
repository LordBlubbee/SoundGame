using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Swamp = 0,
    River = 1,
    Plains = 2,
    House = 3,
    Mine = 4,
    Body = 5,
    Tree = 6,
    Shack = 7,
    Artifact = 8,
}

[System.Serializable]
public class Tile
{
    public Tile(int x, int y)
    {
        Position = new Vector2Int(x, y);
    }

    [Tooltip("Whether or not it has an Enemy. Gets automatically assigned")]
    public bool HostileEntity = false;

    public bool Visited = false;

    [Tooltip("Whether or not it has an object.")]
    public bool HasEntity = false;

    public TileType Type = TileType.Plains;

    [Tooltip("The creature entities in the tile, enemies, players.")]
    public List<Entity> EntitiesInTile = new();
    public Vector2Int Position;
}