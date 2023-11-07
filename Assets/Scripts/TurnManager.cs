using System.Collections.Generic;

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
    }

    public void ChangeTurn()
    {
        Entity currentEntity = entities[TurnIndex];

        currentEntity.CurrentTurn = false;
        currentEntity.TurnIndex++;

        TurnIndex++;
        ResetIsTurn();

        while (!entities[TurnIndex].IsActive)
        {
            TurnIndex++;
            ResetIsTurn();
        }

        entities[TurnIndex].CurrentTurn = true;
    }

    private void ResetIsTurn()
    {
        if (TurnIndex >= entities.Count)
        {
            TurnIndex = 0;
        }
    }
}