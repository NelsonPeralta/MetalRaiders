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
using System.Net.Mail;
using UnityEditor;

//# https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html

//#if UNITY_EDITOR
//Debug.unityLogger.logEnabled = false;
//#else
//Debug.unityLogger.logEnabled = false;
//#endif

// Create log files
// https://www.youtube.com/watch?v=B0deytHpajQ&ab_channel=techydevy

public class GameManager : MonoBehaviourPunCallbacks
{
    // https://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods

    public static string ROOT_PLAYER_NAME;

    // Events
    public delegate void GameManagerEvent();
    public GameManagerEvent OnSceneLoadedEvent, OnCameraSensitivityChanged;
    // Enums
    public enum Team { None, Red, Blue }
    public enum Connection { Offline, Online }
    public enum GameMode { Multiplayer, Swarm, Unassigned }
    public enum GameType
    {
        Fiesta, Rockets, Slayer, Pro, Snipers, Unassgined,
        Shotguns, Swat, Retro, GunGame, Hill,

        // Swarm Game Types
        Survival
    }
    public enum ArenaGameType { Fiesta, Slayer, Pro, Snipers, Shotguns }
    public enum CoopGameType { Survival }
    public enum TeamMode { Classic, None }

    public List<int> arenaLevelIndexes = new List<int>();

    // Intances
    public static GameManager instance;
    public static int GameStartDelay = 6;
    public static Dictionary<string, string> colorDict = new Dictionary<string, string>();
    public CarnageReport carnageReport { get { return _carnageReport; } set { _carnageReport = value; } }

    public Dictionary<int, PlayerMultiplayerMatchStats.Team> controllerId_TeamDict;
    public Dictionary<int, Player> pid_player_Dict
    {
        get { return _pid_player_Dict; }
        set
        {
            int previousCount = _pid_player_Dict.Count;
            _pid_player_Dict = value;

            if (pid_player_Dict.Count != previousCount)
            {
                Debug.Log(pid_player_Dict.Count);
                Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
                Debug.Log(instance.localPlayers.Count);

                //if (pid_player_Dict.Count == (PhotonNetwork.CurrentRoom.PlayerCount + GameManager.instance.localPlayers.Count - 1))
                //    GetComponent<CurrentRoomManager>().allPlayersJoined = true;
            }
        }
    }


    public Dictionary<Vector3, Biped> orSpPos_Biped_Dict
    {
        get { return _orSpPos_Biped_Dict; }
        set
        {
            _orSpPos_Biped_Dict = value;

            //foreach (KeyValuePair<Vector3, Biped> attachStat in _orSpPos_Biped_Dict)
            //    Debug.Log($"orSpPos_Biped_Dict: Key {attachStat.Key} has value {attachStat.Value.name}");
        }
    }

    public bool gameStarted
    {
        get { return GetComponent<CurrentRoomManager>().gameStarted; }
    }

    public Dictionary<int, Player> localPlayers = new Dictionary<int, Player>();

    [SerializeField] Connection _connection;
    [SerializeField] GameMode _gameMode;
    [SerializeField] GameType _gameType;
    [SerializeField] TeamMode _teamMode;
    [SerializeField] PlayerMultiplayerMatchStats.Team _onlineTeam;
    // Public variables

