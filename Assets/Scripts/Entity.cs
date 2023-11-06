using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool CurrentTurn;
    public bool Player = false;
    public Vector2Int Position = Vector2Int.zero;
    public int TurnIndex = 0;
    public bool isPlayer = false;
}
