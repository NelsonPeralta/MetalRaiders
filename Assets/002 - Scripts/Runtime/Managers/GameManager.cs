using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.UI;

//# https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html

//#if UNITY_EDITOR
//Debug.unityLogger.logEnabled = false;
//#else
//Debug.unityLogger.logEnabled = false;
//#endif

public class GameManager : MonoBehaviourPunCallbacks
{
    // https://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods


    // Events
    public delegate void GameManagerEvent();
    public GameManagerEvent OnSceneLoadedEvent, OnCameraSensitivityChanged;
    // Enums
    public enum GameMode { Multiplayer, Swarm, Unassigned }
    public enum GameType
    {
        Fiesta, Rockets, Slayer, Pro, Snipers, Survival, Unassgined,
        Shotguns
    }
    public enum ArenaGameType { Fiesta, Slayer, Pro, Snipers, Shotguns }
    public enum CoopGameType { Survival }
    public enum TeamMode { Classic, None }

    public List<int> arenaLevelIndexes = new List<int>();

    [SerializeField] int targetFPS;

    // Intances
    public static GameManager instance;

    public Dictionary<int, PlayerMultiplayerMatchStats.Team> controllerId_TeamDict;
    public Dictionary<int, Player> pid_player_Dict = new Dictionary<int, Player>();
    public Dictionary<int, Player> localPlayers = new Dictionary<int, Player>();

    [SerializeField] GameMode _gameMode;
    [SerializeField] GameType _gameType;
    [SerializeField] TeamMode _teamMode;
    [SerializeField] PlayerMultiplayerMatchStats.Team _onlineTeam;
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
        set
        {
            _teamMode = value;
            if (value == TeamMode.None)
            {
                onlineTeam = PlayerMultiplayerMatchStats.Team.None;
                FindObjectOfType<Launcher>().teamRoomUI.SetActive(false);

            }
            else
            {
                FindObjectOfType<Launcher>().teamRoomUI.SetActive(true);
            }
        }
    }

    [Header("Ammo Packs")]
    public Transform grenadeAmmoPack;
    public Transform lightAmmoPack;
    public Transform heavyAmmoPack;
    public Transform powerAmmoPack;

    [SerializeField] int _nbPlayers;
    public int NbPlayers { get { return _nbPlayers; } set { _nbPlayers = value; } }

    int _camSens = 100;

    public bool isDev;
    [SerializeField] int _sceneIndex = 0;
    public static int sceneIndex
    {
        get { return instance._sceneIndex; }
        set { instance._sceneIndex = value; }
    }
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

    public PlayerMultiplayerMatchStats.Team onlineTeam
    {
        get { return instance._onlineTeam; }
        set
        {
            _onlineTeam = value;
            try
            {
                FindObjectOfType<Launcher>().teamModeText.text = $"Team Mode: {teamMode}";
                if (teamMode == TeamMode.Classic)
                    FindObjectOfType<Launcher>().teamText.text = $"Team: ({_onlineTeam.ToString()})";
            }
            catch { }
        }
    }
    //public string rootPlayerNickname
    //{
    //    get { return GetComponent<PhotonView>()}
    //}


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
        try { pid_player_Dict.Clear(); } catch { }
        try { localPlayers.Clear(); } catch { }
        try { FindObjectOfType<GameTime>().totalTime = 0; }
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
                for (int i = 0; i < NbPlayers; i++)
                {
                    Transform spawnpoint = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint();
                    Player player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Online Player V10"), spawnpoint.position + new Vector3(0, 2, 0), spawnpoint.rotation).GetComponent<Player>();
                    player.GetComponent<PlayerController>().rid = i;
                }
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }

            try
            {
                Debug.Log($"Is there a Player Manager: {PlayerManager.playerManagerInstance}");
                //if (!PlayerManager.playerManagerInstance)
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
                if (!GameObjectPool.gameObjectPoolInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ObjectPool"), Vector3.zero, Quaternion.identity);
                //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineWeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
                //if (!OnlineGameTime.onlineGameTimeInstance)
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkGameTime"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }

            try
            {
                int c = 0;
                foreach (NetworkWeaponSpawnPoint nws in FindObjectsOfType<NetworkWeaponSpawnPoint>())
                {

                }
            }
            catch { }
        }
        else
        {
            FindObjectOfType<Launcher>().OnCreateSwarmRoomButton += OnCreateSwarmRoomButton_Delegate;
            FindObjectOfType<Launcher>().OnCreateMultiplayerRoomButton += OnCreateMultiplayerRoomButton_Delegate;
            FindObjectOfType<GameTime>().totalTime = 0;
        }
        OnSceneLoadedEvent?.Invoke();
    }

    // called third
    private void Start()
    {

        Debug.Log("GameManager Start called");

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 200;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
                  Debug.unityLogger.logEnabled = false;
#endif


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
        if (targetFPS != Application.targetFrameRate)
            Application.targetFrameRate = targetFPS;

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

    public static Player GetMyPlayer(int controllerId = 0)
    {
        foreach (Player p in FindObjectsOfType<Player>())
            if (p.GetComponent<PhotonView>().IsMine && controllerId == p.controllerId)
                return p;
        return null;
    }

    public static Player GetPlayerWithPhotonViewId(int pid)
    {
        return instance.pid_player_Dict[pid];
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


    // Custom List
    #region
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

    public int CalculateLengthOfString(string message, Text text)
    {
        int totalLength = 0;

        Font myFont = text.font;  //chatText is my Text component
        CharacterInfo characterInfo = new CharacterInfo();

        char[] arr = message.ToCharArray();

        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out characterInfo, text.fontSize);

            totalLength += characterInfo.advance;
        }

        return totalLength;
    }
    #endregion

    public static Player GetLocalMasterPlayer()
    {
        foreach (Player p in FindObjectsOfType<Player>())
        {
            if (p.PV.IsMine && p.rid == 0)
                return p;
        }
        return null;
    }

    public void ChangeNbLocalPlayers(string n)
    {
        NbPlayers = int.Parse(n);
    }

    public static int GetNextTiming(int tts)
    {
        int _time = FindObjectOfType<GameTime>().totalTime;

        int timeLeft = 0;

        if (_time < tts)
            timeLeft = tts - _time;
        else
            timeLeft = tts - (_time % tts);
        Debug.Log(timeLeft);

        return timeLeft;
    }
}