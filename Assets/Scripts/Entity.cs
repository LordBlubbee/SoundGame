using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool CurrentTurn;
    public Vector2Int Position = Vector2Int.zero;
    public int TurnIndex = 0;
    public bool IsPlayer = false;
    public bool IsActive = false;
}
