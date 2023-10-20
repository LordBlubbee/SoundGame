using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GridManager
{
    [SerializeField] private Tile[,] tiles;

    private int mapWidth;
    private int mapHeight;

    public event Action<List<SoundObject>> OnMoveEntity;

    public GridManager(int mapWidth = 3, int mapHeight = 5, GameManager gameManager = null)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;

        GenerateGrid();
    }

    public void MoveEntityInGrid(Entity entityToMove, Vector2Int Movement)
    {
        Vector2Int futurePosition = entityToMove.Position + Movement;
        Tile currentTile = tiles[entityToMove.Position.x, entityToMove.Position.y];
        currentTile.EntitiesInTile.Remove(entityToMove);

        if (futurePosition.x < 0 || futurePosition.y < 0) { return; }
        if (futurePosition.x > mapWidth - 1 || futurePosition.y > mapHeight - 1) { return; }

        Tile nextTile = tiles[futurePosition.x, futurePosition.y];
        nextTile.EntitiesInTile.Add(entityToMove);
        entityToMove.Position = nextTile.Position;

        OnMoveEntity?.Invoke(ReturnNeighbourSoundObjects(entityToMove.Position));
    }

    private List<SoundObject> ReturnNeighbourSoundObjects(Vector2Int position)
    {
        List<SoundObject> soundObjects = new();

        if (position.x - 1 >= 0)
        {
            if (tiles[position.x - 1, position.y] != null)
            {
                SoundObject soundObject = new()
                {
                    Direction = Direction.Left,
                    Type = tiles[position.x - 1, position.y].Type
                };
                soundObjects.Add(soundObject);
            }
        }
        if (position.x + 1 < mapWidth)
        {
            if (tiles[position.x + 1, position.y] != null)
            {
                SoundObject soundObject = new()
                {
                    Direction = Direction.Right,
                    Type = tiles[position.x - 1, position.y].Type
                };
                soundObjects.Add(soundObject);
            }
        }
        if (position.y - 1 >= 0)
        {
            if (tiles[position.x, position.y - 1] != null)
            {
                SoundObject soundObject = new()
                {
                    Direction = Direction.Down,
                    Type = tiles[position.x - 1, position.y].Type
                };
                soundObjects.Add(soundObject);
            }
        }
        if (position.y + 1 < mapHeight)
        {
            if (tiles[position.x, position.y + 1] != null)
            {
                SoundObject soundObject = new()
                {
                    Direction = Direction.Up,
                    Type = tiles[position.x - 1, position.y].Type
                };
                soundObjects.Add(soundObject);
            }
        }

        return soundObjects;
    }

    private void GenerateGrid()
    {
        tiles = new Tile[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Tile tile = new(x, y)
                {
                    Type = GetRandomTileType()
                };
                tiles[x, y] = tile;
            }
        }
    }

    private TileType GetRandomTileType()
    {
        return (TileType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(TileType)).Length);
    }
}
