using System.Collections;
using UnityEngine;

public class Player : Entity
{
    public int Health
    {
        get => health;
        set
        {
            health = value;
            if (health <= 0)
            {
                Debug.Log("Game Over.");
                EventManager.InvokeEvent(EventType.GameOver);
                EventManager.InvokeEvent(EventType.Pause);
                StopAllCoroutines();
            }
        }
    }

    [SerializeField] private int health = 3;

    private GameManager gameManager;
    private bool gamePaused = false;
    private bool coroutineIsRunning = false;

    public void StartGame()
    {
        if (coroutineIsRunning) { return; }

        coroutineIsRunning = true;
        StartCoroutine(WaitUntillUnpaused());
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        IsPlayer = true;
    }

    private void OnEnable()
    {
        EventManager.AddListener(EventType.Pause, () => gamePaused = true);
        EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
    }

    private IEnumerator WaitUntillUnpaused()
    {
        while (gamePaused)
        {
            yield return null;
        }
        Vector2Int movementToTile = (Position - Vector2Int.zero) * -1;
        IsActive = true;
        coroutineIsRunning = false;
        gameManager.GridManager.MoveEntityInGrid(this, movementToTile);
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