    public Connection connection
    {
        get { return _connection; }
        set
        {
            _connection = value;

            if (value == Connection.Online)
            {
                Launcher.instance.LoginWithSteamName();
            }
        }
    }
    public GameMode gameMode
    {
        get { return _gameMode; }
        set
        {
            _gameMode = value;
            Debug.Log($"Game Mode: {gameMode}");

            if (_gameMode == GameMode.Swarm)
            {
                FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(false);
                FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(PhotonNetwork.IsMasterClient);
                FindObjectOfType<Launcher>().levelToLoadIndex = 13;

                gameType = GameType.Survival;
                teamMode = TeamMode.Classic;
                difficulty = SwarmManager.Difficulty.Normal;
            }
            else if (_gameMode == GameMode.Multiplayer)
            {
                if (CurrentRoomManager.instance.roomType != CurrentRoomManager.RoomType.QuickMatch)
                    FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(PhotonNetwork.IsMasterClient);
                else
                    FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(false);

                FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(false);
                FindObjectOfType<Launcher>().levelToLoadIndex = 5;


                //teamDict = new Dictionary<string, int>();
                gameType = GameType.Slayer;
                teamMode = TeamMode.None;
            }
            Launcher.instance.gameModeText.text = $"Game Mode: {_gameMode}";

            if (CurrentRoomManager.instance.roomType != CurrentRoomManager.RoomType.QuickMatch)
                FindObjectOfType<Launcher>().gameModeBtns.SetActive(PhotonNetwork.IsMasterClient);
            else
            {
                Launcher.instance.multiplayerMcComponentsHolder.SetActive(false);
                FindObjectOfType<Launcher>().gameModeBtns.SetActive(false);
                FindObjectOfType<Launcher>().swarmModeBtns.SetActive(false);
                FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(false);
            }
        }
    }
    public GameType gameType
    {
        get { return _gameType; }
        set
        {
            Debug.Log($"GAME TYPE: {value}");

            _gameType = value;
            if (_gameType == GameType.GunGame)
                teamMode = TeamMode.None;

            Launcher.instance.gametypeSelectedText.text = $"Gametype: {_gameType}";
        }
    }

