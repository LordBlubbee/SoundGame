using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GridManager
{
    [SerializeField] private Tile[,] tiles;

    private int mapWidth;
    private int mapHeight;

    public bool GameStarted = false;
    public bool GamePaused = false;

    public event Action<List<SoundObject>, Tile> OnMoveEntity;

    public void OnGameEnd()
    {
        GameStarted = false;
        tiles = null;
        OnMoveEntity = null;
    }

    public GridManager(int mapWidth = 3, int mapHeight = 5, GameManager gameManager = null)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;

        GenerateGrid();
    }

    public void MoveEntityInGrid(Entity entityToMove, Vector2Int Movement, bool player = false)
    {
        if (!GameStarted || GamePaused) { return; }

        Vector2Int futurePosition = entityToMove.Position + Movement;
        Tile currentTile = tiles[entityToMove.Position.x, entityToMove.Position.y];
        currentTile.EntitiesInTile.Remove(entityToMove);

        if (currentTile.EntitiesInTile.Count < 1) { currentTile.HasEntity = false; }

        if (futurePosition.x < 0 || futurePosition.y < 0) { return; }
        if (futurePosition.x > mapWidth - 1 || futurePosition.y > mapHeight - 1) { return; }

        Tile nextTile = tiles[futurePosition.x, futurePosition.y];
        nextTile.EntitiesInTile.Add(entityToMove);

        entityToMove.Position = nextTile.Position;

        OnMoveEntity?.Invoke(ReturnNeighbourSoundObjects(entityToMove.Position), nextTile);

        if (nextTile.EntitiesInTile.Count > 0) { currentTile.HasEntity = true; }
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
                    Type = tiles[position.x - 1, position.y].Type,
                    HasOtherEntity = tiles[position.x - 1, position.y].HasEntity
                };
                soundObjects.Add(soundObject);
            }
        }
        if (position.x + 1 < mapWidth - 1)
        {
            if (tiles[position.x + 1, position.y] != null)
            {
                SoundObject soundObject = new()
                {
                    Direction = Direction.Right,
                    Type = tiles[position.x + 1, position.y].Type,
                    HasOtherEntity = tiles[position.x + 1, position.y].HasEntity
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
                    Type = tiles[position.x, position.y - 1].Type,
                    HasOtherEntity = tiles[position.x, position.y - 1].HasEntity
                };
                soundObjects.Add(soundObject);
            }
        }
        if (position.y + 1 < mapHeight - 1)
        {
            if (tiles[position.x, position.y + 1] != null)
            {
                SoundObject soundObject = new()
                {
                    Direction = Direction.Up,
                    Type = tiles[position.x, position.y + 1].Type,
                    HasOtherEntity = tiles[position.x, position.y + 1].HasEntity
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
                    Type = x == 0 && y == 0 ? TileType.Plains : GetRandomTileType()
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