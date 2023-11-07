using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GridManager
{
    [SerializeField] private Tile[,] tiles;

    //For Convenience when picking a random Tile.
    private List<Vector2Int> tilePositions = new();

    private List<Tile> predefinedTiles = new();

    private int mapWidth;
    private int mapHeight;

    public bool GameStarted = false;
    public bool GamePaused = false;

    private List<TileType> tileTypesForRandomGeneration = new();

    public event Action<List<SoundObject>, Tile, bool> OnMoveEntity;

    public void OnGameEnd()
    {
        GameStarted = false;
        tiles = null;
        OnMoveEntity = null;
    }

    public GridManager(int mapWidth, int mapHeight, List<Tile> listOfPredefinedTiles, List<TileType> randomTileTypes, GameManager gameManager = null)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;

        tileTypesForRandomGeneration.Clear();
        tileTypesForRandomGeneration.AddRange(randomTileTypes);

        predefinedTiles.Clear();
        predefinedTiles.AddRange(listOfPredefinedTiles);

        GenerateGrid();
    }

    public Tile GetRandomTile()
    {
        Vector2Int position = tilePositions[UnityEngine.Random.Range(0, tilePositions.Count)];
        Tile randomTile = tiles[position.x, position.y];
        return randomTile;
    }

    public void MoveEntityInGrid(Entity entityToMove, Vector2Int Movement)
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

        SetTileVisited(entityToMove.Position, entityToMove.IsPlayer);
        OnMoveEntity?.Invoke(ReturnNeighbourSoundObjects(entityToMove.Position), nextTile, entityToMove.IsPlayer);

        if (nextTile.EntitiesInTile.Count > 0) { currentTile.HasEntity = true; }
    }

    private List<SoundObject> ReturnNeighbourSoundObjects(Vector2Int position)
    {
        List<SoundObject> soundObjects = new();

        if (position.x - 1 >= 0)
        {
            if (tiles[position.x - 1, position.y] != null && !tiles[position.x - 1, position.y].Visited)
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
            if (tiles[position.x + 1, position.y] != null && !tiles[position.x + 1, position.y].Visited)
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
            if (tiles[position.x, position.y - 1] != null && !tiles[position.x, position.y - 1].Visited)
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
            if (tiles[position.x, position.y + 1] != null && !tiles[position.x, position.y + 1].Visited)
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

    public void SetTileVisited(Vector2Int position, bool player)
    {
        if (!player) { return; }

        foreach (Tile predefinedTile in predefinedTiles)
        {
            if (predefinedTile == tiles[position.x, position.y])
            {
                return;
            }
        }

        tiles[position.x, position.y].Visited = true;
    }

    public List<Tile> ReturnNeighbours(Vector2Int position)
    {
        List<Tile> neighbourTiles = new();

        if (position.x - 1 >= 0)
        {
            if (tiles[position.x - 1, position.y] != null)
            {
                neighbourTiles.Add(tiles[position.x - 1, position.y]);
            }
        }
        if (position.x + 1 < mapWidth - 1)
        {
            if (tiles[position.x + 1, position.y] != null)
            {
                neighbourTiles.Add(tiles[position.x + 1, position.y]);
            }
        }
        if (position.y - 1 >= 0)
        {
            if (tiles[position.x, position.y - 1] != null)
            {
                neighbourTiles.Add(tiles[position.x, position.y - 1]);
            }
        }
        if (position.y + 1 < mapHeight - 1)
        {
            if (tiles[position.x, position.y + 1] != null)
            {
                neighbourTiles.Add(tiles[position.x, position.y + 1]);
            }
        }

        return neighbourTiles;
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
                tilePositions.Add(new Vector2Int(x, y));
            }
        }

        foreach (Tile predefinedTile in predefinedTiles)
        {
            tiles[predefinedTile.Position.x, predefinedTile.Position.y] = predefinedTile;
        }
    }

    private TileType GetRandomTileType()
    {
        TileType randomTileType;

        randomTileType = tileTypesForRandomGeneration[UnityEngine.Random.Range(0, tileTypesForRandomGeneration.Count)];

        return randomTileType;
    }
}