    public TeamMode teamMode
    {
        get { return _teamMode; }
        set
        {
            TeamMode _prev = _teamMode;

            Debug.Log("GAMEMANAGE Team Mode: " + value);
            _teamMode = value;
            Launcher.instance.teamModeText.text = $"Team Mode: {teamMode.ToString()}";
            if (value == TeamMode.None)
            {
                foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.extendedPlayerData) s.team = Team.None;


                onlineTeam = PlayerMultiplayerMatchStats.Team.None;
                FindObjectOfType<Launcher>().teamRoomUI.SetActive(false);
                _teamDict = new Dictionary<string, int>();
            }
            else
            {
                if (gameMode == GameMode.Multiplayer)
                    FindObjectOfType<Launcher>().teamRoomUI.SetActive(true);

                Dictionary<string, int> _teamDict = new Dictionary<string, int>();

                foreach (KeyValuePair<int, Photon.Realtime.Player> kvp in PhotonNetwork.CurrentRoom.Players)
                    if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                    {
                        Team t = Team.Red;

                        if ((kvp.Key % 2 == 0) && gameMode != GameMode.Swarm)
                            t = Team.Blue;

                        //ScriptObjPlayerData spd = CurrentRoomManager.instance.extendedPlayerData.FirstOrDefault(item => item.playerExtendedPublicData.player_id.ToString().Equals( kvp.Value.NickName.Split(char.Parse("-"))[0]));
                        //Debug.Log(spd);
                        //spd.team = t;

                        //CurrentRoomManager.instance.GetPlayerDataWithId(int.Parse(kvp.Value.NickName.Split(char.Parse("-"))[0])).team = t;
                        CurrentRoomManager.instance.GetPlayerDataWithId(int.Parse(kvp.Value.NickName)).team = t;

                        _teamDict.Add(kvp.Value.NickName, (int)t);

                        Debug.Log($"Player {kvp.Value.NickName} is part of {t} team");
                    }
                //GameManager.instance.teamDict = _teamDict;
            }

            foreach (Transform child in Launcher.instance.playerListContent)
            {
                child.GetComponent<PlayerListItem>().UpdateColorPalette();
            }
            //FindObjectOfType<NetworkMainMenu>().UpdatePlayerList();
        }
    }

    public SwarmManager.Difficulty difficulty
    {
        get { return _difficulty; }
        set
        {
            _difficulty = value;
            FindObjectOfType<Launcher>().teamModeText.text = $"Difficulty: {_difficulty.ToString()}";
            NetworkGameManager.instance.SendGameParams();
        }
    }

    [Header("Ammo Packs")]
    public Transform grenadeAmmoPack;
    public Transform lightAmmoPack;
    public Transform heavyAmmoPack;
    public Transform powerAmmoPack;

    [SerializeField] int _nbLocalPlayersPreset;
    public int nbLocalPlayersPreset { get { return _nbLocalPlayersPreset; } set { _nbLocalPlayersPreset = value; } }

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
            Debug.Log("onlineTeam: " + value);

            try
            {
                FindObjectOfType<Launcher>().teamModeText.text = $"Team Mode: {teamMode}";
                if (teamMode == TeamMode.Classic)
                    FindObjectOfType<Launcher>().teamText.text = $"Team: ({_onlineTeam.ToString()})";
            }
            catch { }

            if (value == PlayerMultiplayerMatchStats.Team.None)
            {
                foreach (Transform child in Launcher.instance.playerListContent)
                {
                    child.GetComponent<PlayerListItem>().UpdateColorPalette();
                }
            }
        }
    }

    public string rootPlayerNickname
    {
        get { return WebManager.webManagerInstance.pda.username; }
    }

    public Dictionary<string, int> teamDict
    {
        get
        {
            foreach (KeyValuePair<string, int> attachStat in _teamDict)
                Debug.Log($"Team Dict entry: Key {attachStat.Key} has value {attachStat.Value}");


            return _teamDict;
        }
        set
        {
            Debug.Log("teamDict");
            _teamDict = value;

            foreach (KeyValuePair<string, int> attachStat in _teamDict)
            {
                Debug.Log(attachStat.Key);
                Debug.Log(attachStat.Value);
            }

            foreach (Transform child in Launcher.instance.playerListContent)
            {
                child.GetComponent<PlayerListItem>().UpdateColorPalette();
            }

            if (_teamDict.ContainsKey(rootPlayerNickname))
            {
                Debug.Log("Contains my name");
                onlineTeam = (PlayerMultiplayerMatchStats.Team)_teamDict[rootPlayerNickname];



                //    Destroy(child.gameObject);

                //Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
                //for (int i = 1; i < PhotonNetwork.CurrentRoom.PlayerCount + 1; i++)
                //{
                //    string n = PhotonNetwork.CurrentRoom.Players[i].NickName;
                //    Debug.Log(n);

                //    GameObject plt = Instantiate(Launcher.instance.playerListItemPrefab, Launcher.instance.playerListContent);
                //    plt.GetComponent<PlayerListItem>().SetUp($"{n} ({(PlayerMultiplayerMatchStats.Team)(_teamDict[n])})");
                //}
            }
            else
            {
                _teamDict = new Dictionary<string, int>();
                onlineTeam = PlayerMultiplayerMatchStats.Team.None;

                FindObjectOfType<NetworkMainMenu>().UpdatePlayerList();
            }
        }
    }

    public Material armorMaterial { get { return _armorMaterial; } }
    public List<Texture> colorPaletteTextures { get { return _colorPaletteTextures; } }
    public List<LootableWeapon> lootableWeapons { get { return _lootableWeapons; } }
    public List<NetworkGrenadeSpawnPoint> networkGrenadeSpawnPoints { get { return _networkGrenadeSpawnPoints; } }

    // called zero

    // private Variables
    [SerializeField] WeaponPool _weaponPoolPrefab;
    [SerializeField] RagdollPool _ragdollPoolPrefab;
    [SerializeField] Dictionary<int, Player> _pid_player_Dict = new Dictionary<int, Player>();
    [SerializeField] Dictionary<Vector3, Biped> _orSpPos_Biped_Dict = new Dictionary<Vector3, Biped>();
    [SerializeField] Dictionary<string, int> _teamDict = new Dictionary<string, int>();
    [SerializeField] Dictionary<string, PlayerDatabaseAdaptor> _roomPlayerData = new Dictionary<string, PlayerDatabaseAdaptor>();
    [SerializeField] Material _armorMaterial;
    [SerializeField] List<Texture> _colorPaletteTextures = new List<Texture>();
    [SerializeField] List<ScriptObjBipedTeam> _teamsData;

    SwarmManager.Difficulty _difficulty;
    CarnageReport _carnageReport;
    List<LootableWeapon> _lootableWeapons = new List<LootableWeapon>();
    List<NetworkGrenadeSpawnPoint> _networkGrenadeSpawnPoints = new List<NetworkGrenadeSpawnPoint>();




    public bool playerDataRetrieved
    {
        get { return _playerDataRetrieved; }
        set
        {
            if (value && !playerDataRetrieved)
            {
                try
                {
                    MenuManager.Instance.OpenMenu("online title");
                }
                catch { }
            }
            _playerDataRetrieved = true;
        }
    }
    bool _playerDataRetrieved;
    void Awake()
    {
        colorDict.Add("white", "#FFFFFF");
        colorDict.Add("grey", "#B3B3B3");
        colorDict.Add("black", "#333333");

        colorDict.Add("red", "#FF3939");
        colorDict.Add("lightred", "#FF7C7C");
        colorDict.Add("darkred", "#FF0000");

        colorDict.Add("blue", "#00B0FF");
        colorDict.Add("lightblue", "#77D5FF");
        colorDict.Add("darkblue", "#0080ff");

        colorDict.Add("lightyellow", "#FFFF00");
        colorDict.Add("yellow", "#ABA80E");

        colorDict.Add("green", "#08c50f");

        colorDict.Add("purple", "#A6327E");

        colorDict.Add("orange", "#ff8000");

        colorDict.Add("brown", "#964B00");

        connection = Connection.Offline;
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
        try { instance.pid_player_Dict.Clear(); } catch { }
        try { instance.localPlayers.Clear(); } catch { }

        try { FindObjectOfType<GameTime>().totalTime = 0; }
        catch (Exception e) { Debug.LogWarning(e.Message); }

        Debug.Log("GameManager OnSceneLoaded called");

        instance = this;
        sceneIndex = scene.buildIndex;

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("GameManager OnSceneLoaded: in a room");
        }
        else
        {
            Debug.Log("GameManager OnSceneLoaded: NOT in a room");
        }

        if (scene.buildIndex == 0)
        {
            try { gameMode = GameMode.Multiplayer; } catch { }
            try { gameType = GameType.Fiesta; } catch { }
            try { teamMode = TeamMode.None; } catch { }
            try { onlineTeam = PlayerMultiplayerMatchStats.Team.None; } catch { }
            _lootableWeapons.Clear();
            _networkGrenadeSpawnPoints.Clear();


            WeaponPool.instance = null;
        }
        else if (scene.buildIndex > 0) // We're in the game scene
        {
            if (PhotonNetwork.InRoom)
                try
                {
                    GameManager.instance.gameMode = (GameMode)Enum.Parse(typeof(GameMode), PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString());
                    GameManager.instance.gameType = (GameType)Enum.Parse(typeof(GameType), PhotonNetwork.CurrentRoom.CustomProperties["gametype"].ToString());
                }
                catch (Exception e) { Debug.LogWarning(e.Message); }

            //StartCoroutine(SpawnPlayers_Coroutine());

            try
            {
                Debug.Log($"Is there a Player Manager: {PlayerManager.playerManagerInstance}");
                //if (!PlayerManager.playerManagerInstance)
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
                if (!GameObjectPool.instance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ObjectPool"), Vector3.zero, Quaternion.identity);
                //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineWeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
                //if (!OnlineGameTime.onlineGameTimeInstance)
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkGameTime"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
            }
            catch (Exception e) { Debug.LogWarning(e.Message); }

            Instantiate(_weaponPoolPrefab);
            Instantiate(_ragdollPoolPrefab);

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
        nbLocalPlayersPreset = 1;
        Debug.Log("GameManager Start called");
        //Debug.Log(colorPaletteTextures.Count);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 100;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
                  Debug.unityLogger.logEnabled = false;
#endif


        //SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log(PhotonNetwork.IsConnected);
        Debug.Log(PhotonNetwork.IsConnectedAndReady);

        Launcher.instance.ConnectToPhotonMasterServer();
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
            if (instance.localPlayers.Count == 1 && instance.pid_player_Dict.Count == 1)
                Debug.Log("Alpha0");
        }

        //if (Input.GetKeyDown(KeyCode.Alpha6))
        //    SendReport();

        //if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    try
        //    {
        //        GetRootPlayer().playerMedals.SpawnHeadshotMedal();
        //    }
        //    catch { }
        //    //Transform sp = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint();
        //    //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", "ShooterAI"), sp.position + new Vector3(0, 2, 0), sp.rotation);
        //}
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
        //if (PhotonNetwork.IsMasterClient)
        //    UpdateRoomSettings();

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

        FindObjectOfType<NetworkMainMenu>().GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    List<Vector3> _orSpPts = new List<Vector3>();
    public IEnumerator SpawnPlayers_Coroutine()
    {
        float o = 2; if (PhotonNetwork.IsMasterClient) o = 0.5f;
        Debug.Log("SpawnPlayers_Coroutine");
        yield return new WaitForSeconds(o);

        try
        {
            for (int i = 0; i < nbLocalPlayersPreset; i++)
            {
                Transform spawnpoint = null;
                do
                {
                    spawnpoint = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint();
                } while (_orSpPts.Contains(spawnpoint.position));


                Player player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Network Player"), spawnpoint.position + new Vector3(0, 2 + ((WebManager.webManagerInstance.pda.id) * 0.0001f), 0 + (i * 0.0001f)), spawnpoint.rotation).GetComponent<Player>();
                player.GetComponent<PlayerController>().rid = i;

                //player.originalSpawnPosition = spawnpoint.position;
                //GameManager.instance.orSpPos_Biped_Dict.Add(spawnpoint.position, player); GameManager.instance.orSpPos_Biped_Dict = GameManager.instance.orSpPos_Biped_Dict;
            }
        }
        catch (Exception e) { Debug.LogWarning(e.Message); }
    }










    public static Player GetLocalPlayer(int controllerId)
    {
        return instance.localPlayers[controllerId];
    }

    public static Player GetRootPlayer()
    {
        return instance.localPlayers[0];
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


    public static string report;

    static void SendReport()
    {
        if (report != "")
            SendErrorEmailReport(report);
        report = "";
    }
    public static void SendErrorEmailReport(string m)
    {
        MailMessage newMail = new MailMessage();
        // use the Gmail SMTP Host
        SmtpClient client = new SmtpClient("smtp.office365.com");

        // Follow the RFS 5321 Email Standard
        newMail.From = new MailAddress("nelson@peralta.tech", "Nelson");

        newMail.To.Add("development@spacewackos.com");// declare the email subject
        newMail.To.Add("nelson@peralta.tech");// declare the email subject

        newMail.Subject = "Space Wackos Error Report"; // use HTML for the email body

        newMail.IsBodyHtml = true; newMail.Body = $"<h1> Space Wackos </h1><br><h2>Error</h2><br>=====<br><p>${m}</p>";

        // enable SSL for encryption across channels
        client.EnableSsl = true;
        // Port 465 for SSL communication
        client.Port = 587;
        // Provide authentication information with Gmail SMTP server to authenticate your sender account
        client.Credentials = new System.Net.NetworkCredential("nelson@peralta.tech", "br0wn!c375");

        client.Send(newMail); // Send the constructed mail
        Debug.Log("SenErrorEmailReport Sent");
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
        nbLocalPlayersPreset = int.Parse(n);
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




    public override void OnDisconnected(DisconnectCause cause)
    {
        GameManager.instance.connection = GameManager.Connection.Offline;
    }

    public override void OnConnectedToMaster()
    {
        GameManager.instance.connection = GameManager.Connection.Online;

    }


    public enum MaterialBlendMode
    {
        Opaque,
        Cutout,
        Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply

    }

    public static void SetupMaterialWithBlendMode(Material material, MaterialBlendMode blendMode, bool overrideRenderQueue)
    {
        int minRenderQueue = -1;
        int maxRenderQueue = 5000;
        int defaultRenderQueue = -1;
        switch (blendMode)
        {
            case MaterialBlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                minRenderQueue = -1;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest - 1;
                defaultRenderQueue = -1;
                break;
            case MaterialBlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast;
                defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
            case MaterialBlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
            case MaterialBlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }

        if (overrideRenderQueue || material.renderQueue < minRenderQueue || material.renderQueue > maxRenderQueue)
        {
            if (!overrideRenderQueue)
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", minRenderQueue, maxRenderQueue);
            material.renderQueue = defaultRenderQueue;
        }
    }

    // https://discussions.unity.com/t/check-if-layer-is-in-layermask/16007
    public static bool LayerIsPartOfLayerMask(int lay, LayerMask lm)
    {
        return lm == (lm | (1 << lay));
    }
}