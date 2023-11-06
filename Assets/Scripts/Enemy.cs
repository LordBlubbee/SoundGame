using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    private GameManager gameManager;

    private bool gamePaused = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void SpawnEnemy()
    {
        IsActive = true;
        Position = gameManager.GridManager.GetRandomTile().Position;
    }

    private void OnEnable()
    {
        EventManager.AddListener(EventType.Pause, () => gamePaused = true);
        EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
    }

    private void Update()
    {
        if (!CurrentTurn || gamePaused) { return; }

        List<Tile> neighbourTiles = gameManager.GridManager.ReturnNeighbours(Position);

        Tile randomTile = neighbourTiles[Random.Range(0, neighbourTiles.Count)];

        Debug.Log($"Current Position = {Position}, Tile Position = {randomTile.Position}");
        Vector2Int movementToTile = (Position - randomTile.Position) * -1;
        Debug.Log(movementToTile);

        gameManager.GridManager.MoveEntityInGrid(this, movementToTile);

        Debug.Log(Position);
        gameManager.TurnManager.ChangeTurn();
    }
}