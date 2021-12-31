using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Enums
    public enum GameMode { Multiplayer, Swarm, None }

    // Intances
    public static GameManager instance;

    // Public variables
    public GameMode gameMode;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    private void Start()
    {
        Launcher.launcherInstance.OnCreateSwarmRoomButton += OnCreateSwarmRoomButton_Delegate;
    }

    void OnCreateSwarmRoomButton_Delegate(Launcher launcher)
    {
        gameMode = GameMode.Swarm;
    }
}
