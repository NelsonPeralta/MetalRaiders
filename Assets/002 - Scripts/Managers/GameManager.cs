using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.IO;

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

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {

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
                if (!WeaponPool.weaponPoolInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineWeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
                if (!OnlineGameTime.onlineGameTimeInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineGameTime"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
            }
            catch
            {

            }
        }
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
