using System;
using System.Diagnostics;
using UnityEngine;

public static class Log
{
    public static bool Enabled =>
        GameManager.instance && !GameManager.instance.disableAllLogs;

    [Conditional("UNITY_EDITOR")]
    public static void Print(Func<object> message)
    {
        if (Enabled)
            UnityEngine.Debug.Log(message());
    }

    [Conditional("UNITY_EDITOR")]
    public static void PrintWarning(Func<object> message)
    {
        if (Enabled)
            UnityEngine.Debug.LogWarning(message());
    }

    [Conditional("UNITY_EDITOR")]
    public static void PrintError(Func<object> message)
    {
        if (Enabled)
            UnityEngine.Debug.LogError(message());
    }

    public static void PrintInBuildAlso(Func<object> message)
    {
        if (Enabled)
            UnityEngine.Debug.Log(message());
    }

    public static void PrintWarningInBuildAlso(Func<object> message)
    {
        if (Enabled)
            UnityEngine.Debug.LogWarning(message());
    }

    public static void PrintErrorInBuildAlso(Func<object> message)
    {
        if (Enabled)
            UnityEngine.Debug.LogError(message());
    }
}
