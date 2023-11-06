using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class GameManager : MonoBehaviour
{
    public GridManager GridManager;
    public TurnManager TurnManager;
    public AudioManager AudioManager;

    [SerializeField] private int indexForRestartingScene = 0;

    [SerializeField] private Vector2Int MapSize = new();

    public event Action OnGameOver;

    public List<Entity> entities = new();

    void Start()
    {
        AudioManager = GetComponent<AudioManager>();

        GridManager = new(MapSize.x, MapSize.y, this);
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
        OnGameOver?.Invoke();
        OnGameOver = null;
    }

    private void OnDisable()
    {
        OnGameEnd();
    }
}