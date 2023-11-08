using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    private GameManager gameManager;

    private bool gamePaused = false;
    private bool spaceShipEvent = false;
    private bool skipEnemyTurn = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void SetToRandomNeighbourOfPlayer()
    {
        if (skipEnemyTurn)
        {
            skipEnemyTurn = false;
            return;
        }

        List<Tile> neighbourTiles = gameManager.GridManager.ReturnNeighbours(gameManager.Player.Position);

        gameManager.GridManager.ReturnTile(Position).EntitiesInTile.Clear();

        Tile randomTile = neighbourTiles[Random.Range(0, neighbourTiles.Count)];
        randomTile.EntitiesInTile.Remove(this);

        Position = randomTile.Position;
        gameManager.GridManager.SetEnemyPosition(this);

        EventManager.InvokeEvent(EventType.ReloadSoundObjects);
    }

    public void SpawnEnemy()
    {
        IsActive = true;
        gameManager.GridManager.SetEnemyPosition(this);
    }

    private void OnEnable()
    {
        EventManager.AddListener(EventType.Pause, () => gamePaused = true);
        EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
        EventManager.AddListener(EventType.ShipEncounter, () => spaceShipEvent = true);
        EventManager.AddListener(EventType.SkipEnemyTurn, () => skipEnemyTurn = true);
    }

    private void Update()
    {
        if (!CurrentTurn || gamePaused) { return; }

        if (skipEnemyTurn)
        {
            skipEnemyTurn = false;
            gameManager.TurnManager.ChangeTurn();
            return;
        }

        if (spaceShipEvent)
        {
            //SetToRandomNeighbourOfPlayer();
        }
        else
        {
            List<Tile> neighbourTiles = gameManager.GridManager.ReturnNeighbours(Position);

            Tile randomTile = neighbourTiles[Random.Range(0, neighbourTiles.Count)];

            Tile previousTile = gameManager.GridManager.ReturnTile(Position);
            previousTile.EntitiesInTile.Remove(this);

            Debug.Log($"Current Position = {Position}, Tile Position = {randomTile.Position}");
            Vector2Int movementToTile = (Position - randomTile.Position) * -1;
            Debug.Log(movementToTile);
            gameManager.GridManager.MoveEntityInGrid(this, movementToTile);
        }

        gameManager.TurnManager.ChangeTurn();
    }
}