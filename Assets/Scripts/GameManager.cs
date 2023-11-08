using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class GameManager : MonoBehaviour
{
    public GridManager GridManager { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public AudioManager AudioManager { get; private set; }

    [Tooltip("The scene index, used for restarting.")]
    [SerializeField] private int indexForRestartingScene = 0;

    public Vector2Int MapSize = new();

    [SerializeField] private List<Tile> predefinedTilesForest = new();
    [SerializeField] private List<Tile> predefinedTilesMine = new();

    [Tooltip("Which tile Types are used in the random generation.")]
    [SerializeField] private List<TileType> randomTileTypesForest = new();
    [Tooltip("Which tile Types are used in the random generation.")]
    [SerializeField] private List<TileType> randomTileTypesMine = new();

    private List<Entity> entities = new();

    public Player Player;
    private Enemy enemy;

    void Start()
    {
        AudioManager = GetComponent<AudioManager>();

        Player = FindObjectOfType<Player>();
        enemy = FindObjectOfType<Enemy>();

        GridManager = new(MapSize.x, MapSize.y, predefinedTilesForest, predefinedTilesMine, randomTileTypesForest, randomTileTypesMine, ref Player, ref enemy, this);
        GridManager.OnMoveEntity += AudioManager.OnMovement;

        EventManager.AddListener(EventType.StartGame, () => GridManager.GameStarted = true);
        EventManager.AddListener(EventType.Pause, () => GridManager.GamePaused = true);
        EventManager.AddListener(EventType.UnPause, () => GridManager.GamePaused = false);

        Player player = FindObjectOfType<Player>();
        entities.Add(player);
        EventManager.AddListener(EventType.StartGame, player.StartGame);
        EventManager.AddListener(EventType.ResetPlayer, player.StartGame);

        Enemy[] entitiesArray = FindObjectsOfType<Enemy>();
        foreach (Enemy entity in entitiesArray)
        {
            if (entities.Contains(entity)) { continue; }
            entities.Add(entity);
        }

        TurnManager = new();
        TurnManager.AddEntitiesToList(ref entities);
    }

    public void EnableShipEncounter()
    {
        EventManager.InvokeEvent(EventType.ShipEncounter);
    }

    public void RestartGame()
    {
        OnGameEnd();
        UnityEngine.SceneManagement.SceneManager.LoadScene(indexForRestartingScene);
    }

    public void OnGameEnd()
    {
        EventManager.ClearEvents(true);
        GridManager.OnGameEnd();
    }

    public void SwapMap(MapType type)
    {
        GridManager.SwapMap(type);
    }
}