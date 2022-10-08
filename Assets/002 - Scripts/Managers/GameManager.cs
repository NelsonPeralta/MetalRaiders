using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.IO;
using System.Linq;

//# https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html

public class GameManager : MonoBehaviourPunCallbacks
{
    // Events
    public delegate void GameManagerEvent();
    public GameManagerEvent OnSceneLoadedEvent, OnCameraSensitivityChanged;
    // Enums
    public enum GameMode { Multiplayer, Swarm, Unassigned }
    public enum GameType { Fiesta, Rockets, Slayer, Pro, Snipers, Survival, Unassgined }
    public enum ArenaGameType { Fiesta, Slayer, Pro, Snipers }
    public enum CoopGameType { Survival }
    public enum TeamMode { Classic, None }

    public List<int> arenaLevelIndexes = new List<int>();

    // Intances
    public static GameManager instance;


    [SerializeField] GameMode _gameMode;
    [SerializeField] GameType _gameType;
    [SerializeField] TeamMode _teamMode;
    // Public variables
    public GameMode gameMode
    {
        get { return _gameMode; }
        set { _gameMode = value; Debug.Log($"Game Mode: {gameMode}"); }
    }
    public GameType gameType
    {
        get { return _gameType; }
        set { _gameType = value; }
    }

    public TeamMode teamMode
    {
        get { return _teamMode; }
        set { _teamMode = value; }
    }

    [Header("Ammo Packs")]
    public Transform grenadeAmmoPack;
    public Transform lightAmmoPack;
    public Transform heavyAmmoPack;
    public Transform powerAmmoPack;

    int _camSens = 100;

    public bool isDev;
    public static int sceneIndex = 0;
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
        // https://forum.unity.com/threads/on-scene-change-event-for-dontdestroyonload-object.814299/
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
        base.OnEnable(); // need this for OnRoomPropertiesUpdate to work
        Debug.Log("GameManager OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        try { FindObjectOfType<OnlineGameTime>().totalTime = 0; }
        catch (Exception e) { Debug.LogWarning(e.Message); }

        Debug.Log("GameManager OnSceneLoaded called");

        instance = this;
        sceneIndex = scene.buildIndex;

        if (scene.buildIndex > 0) // We're in the game scene
        {
            try
            {
                Debug.Log($"{PhotonNetwork.CurrentRoom.CustomProperties["gamemode"]}");
                Debug.Log($"{PhotonNetwork.CurrentRoom.CustomProperties["gametype"]}");
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }

            try
            {
                GameManager.instance.gameMode = (GameMode)Enum.Parse(typeof(GameMode), PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString());
                GameManager.instance.gameType = (GameType)Enum.Parse(typeof(GameType), PhotonNetwork.CurrentRoom.CustomProperties["gametype"].ToString());
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }

            try
            {
                Transform spawnpoint = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint();
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Online Player V10"), spawnpoint.position + new Vector3(0, 2, 0), spawnpoint.rotation);
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }

            try
            {
                Debug.Log($"Is there a Player Manager: {PlayerManager.playerManagerInstance}");
                //if (!PlayerManager.playerManagerInstance)
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
                if (!GameObjectPool.gameObjectPoolInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ObjectPool"), Vector3.zero, Quaternion.identity);
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineWeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
                //if (!OnlineGameTime.onlineGameTimeInstance)
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkGameTime"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }
        }
        else
        {
            FindObjectOfType<Launcher>().OnCreateSwarmRoomButton += OnCreateSwarmRoomButton_Delegate;
            FindObjectOfType<Launcher>().OnCreateMultiplayerRoomButton += OnCreateMultiplayerRoomButton_Delegate;
            FindObjectOfType<OnlineGameTime>().totalTime = 0;
        }
        OnSceneLoadedEvent?.Invoke();
    }

    // called third
    private void Start()
    {
        Debug.Log("GameManager Start called");
        //SceneManager.sceneLoaded += OnSceneLoaded;

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

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Transform sp = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint();
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", "ShooterAI"), sp.position + new Vector3(0, 2, 0), sp.rotation);
        }
    }

    void OnCreateSwarmRoomButton_Delegate(Launcher launcher)
    {
        gameMode = GameMode.Swarm;
        gameType = GameType.Survival;
    }

    void OnCreateMultiplayerRoomButton_Delegate(Launcher launcher)
    {
        gameMode = GameMode.Multiplayer;
        gameType = GameType.Slayer;
    }



    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            UpdateRoomSettings();

        Debug.Log($"Player joined room. ({PhotonNetwork.CurrentRoom.PlayerCount})");
    }

