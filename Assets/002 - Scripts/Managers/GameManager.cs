using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.IO;

//# https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html

public class GameManager : MonoBehaviourPunCallbacks
{
    // Events
    public delegate void GameManagerEvent();
    public GameManagerEvent OnSceneLoadedEvent, OnCameraSensitivityChanged;
    // Enums
    public enum GameMode { Multiplayer, Swarm, Unassigned }
    public enum MultiplayerMode { Deathmatch, Unassgined }
    public enum SwarmMode { Survival, Unassigned }

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

    int _camSens = 100;

    public int sceneIndex = 0;
    public int camSens
    {
        get { return instance._camSens; }
        set
        {
            int previousValue = instance._camSens;

            if (previousValue != value)
            {
                instance._camSens = value;
                OnCameraSensitivityChanged?.Invoke();
            }
        }
    }

    // called zero
    void Awake()
    {
        Debug.Log("GameManager Awake");
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    // called first
    void OnEnable()
    {
        Debug.Log("GameManager OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("GameManager OnSceneLoaded called");
        sceneIndex = scene.buildIndex;

        if (scene.buildIndex > 0) // We're in the game scene
        {
            try
            {
                Debug.Log($"Master CLient: {PhotonNetwork.MasterClient}");
                Debug.Log($"{PhotonNetwork.CurrentRoom.CustomProperties["mode"]}");
                string mode = PhotonNetwork.CurrentRoom.CustomProperties["mode"].ToString();

                Debug.Log($"Is there a Player Manager: {PlayerManager.playerManagerInstance}");
                if (!PlayerManager.playerManagerInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
                if (!GameObjectPool.gameObjectPoolInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ObjectPool"), Vector3.zero, Quaternion.identity);
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineWeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
                //if (!OnlineGameTime.onlineGameTimeInstance)
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkGameTime"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
            }
            catch
            {

            }
        }
        OnSceneLoadedEvent?.Invoke();
    }

    // called third
    private void Start()
    {
        Debug.Log("GameManager Start called");
        //SceneManager.sceneLoaded += OnSceneLoaded;
        Launcher.instance.OnCreateSwarmRoomButton += OnCreateSwarmRoomButton_Delegate;
        Launcher.instance.OnCreateMultiplayerRoomButton += OnCreateMultiplayerRoomButton_Delegate;
    }

    // called when the game is terminated
    void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
            camSens -= 10;
        if (Input.GetKeyDown(KeyCode.Alpha5))
            camSens += 10;
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

        FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    public Player GetMyPlayer()
    {
        foreach (Player p in FindObjectsOfType<Player>())
            if (p.GetComponent<PhotonView>().IsMine)
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
