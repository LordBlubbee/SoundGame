using UnityEngine;

public class UnlockRadar : MonoBehaviour
{
    public void UnlockRadarFunction()
    {
        EventManager.InvokeEvent(EventType.UnlockRadar);
    }
}
