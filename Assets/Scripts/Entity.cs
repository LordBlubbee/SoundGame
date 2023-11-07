using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool IsCreature = false;

    public bool CurrentTurn;

    public Vector2Int Position = Vector2Int.zero;

    public int TurnIndex = 0;

    public bool IsPlayer = false;

    [Tooltip("Used for turn management.")]
    public bool IsActive = false;

    public bool HasVisitedHouseOrTree = false;
}
