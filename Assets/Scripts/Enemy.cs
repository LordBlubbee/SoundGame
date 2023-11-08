using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    private GameManager gameManager;

    private bool gamePaused = false;
    private bool spaceShipEvent = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void SetToRandomNeighbourOfPlayer()
    {
        List<Tile> neighbourTiles = gameManager.GridManager.ReturnNeighbours(gameManager.Player.Position);

        Tile randomTile = neighbourTiles[Random.Range(0, neighbourTiles.Count)];
        randomTile.EntitiesInTile.Remove(this);
        if (randomTile.EntitiesInTile.Count < 1) { randomTile.HostileEntity = false; }

        Position = randomTile.Position;
        gameManager.GridManager.SetEnemyPosition(this);
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
    }

    private void Update()
    {
        if (!CurrentTurn || gamePaused) { return; }

        if (spaceShipEvent)
        {
            SetToRandomNeighbourOfPlayer();
        }
        else
        {
            List<Tile> neighbourTiles = gameManager.GridManager.ReturnNeighbours(Position);

            Tile randomTile = neighbourTiles[Random.Range(0, neighbourTiles.Count)];

            Debug.Log($"Current Position = {Position}, Tile Position = {randomTile.Position}");
            Vector2Int movementToTile = (Position - randomTile.Position) * -1;
            Debug.Log(movementToTile);

            gameManager.GridManager.MoveEntityInGrid(this, movementToTile);
        }

        gameManager.TurnManager.ChangeTurn();
    }
}