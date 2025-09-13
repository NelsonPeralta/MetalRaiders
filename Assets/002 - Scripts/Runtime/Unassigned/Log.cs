using System.Diagnostics;

public static class Log
{
    [Conditional("UNITY_EDITOR")]
    public static void Print(object message)
    {
        if (GameManager.instance.disableAllLogs == false)
            UnityEngine.Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR")]
    public static void PrintWarning(object message)
    {
        if (GameManager.instance.disableAllLogs == false)
            UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR")]
    public static void PrintError(object message)
    {
        if (GameManager.instance.disableAllLogs == false)
            UnityEngine.Debug.LogError(message);
    }

    public static void PrintInBuildAlso(object message)
    {
        if (GameManager.instance.disableAllLogs == false)
            UnityEngine.Debug.Log(message);
    }

    public static void PrintWarningInBuildAlso(object message)
    {
        if (GameManager.instance.disableAllLogs == false)
            UnityEngine.Debug.LogWarning(message);
    }

    public static void PrintErrorInBuildAlso(object message)
    {
        if (GameManager.instance.disableAllLogs == false)
            UnityEngine.Debug.LogError(message);
    }
}
