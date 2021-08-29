using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dev 
{
    public static void Log(object message)
    {
        if (!Debug.isDebugBuild)
            return;

        Debug.Log(message);
    }

    public static void LogWarning(object message)
    {
        if (!Debug.isDebugBuild)
            return;

        Debug.LogWarning(message);
    }

    public static void LogError(object message)
    {
        if (!Debug.isDebugBuild)
            return;

        Debug.LogError(message);
    }
}
