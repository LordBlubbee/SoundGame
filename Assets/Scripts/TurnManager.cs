using System.Collections.Generic;
using UnityEngine;

public class TurnManager
{
    private int TurnIndex = 0;
    private readonly List<Entity> entities = new();

    public void AddEntitiesToList(ref List<Entity> entities)
    {
        this.entities.Clear();
        this.entities.AddRange(entities);

        this.entities[0].CurrentTurn = true;
        foreach (Entity entity in this.entities)
        {
            if (entity == this.entities[0]) { continue; }
            entity.CurrentTurn = false;
        }

        Debug.Log(string.Join(",", entities));
    }

    public void ChangeTurn()
    {
        if (entities.Count <= 1) { return; }

        Debug.Log(entities[TurnIndex].CurrentTurn);
        entities[TurnIndex].CurrentTurn = false;

        TurnIndex++;
        ResetIsTurn();

        Debug.Log(entities[TurnIndex].CurrentTurn);
        entities[TurnIndex].CurrentTurn = true;
    }

    private void ResetIsTurn()
    {
        if (TurnIndex > entities.Count)
        {
            TurnIndex = 0;
        }
    }
}