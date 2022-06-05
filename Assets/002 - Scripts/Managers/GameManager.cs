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

    [Header("Ammo Packs")]
    public Transform grenadeAmmoPack;
    public Transform lightAmmoPack;
    public Transform heavyAmmoPack;
    public Transform powerAmmoPack;
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
        Launcher.instance.OnCreateSwarmRoomButton += OnCreateSwarmRoomButton_Delegate;
        Launcher.instance.OnCreateMultiplayerRoomButton += OnCreateMultiplayerRoomButton_Delegate;

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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
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

        GetComponent<MainMenuCaller>().GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    public Player GetMyPlayer()
    {
        foreach(Player p in FindObjectsOfType<Player>())
            if(p.GetComponent<PhotonView>().IsMine)
                return p;
        return null;
    }

    public Player GetPlayerWithPhotonViewId(int pid)
    {
        return PhotonView.Find(pid).GetComponent<Player>();
    }

    public static void SetLayerRecursively(GameObject go, int layerNumber)
    {
        // Reference: https://forum.unity.com/threads/change-gameobject-layer-at-run-time-wont-apply-to-child.10091/

        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }
}
