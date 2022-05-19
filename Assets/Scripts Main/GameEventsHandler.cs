using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsHandler : MonoBehaviour
{
    public static GameEventsHandler current;

    private void Awake()
    {
        current = this;
    }

    public event Action<string> onDisplayDebugMessage;
    public void DisplayDebugMessage(string message)
    {
        if(onDisplayDebugMessage != null)
        {
            onDisplayDebugMessage(message);
        }
    }
}
