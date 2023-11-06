using System.Collections.Generic;
using System;

public enum EventType
{
    Pause = 0,
    UnPause = 1,
    StartGame = 3,
    EventStart = 4,
    EventStop = 5,
    UnlockRadar = 6,
}

public static class EventManager
{
    private static Dictionary<EventType, Action> Events = new();

    public static void AddListener(EventType type, Action action)
    {
        if (Events.ContainsKey(type))
        {
            Events[type] += action;
        }
        else
        {
            Events.Add(type, action);
        }
    }

    public static void ClearEvents(bool AreYouSure = false)
    {
        if (!AreYouSure) { return; }

        Events.Clear();
    }

    public static void RemoveListener(EventType type, Action action)
    {
        if (!Events.ContainsKey(type)) { return; }

        Events[type] -= action;
    }

    public static void InvokeEvent(EventType type)
    {
        if (!Events.ContainsKey(type)) { return; }

        Events[type]?.Invoke();
    }
}