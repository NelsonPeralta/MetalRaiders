using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Events
    public delegate void GameManagerEvent();
    public GameManagerEvent OnSceneLoadedEvent;
    // Enums
    public enum GameMode { Multiplayer, Swarm, Unassigned }
    public enum MultiplayerMode { Deathmatch, Unassgined}
    public enum SwarmMode { Survival, Unassigned}

    // Intances
    public static GameManager instance;

    // Public variables
    public GameMode gameMode;
    public MultiplayerMode multiplayerMode;
    public SwarmMode swarmMode;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
        Launcher.launcherInstance.OnCreateSwarmRoomButton += OnCreateSwarmRoomButton_Delegate;
        Launcher.launcherInstance.OnCreateMultiplayerRoomButton += OnCreateMultiplayerRoomButton_Delegate;

        OnSceneLoadedEvent?.Invoke(); // First call when starting the game
    }

    void OnCreateSwarmRoomButton_Delegate(Launcher launcher)
    {
        gameMode = GameMode.Swarm;
        swarmMode = SwarmMode.Survival;
    }

    void OnCreateMultiplayerRoomButton_Delegate(Launcher launcher)
    {
        gameMode = GameMode.Multiplayer;
        multiplayerMode = MultiplayerMode.Deathmatch;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        OnSceneLoadedEvent?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            UpdateRoomSettings();
    }

    public override void OnJoinedRoom()
    {

    }

    void UpdateRoomSettings()
    {
        Dictionary<string, string> roomParams = new Dictionary<string, string>();
        roomParams.Add("gamemode", gameMode.ToString());
        roomParams.Add("multiplayermode", multiplayerMode.ToString());
        roomParams.Add("swarmmode", swarmMode.ToString());

        GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    [PunRPC]
    void UpdateRoomSettings_RPC(Dictionary<string, string> roomParams)
    {
        try
        {
            instance.gameMode = (GameMode)System.Enum.Parse(typeof(GameMode), roomParams["gamemode"]);
            instance.multiplayerMode = (MultiplayerMode)System.Enum.Parse(typeof(MultiplayerMode), roomParams["multiplayermode"]);
            instance.swarmMode = (SwarmMode)System.Enum.Parse(typeof(SwarmMode), roomParams["swarmmode"]);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No such gamemode. {e}");
        }
    }












    public PlayerProperties GetPlayerWithPhotonViewId(int pid)
    {
        return PhotonView.Find(pid).GetComponent<PlayerProperties>();
    }
}
