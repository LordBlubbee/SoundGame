using UnityEngine;

public class Player : Entity
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        InputHandling();
    }

    private void InputHandling()
    {
        if (!CurrentTurn) { return; }

        if (Input.GetKeyUp(KeyCode.W))
        {
            PlayerMovement(0, 1);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            PlayerMovement(0, -1);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            PlayerMovement(-1, 0);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            PlayerMovement(1, 0);
        }
    }

    private void PlayerMovement(int x, int y)
    {
        gameManager.GridManager.MoveEntityInGrid(this, new Vector2Int(x, y));
        gameManager.TurnManager.ChangeTurn();
    }
}