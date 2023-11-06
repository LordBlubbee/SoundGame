using UnityEngine;

public class Player : Entity
{
    private GameManager gameManager;

    private bool gamePaused = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        isPlayer = true;
    }

    private void OnEnable()
    {
        EventManager.AddListener(EventType.Pause, () => gamePaused = true);
        EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
    }

    public void StartGame()
    {
        gameManager.GridManager.MoveEntityInGrid(this, new Vector2Int(0, 0));
    }

    private void Update()
    {
        InputHandling();
    }

    private void InputHandling()
    {
        if (!CurrentTurn || gamePaused) { return; }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            PlayerMovement(0, 1);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            PlayerMovement(0, -1);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PlayerMovement(-1, 0);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
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