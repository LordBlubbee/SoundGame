using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class GameManager : MonoBehaviour
{
    public GridManager GridManager;
    public TurnManager TurnManager;
    public AudioManager AudioManager;

    //Will come in handy Later. Unless you'd prefer a more elaborate Event System.
    public event Action OnGameOver;

    public List<Entity> entities = new();

    void Start()
    {
        AudioManager = GetComponent<AudioManager>();

        GridManager = new(3, 5, this);
        GridManager.OnMoveEntity += AudioManager.OnMovement;

        //Just to make sure the player is at the start, for convenience.
        Entity player = FindObjectOfType<Player>();
        entities.Add(player);

        Entity[] entitiesArray = FindObjectsOfType<Entity>();
        foreach (Entity entity in entitiesArray)
        {
            if (entities.Contains(entity)) { continue; }
            entities.Add(entity);
        }

        TurnManager = new();
        TurnManager.AddEntitiesToList(ref entities);
    }
}
