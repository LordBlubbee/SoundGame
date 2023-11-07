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

    [SerializeField] private Vector2Int MapSize = new();

    [SerializeField] private List<Tile> predefinedTilesForest = new();
    [SerializeField] private List<Tile> predefinedTilesMine = new();

    [Tooltip("Which tile Types are used in the random generation.")]
    [SerializeField] private List<TileType> randomTileTypesForest = new();
    [Tooltip("Which tile Types are used in the random generation.")]
    [SerializeField] private List<TileType> randomTileTypesMine = new();

    private List<Entity> entities = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AudioManager = GetComponent<AudioManager>();

        GridManager = new(MapSize.x, MapSize.y, predefinedTilesForest, predefinedTilesMine, randomTileTypesForest, randomTileTypesMine, this);
        GridManager.OnMoveEntity += AudioManager.OnMovement;

        EventManager.AddListener(EventType.StartGame, () => GridManager.GameStarted = true);
        EventManager.AddListener(EventType.Pause, () => GridManager.GamePaused = true);
        EventManager.AddListener(EventType.UnPause, () => GridManager.GamePaused = false);

        //Just to make sure the player is at the start, for convenience.
        Player player = FindObjectOfType<Player>();
        entities.Add(player);
        AudioManager.SetPlayer(player);
        EventManager.AddListener(EventType.StartGame, () => player.StartGame());

        Enemy[] entitiesArray = FindObjectsOfType<Enemy>();
        foreach (Enemy entity in entitiesArray)
        {
            if (entities.Contains(entity)) { continue; }
            entities.Add(entity);
        }

        TurnManager = new();
        TurnManager.AddEntitiesToList(ref entities);
    }

    //Called through the input script
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