    public override void OnJoinedRoom()
    {

    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //Debug.Log("OnRoomPropertiesUpdate");
        //Debug.Log(propertiesThatChanged["gametype"]);
        //Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["gametype"]);
    }

    void UpdateRoomSettings()
    {
        Dictionary<string, string> roomParams = new Dictionary<string, string>();
        roomParams.Add("gamemode", gameMode.ToString());
        roomParams.Add("gametype", gameType.ToString());

        FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    public static Player GetMyPlayer()
    {
        foreach (Player p in FindObjectsOfType<Player>())
            if (p.GetComponent<PhotonView>().IsMine)
                return p;
        return null;
    }

    public static Player GetPlayerWithPhotonViewId(int pid)
    {
        return PhotonView.Find(pid).GetComponent<Player>();
    }

    public static void SetLayerRecursively(GameObject go, int layerNumber, List<int>? ignoreList = null)
    {
        if (ignoreList == null)
            ignoreList = new List<int>();
        // Reference: https://forum.unity.com/threads/change-gameobject-layer-at-run-time-wont-apply-to-child.10091/

        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            if (!ignoreList.Contains(trans.gameObject.layer))
                trans.gameObject.layer = layerNumber;
        }
    }

    public void EnableCameraMaskLayer(Camera camera, string layerName) { camera.cullingMask |= 1 << LayerMask.NameToLayer($"{layerName}"); }
    public void DisableCameraMaskLayer(Camera camera, string layerName) { camera.cullingMask &= ~(1 << LayerMask.NameToLayer($"{layerName}")); }
    public void ToggleCameraMaskLayer(Camera camera, string layerName) { camera.cullingMask ^= 1 << LayerMask.NameToLayer("SomeLayer"); }
    public void LeaveRoom()
    {
        Cursor.visible = true;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    //https://answers.unity.com/questions/1262342/how-to-get-scene-name-at-certain-buildindex.html
    public static string SceneNameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    // https://answers.unity.com/questions/1135506/how-do-i-get-the-name-of-the-active-scene.html
    public static string GetActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/
    // https://docs.microsoft.com/en-us/dotnet/api/system.collections.ilist.remove?view=net-6.0#system-collections-ilist-remove(system-object)
    [Serializable]
    public class CustomList<T>
    {
        // Declare an array to store the data elements.
        public T[] arr = new T[10];
        protected int nextIndex = 0;
        protected int _count = 0;

        // Define the indexer to allow client code to use [] notation.
        public T this[int i]
        {
            get => arr[i];
            set => arr[i] = value;
        }

        public int Add(T value)
        {
            if (_count < arr.Length)
            {
                arr[_count] = value;
                _count++;

                return (_count - 1);
            }

            return -1;
        }

        public void Add(T value, CustomList<T> customList)
        {
            Add(value);
            Debug.Log("ser");
            CustomList<T> cl = new CustomList<T>();
            cl = this;
            customList = cl;
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(arr[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Remove(T value, CustomList<T> customList)
        {
            RemoveAt(IndexOf(value));
            customList = this;
        }

        public void RemoveAt(int index)
        {
            if ((index >= 0) && (index < Count))
            {
                for (int i = index; i < Count - 1; i++)
                {
                    arr[i] = arr[i + 1];
                }
                _count--;
            }
        }

        public bool Contains(T value)
        {
            foreach (T item in arr)
                if (EqualityComparer<T>.Default.Equals(item, value))
                    return true;
            return false;
        }

        public void Clear(CustomList<T> customList)
        {
            arr = null;
            _count = 0;
            customList = this;
        }
        public int Count => arr.Length;
    }
}
