using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GridManager
{
    public event Action<List<SoundObject>, Tile, bool> OnMoveEntity;

    public bool GameStarted = false;
    public bool GamePaused = false;

    [SerializeField] private Tile[,] tiles;
    //For Convenience when picking a random Tile.
    private List<Vector2Int> tilePositions = new();

    private List<Tile> predefinedTiles = new();
    private List<Tile> predefinedTilesMines = new();

    private int mapWidth;
    private int mapHeight;

    private List<TileType> tileTypesForRandomGenerationForest = new();
    private List<TileType> tileTypesForRandomGenerationMine = new();

    private MapData mapData;
    private MapType currentMapType = MapType.ForestMap;

    public void OnGameEnd()
    {
        tiles = null;
        GameStarted = false;
        OnMoveEntity = null;
        tileTypesForRandomGenerationMine.Clear();
        tileTypesForRandomGenerationForest.Clear();
        mapData.ClearMapCollection(true, true);
    }

    public GridManager(int mapWidth, int mapHeight, List<Tile> listOfPredefinedTiles, List<Tile> listOfPredefinedTilesMines, List<TileType> randomTileTypesForest, List<TileType> randomTileTypeMine, GameManager gameManager = null)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;

        mapData = ScriptableObject.CreateInstance<MapData>();
        mapData.SetMapDimensions(mapWidth, mapHeight);

        tileTypesForRandomGenerationForest.Clear();
        tileTypesForRandomGenerationForest.AddRange(randomTileTypesForest);

        tileTypesForRandomGenerationMine.Clear();
        tileTypesForRandomGenerationMine.AddRange(randomTileTypeMine);

        predefinedTilesMines.Clear();
        predefinedTilesMines.AddRange(listOfPredefinedTilesMines);

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

    public void SwapMap(MapType mapToSwitchTo)
    {
        EventManager.InvokeEvent(EventType.SwapMap);

        foreach (Tile tile in tiles)
        {
            tile.HostileEntity = false;
            tile.EntitiesInTile.Clear();
        }

        mapData.SetMap(currentMapType, tiles);

        currentMapType = mapToSwitchTo;

        if (mapData.MapExists(mapToSwitchTo))
        {
            tilePositions.Clear();

            tiles = mapData.ReturnMap(mapToSwitchTo);

            foreach (Tile tile in tiles)
            {
                tilePositions.Add(tile.Position);
            }
        }
        else
        {
            currentMapType = mapToSwitchTo;

            GenerateGrid();
        }
    }

    public void MoveEntityInGrid(Entity entityToMove, Vector2Int Movement)
    {
        if (!GameStarted) { return; }

        Vector2Int futurePosition = entityToMove.Position + Movement;
        if (futurePosition.x < 0 || futurePosition.y < 0) { return; }
        if (futurePosition.x > mapWidth - 1 || futurePosition.y > mapHeight - 1) { return; }

        if (!entityToMove.IsPlayer && (tiles[futurePosition.x, futurePosition.y].Type == TileType.House || tiles[futurePosition.x, futurePosition.y].Type == TileType.Shack)) { return; }

        Tile currentTile = tiles[entityToMove.Position.x, entityToMove.Position.y];
        currentTile.EntitiesInTile.Remove(entityToMove);
        currentTile.HostileEntity = currentTile.EntitiesInTile.Count >= 1;

        Tile nextTile = tiles[futurePosition.x, futurePosition.y];
        nextTile.EntitiesInTile.Add(entityToMove);
        nextTile.HostileEntity = nextTile.EntitiesInTile.Count >= 1;

        if (nextTile.HostileEntity && (nextTile.Type != TileType.House && nextTile.Type != TileType.Shack))
        {
            Debug.Log("Attack.");
            EventManager.InvokeEvent(EventType.Attack);
        }

        entityToMove.Position = nextTile.Position;

        OnMoveEntity?.Invoke(ReturnNeighbourSoundObjects(entityToMove.Position), nextTile, entityToMove.IsPlayer);

    }

    public void SetEnemyPosition(Entity entity)
    {
        if (entity.Position.x < 0 || entity.Position.x >= mapWidth || entity.Position.y < 0 || entity.Position.y >= mapHeight) { return; }

        if (entity.IsCreature && !entity.IsPlayer)
        {
            Tile currentTile = tiles[entity.Position.x, entity.Position.y];

            currentTile.EntitiesInTile.Add(entity);
            currentTile.HostileEntity = true;
        }
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
                    HostileEntity = tiles[position.x - 1, position.y].HostileEntity,
                    HasOtherEntity = tiles[position.x - 1, position.y].HasEntity,
                    Tile = tiles[position.x - 1, position.y],
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
                    HostileEntity = tiles[position.x + 1, position.y].HostileEntity,
                    HasOtherEntity = tiles[position.x + 1, position.y].HasEntity,
                    Tile = tiles[position.x + 1, position.y],
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
                    HostileEntity = tiles[position.x, position.y - 1].HostileEntity,
                    HasOtherEntity = tiles[position.x, position.y - 1].HasEntity,
                    Tile = tiles[position.x, position.y - 1],
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
                    HostileEntity = tiles[position.x, position.y + 1].HostileEntity,
                    HasOtherEntity = tiles[position.x, position.y + 1].HasEntity,
                    Tile = tiles[position.x, position.y + 1],
                };
                soundObjects.Add(soundObject);
            }
        }

        return soundObjects;
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
        tilePositions.Clear();

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

        if (currentMapType == MapType.ForestMap)
        {
            foreach (Tile predefinedTile in predefinedTiles)
            {
                tiles[predefinedTile.Position.x, predefinedTile.Position.y] = predefinedTile;
            }
        }
        else
        {
            foreach (Tile predefinedTile in predefinedTilesMines)
            {
                tiles[predefinedTile.Position.x, predefinedTile.Position.y] = predefinedTile;
            }
        }
    }

    private TileType GetRandomTileType()
    {
        TileType randomTileType;

        if (currentMapType == MapType.ForestMap)
        {
            randomTileType = tileTypesForRandomGenerationForest[UnityEngine.Random.Range(0, tileTypesForRandomGenerationForest.Count)];
        }
        else
        {
            randomTileType = tileTypesForRandomGenerationMine[UnityEngine.Random.Range(0, tileTypesForRandomGenerationMine.Count)];
        }

        return randomTileType;
    }
}