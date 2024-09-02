using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
using System.Net.Mail;
using TMPro;
using Rewired;
using System.Linq;
using Steamworks;

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
    public static int DEFAULT_EXPLOSION_POWER = 300;
    public static int DEFAULT_FRAMERATE = 65;

    // Events
    public delegate void GameManagerEvent();
    public GameManagerEvent OnGameManagerFinishedLoadingScene_Late, OnCameraSensitivityChanged;
    // Enums
    public enum Team { None, Red, Blue, Alien }
    public enum Connection { Unassigned, Local, Online }
    public enum GameMode { Versus, Coop, Unassigned }
    public enum GameType
    {
        Fiesta, Rockets, Slayer, Pro, Snipers, Unassgined,
        Shotguns, Swat, Retro, GunGame, Hill, Oddball, PurpleRain,

        // Swarm Game Types
        Survival, Endless
    }
    public enum ArenaGameType { Fiesta, Slayer, Pro, Snipers, Shotguns }
    public enum TeamMode { Classic, None }

    public enum PreviousScenePayload { None, OpenCarnageReportAndCredits, ResetPlayerDataCells, LoadTimeOutOpenErrorMenu }

    public List<int> arenaLevelIndexes = new List<int>();

    // Intances
    public static GameManager instance;
    public static int GameStartDelay = 6;
    public static Dictionary<string, string> colorDict = new Dictionary<string, string>();
    public CarnageReport carnageReport { get { return _carnageReport; } set { _carnageReport = value; } }

    public Dictionary<int, GameManager.Team> controllerId_TeamDict;
    //public Dictionary<int, Player> pid_player_Dict
    //{
    //    private get { return _pid_player_Dict; }

    //    //set
    //    //{
    //    //    int previousCount = _pid_player_Dict.Count;
    //    //    _pid_player_Dict = value;

    //    //    if (pid_player_Dict.Count != previousCount)
    //    //    {
    //    //        Debug.Log(pid_player_Dict.Count);
    //    //        Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
    //    //        Debug.Log(instance.localPlayers.Count);

    //    //        //if (pid_player_Dict.Count == (PhotonNetwork.CurrentRoom.PlayerCount + GameManager.instance.localPlayers.Count - 1))
    //    //        //    GetComponent<CurrentRoomManager>().allPlayersJoined = true;
    //    //    }
    //    //}
    //}


    public Dictionary<Vector3, Biped> instantiation_position_Biped_Dict
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

    List<Player> _allPlayers = new List<Player>();

    [SerializeField] Connection _connection;
    [SerializeField] Photon.Realtime.ClientState _photonNetworkClientState;
    [SerializeField] GameMode _gameMode;
    [SerializeField] GameType _gameType;
    [SerializeField] TeamMode _teamMode;
    [SerializeField] GameManager.Team _onlineTeam;
    [SerializeField] Player _rootPlayer;
    [SerializeField] bool _inARoom;
    // Public variables

    public Connection connection
    {
        get { return _connection; }
        set
        {
            _connection = value;

            if (value == Connection.Online)
            {
                //if (SteamAPI.IsSteamRunning())
                //    Launcher.instance.LoginWithSteamName();
            }
            else if (value == Connection.Local)
            {
                Launcher.CreateLocalModePlayerDataCells();
                MenuManager.Instance.OpenMenu("online title");
                Launcher.instance.nbLocalPlayersHolder.SetActive(true);
            }
        }
    }
    public GameMode gameMode
    {
        get { return _gameMode; }
        set
        {
            _gameMode = value;

            if (_gameMode == GameMode.Coop)
            {
                FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(false);
                FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(PhotonNetwork.IsMasterClient);
                FindObjectOfType<Launcher>().levelToLoadIndex = 11;

                gameType = GameType.Survival;
                difficulty = SwarmManager.Difficulty.Normal;
            }
            else if (_gameMode == GameMode.Versus)
            {
                if (CurrentRoomManager.instance.roomType != CurrentRoomManager.RoomType.QuickMatch)
                    FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(PhotonNetwork.IsMasterClient);
                else
                    FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(false);

                FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(false);
                FindObjectOfType<Launcher>().levelToLoadIndex = 5;


                //teamDict = new Dictionary<string, int>();
                gameType = GameType.Slayer;
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
            print($"Gameype: {value}");
            _gameType = value;

            //if (value == GameType.GunGame || value == GameType.Hill || value == GameType.Oddball)
            //    if (teamMode != TeamMode.None)
            //        teamMode = TeamMode.None;



            Launcher.instance.gametypeSelectedText.text = $"Gametype: {_gameType}";
        }
    }

    public TeamMode teamMode
    {
        get { return _teamMode; }
        set
        {
            TeamMode _prev = _teamMode;

            _teamMode = value;
            print($"teamMode: {value}");

            Launcher.instance.teamModeText.text = $"Team Mode: {teamMode.ToString()}";
            if (value == TeamMode.None)
            {
                foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells) s.team = Team.None;


                FindObjectOfType<Launcher>().teamRoomUI.SetActive(false);
                _teamDict = new Dictionary<string, int>();
            }
            else
            {
                if (GameManager.instance.connection == Connection.Online)
                {
                    if (gameMode == GameMode.Versus)
                        FindObjectOfType<Launcher>().teamRoomUI.SetActive(true);

                    if (_prev != value)
                    {
                        CreateTeamsBecausePlayerJoined();
                    }
                }
                else
                {
                    CurrentRoomManager.GetLocalPlayerData(0).team = Team.Red;
                    CurrentRoomManager.GetLocalPlayerData(1).team = Team.Red;
                    CurrentRoomManager.GetLocalPlayerData(2).team = Team.Blue;
                    CurrentRoomManager.GetLocalPlayerData(3).team = Team.Blue;
                }
            }

            UpdateNamePlateColorsAndSort();
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
    public int nbLocalPlayersPreset
    {
        get { return _nbLocalPlayersPreset; }
        set
        {
            _nbLocalPlayersPreset = Mathf.Clamp(value, 1, 4);
        }
    }


    public bool devMode;
    [SerializeField] int _sceneIndex = 0;
    public static int sceneIndex
    {
        get { return instance._sceneIndex; }
        set { instance._sceneIndex = value; }
    }
    public string rootPlayerNickname
    {
        get { return WebManager.webManagerInstance.pda.username; }
    }


    public Material armorMaterial { get { return _armorMaterial; } }
    public List<Texture> colorPaletteTextures { get { return _colorPaletteTextures; } }
    public List<LootableWeapon> lootableWeapons { get { return _lootableWeapons; } }
    public List<NetworkGrenadeSpawnPoint> networkGrenadeSpawnPoints { get { return _networkGrenadeSpawnPoints; } }
    public List<Hazard> hazards = new List<Hazard>();
    public List<ScriptObjMapInfo> mapDataCells { get { return _mapDataCells; } }


    public OddballSkull oddballSkull;

    public TMP_Text debugText { get { return _deb; } }

    // called zero

    // private Variables
    [SerializeField] WeaponPool _weaponPoolPrefab;
    [SerializeField] RagdollPool _ragdollPoolPrefab;
    [SerializeField] GrenadePool _grenadePoolPrefab;
    [SerializeField] ActorAddonsPool _actorAddonsPoolPrefab;
    [SerializeField] Dictionary<Vector3, Biped> _orSpPos_Biped_Dict = new Dictionary<Vector3, Biped>();
    [SerializeField] Dictionary<string, int> _teamDict = new Dictionary<string, int>();
    [SerializeField] Dictionary<string, PlayerDatabaseAdaptor> _roomPlayerData = new Dictionary<string, PlayerDatabaseAdaptor>();
    [SerializeField] Material _armorMaterial;
    [SerializeField] List<Texture> _colorPaletteTextures = new List<Texture>();
    [SerializeField] List<ScriptObjMapInfo> _mapDataCells = new List<ScriptObjMapInfo>();
    [SerializeField] List<ScriptObjBipedTeam> _teamsData;

    SwarmManager.Difficulty _difficulty;
    [SerializeField] CarnageReport _carnageReport;
    List<LootableWeapon> _lootableWeapons = new List<LootableWeapon>();
    List<NetworkGrenadeSpawnPoint> _networkGrenadeSpawnPoints = new List<NetworkGrenadeSpawnPoint>();

    [SerializeField] AudioSource _clickSound, _cancelSound;
    public List<PreviousScenePayload> previousScenePayloads = new List<PreviousScenePayload>();

    [SerializeField] TMP_Text _deb;


    public Texture2D cursorTexture;

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


    public float commonPlayerVoiceCooldown;




    bool _playerDataRetrieved;
    float _checkCooldown;


    [SerializeField] AudioSource _beepConsecutiveAudioSource;

    public List<GameplayRecorderPoint> gameplayRecorderPoints = new List<GameplayRecorderPoint>();
    public RenderTexture[] minimapRenderTextures;







    public LayerMask bulletLayerMask, markLayerMask, ragdollHumpLayerMask;







    void Awake()
    {
        _checkCooldown = 0.05f;

        Debug.Log("GameManager Awake");


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

        // https://forum.unity.com/threads/on-scene-change-event-for-dontdestroyonload-object.814299/
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        PhotonNetwork.GameVersion = "171";
    }







    // called first
    void OnEnable()
    {
        Debug.Log("GameManager OnEnable");
        base.OnEnable(); // need this for OnRoomPropertiesUpdate to work
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void InitialisePlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("volume"))
        {
            PlayerPrefs.SetFloat("volume", 100);
        }

        if (!PlayerPrefs.HasKey("sens"))
        {
            PlayerPrefs.SetFloat("sens", 3);
        }
    }


    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        _allPlayers = new List<Player>();

        string[] names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            print($"Quality: {names[i]}");
            //QualitySettings.SetQualityLevel(i, true);

            if (instance.connection == Connection.Online)
            {
                if (names[i].Equals("High"))
                    QualitySettings.SetQualityLevel(i, true);
            }
            else
            {
                if (instance.nbLocalPlayersPreset == 1)
                {
                    if (names[i].Equals("High"))
                        QualitySettings.SetQualityLevel(i, true);
                }
                else if (instance.nbLocalPlayersPreset >= 2)
                {
                    if (names[i].Equals("Medium"))
                        QualitySettings.SetQualityLevel(i, true);
                }
            }
        }



        //try { FindObjectOfType<GameTime>().timeRemaining = 0; }
        //catch (Exception e) { Debug.LogWarning(e.Message); }


        instance = this;
        sceneIndex = scene.buildIndex;


        if (scene.buildIndex == 0)
        {
            //Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
            //Cursor.visible = true;
            ClearPhotonIdToPlayerDict();


            CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.None;
            Launcher.instance.menuGamePadCursorScript.GetReady(ReInput.controllers.GetLastActiveControllerType());
            ActorAddonsPool.instance = null;

            instantiation_position_Biped_Dict.Clear();
            gameplayRecorderPoints.Clear();
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);


            SwarmManager.instance.StopAllMusic();
            MenuManager.Instance.GetMenu("carnage report").GetComponent<CarnageReportMenu>().ClearCarnageReportData();


            if (RagdollPool.instance) { RagdollPool.instance.ragdollPoolList.Clear(); RagdollPool.instance = null; }


            print("GAME MANAGER ONSCENELOADED 0");

            if (previousScenePayloads.Contains(PreviousScenePayload.ResetPlayerDataCells))
            {
                CurrentRoomManager.instance.CreateCarnageReportData();
                CurrentRoomManager.instance.ResetAllPlayerDataExceptMine();
                previousScenePayloads.Remove(PreviousScenePayload.ResetPlayerDataCells);
            }

            if (previousScenePayloads.Contains(PreviousScenePayload.OpenCarnageReportAndCredits))
            {
                previousScenePayloads.Remove(PreviousScenePayload.OpenCarnageReportAndCredits);
                //MenuManager.Instance.OpenMenu("carnage report");
                MenuManager.Instance.OpenPopUpMenu("credits");
            }

            if (previousScenePayloads.Contains(PreviousScenePayload.LoadTimeOutOpenErrorMenu))
            {
                previousScenePayloads.Remove(PreviousScenePayload.LoadTimeOutOpenErrorMenu);
                MenuManager.Instance.OpenMainMenu();
                MenuManager.Instance.OpenErrorMenu("A player could not load level");
            }



            try { gameMode = GameMode.Versus; } catch { }
            try { gameType = GameType.Fiesta; } catch { }
            try { teamMode = TeamMode.None; } catch { }
            _lootableWeapons.Clear();
            hazards.Clear();
            _networkGrenadeSpawnPoints.Clear();


            WeaponPool.instance = null;
            MultiplayerManager.instance.lootableWeaponsDict.Clear();
        }
        else if (scene.buildIndex > 0) // We're in the game scene
        {
            if (gameMode == GameMode.Coop) Instantiate(_actorAddonsPoolPrefab);


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
            Instantiate(_grenadePoolPrefab);

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
            //FindObjectOfType<GameTime>().timeRemaining = 0;
        }
        OnGameManagerFinishedLoadingScene_Late?.Invoke();
    }

    // called third
    private void Start()
    {
        Debug.Log("GameManager Start");
        nbLocalPlayersPreset = 1;
        InitialisePlayerPrefs();
        LoadPlayerPrefs();
        //Debug.Log(colorPaletteTextures.Count);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = DEFAULT_FRAMERATE;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
                  Debug.unityLogger.logEnabled = false;
#endif
        CurrentRoomManager.InitializeAllPlayerDataCells();


        if (!GameManager.instance.devMode)
            StartCoroutine(PingGoogle("8.8.8.8"));
        else
        {
            GameManager.ROOT_PLAYER_NAME = $"{UnityEngine.Random.Range(1, 100)}";
            PhotonNetwork.NickName = ROOT_PLAYER_NAME;
            GameManager.instance.connection = GameManager.Connection.Online;
            Launcher.instance.ConnectToPhotonMasterServer();
            MenuManager.Instance.OpenMainMenu();
        }


        //SteamManager.Instance.Init();

        //GameManager.instance.debugText.text += "--abc---";

        //if (connection == Connection.Online)
        //{
        //    GameManager.instance.debugText.text += " ConnectToPhotonMasterServer";

        //    Launcher.instance.ConnectToPhotonMasterServer();
        //}


        //if (Application.internetReachability == NetworkReachability.NotReachable)
        //{
        //    Debug.LogError("YOU DONT HAVE INTERNET");
        //    GameManager.instance.connection = GameManager.Connection.Local;
        //    PhotonNetwork.OfflineMode = true;
        //}
        //else
        //{
        //    Debug.LogError("YOU DONT HAVE INTERNET");
        //    SteamManager.Instance.Init();
        //    Launcher.instance.ConnectToPhotonMasterServer();
        //}
    }

    IEnumerator PingGoogle(string ip)
    {
        Ping ping = new Ping(ip);
        yield return new WaitForSeconds(1f);
        if (ping.isDone)
        {
            print($"Pinged Google in: {ping.time}");

            SteamManager.Instance.Init();
            Launcher.instance.ConnectToPhotonMasterServer();
        }
        else
        {
            Debug.LogError("YOU DONT HAVE INTERNET");

            GameManager.instance.connection = GameManager.Connection.Local;
            PhotonNetwork.OfflineMode = true;
        }
    }

    // called when the game is terminated
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {

        if (commonPlayerVoiceCooldown > 0) commonPlayerVoiceCooldown -= Time.deltaTime;

        if (SceneManager.GetActiveScene().buildIndex == 0 && CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private
            && CurrentRoomManager.instance.matchSettingsSet)
        {
            if (CurrentRoomManager.instance.roomGameStartCountdown > 0)
            {
                CurrentRoomManager.instance.roomGameStartCountdown -= Time.deltaTime;

                Launcher.instance.gameCountdownText.text = ($"GAME STARTS IN: {(int)CurrentRoomManager.instance.roomGameStartCountdown}");


                if (CurrentRoomManager.instance.roomGameStartCountdown <= 3)
                    GameManager.PlayerBeeps();

                if (CurrentRoomManager.instance.roomGameStartCountdown <= 0)
                {
                    print("LOADING LEVEL");
                    if (PhotonNetwork.IsMasterClient)
                        PhotonNetwork.LoadLevel(Launcher.instance.levelToLoadIndex);
                }
            }
        }



        if (_checkCooldown > 0)
        {
            _checkCooldown -= Time.deltaTime;

            if (_checkCooldown <= 0)
            {
                _photonNetworkClientState = PhotonNetwork.NetworkClientState;
                _inARoom = PhotonNetwork.CurrentRoom != null;

                _checkCooldown = 0.05f;
            }
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
        gameMode = GameMode.Coop;
        gameType = GameType.Survival;
    }

    void OnCreateMultiplayerRoomButton_Delegate(Launcher launcher)
    {
        gameMode = GameMode.Versus;
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


        for (int i = 0; i < CurrentRoomManager.instance.playerDataCells.Count; i++)
        {
            if (CurrentRoomManager.instance.playerDataCells[i].local)
            {
                Debug.Log($"SpawnPlayers_Coroutine {i} {CurrentRoomManager.instance.playerDataCells[i].photonRoomIndex} {CurrentRoomManager.instance.playerDataCells[i].rewiredId}");

                (Transform, bool) spawnpoint = (null, false);

                if (GameManager.instance.connection == Connection.Local)
                    spawnpoint = (SpawnManager.spawnManagerInstance.GetSpawnPointAtIndex(i), false);
                else
                {
                    spawnpoint = (SpawnManager.spawnManagerInstance.GetSpawnPointAtIndex(CurrentRoomManager.instance.playerDataCells[0].photonRoomIndex - 1), false);

                }

                // Do not use spawn position for authority. It is not accurate due to it being a float type
                Vector3 sp = spawnpoint.Item1.position;
                //print($"Spawning player at: {sp} + {new Vector3((float)CurrentRoomManager.instance.playerDataCells[i].photonRoomIndex / 10, 2, (float)CurrentRoomManager.instance.playerDataCells[i].photonRoomIndex / 10)}");
                sp = sp + new Vector3((float)CurrentRoomManager.instance.playerDataCells[i].photonRoomIndex / 10, 2, (float)CurrentRoomManager.instance.playerDataCells[i].rewiredId / 10);
                //print($"Spawning player at: {sp}");
                Player player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Network Player"), sp, spawnpoint.Item1.rotation).GetComponent<Player>();

                player.UpdateRewiredId(CurrentRoomManager.instance.playerDataCells[i].rewiredId);

                player.ChangePlayerIdLocalMode(i);


                Debug.Log($"SpawnPlayers_Coroutine {i} {player}");
                if (i == 0)
                {
                    instance._rootPlayer = player;
                    _rootPlayer = player;
                }
                //player.originalSpawnPosition = spawnpoint.position;
                //GameManager.instance.orSpPos_Biped_Dict.Add(spawnpoint.position, player); GameManager.instance.orSpPos_Biped_Dict = GameManager.instance.orSpPos_Biped_Dict;
            }
        }
    }










    public static Player GetLocalPlayer(int controllerId)
    {
        foreach (Player p in instance._allPlayers)
            if (p.controllerId == controllerId)
                return p;
        return null;
    }

    public static List<Player> GetLocalPlayers()
    {
        return instance._allPlayers.Where(item => item.isMine).ToList();
    }

    public static Player GetRootPlayer()
    {
        return instance._allPlayers.Where(item => item.rid == 0 && item.isMine).FirstOrDefault();
    }

    public static Player GetPlayerWithPhotonView(int pid)
    {
        return instance._allPlayers.Where(item => item.photonId == pid).FirstOrDefault();
    }



    public static Player GetPlayerWithIdAndRewId(int playerPhotonView)
    {
        return instance._allPlayers.Where(item => item.photonId == playerPhotonView).FirstOrDefault();
    }




    //public static bool PlayerDictContainsPhotonId(int k)
    //{
    //    foreach (Player p in instance._allPlayers)
    //        if (p.controllerId == k) return true;

    //    return false;
    //    //return instance._pid_player_Dict.ContainsKey(k);
    //}

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
    public void LeaveCurrentRoomAndLoadLevelZero()
    {
        if (SceneManager.GetActiveScene().buildIndex > 0)
            PhotonNetwork.LeaveRoom(); // Will trigger OnLeftRoom
    }


    public override void OnLeftRoom() // Is also called when quitting a game while connected to the internet. Does not trigger when offline
    {
        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            Debug.Log("LeaveCurrentRoomAndLoadLevelZero: OnLeftRoom");
            PhotonNetwork.LoadLevel(0);
        }
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
        int _time = FindObjectOfType<GameTime>().timeRemaining;

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
        print("OnDisconnected");
        //GameManager.instance.connection = GameManager.Connection.Local;
    }

    public override void OnConnectedToMaster()
    {
        //print("OnConnectedToMaster");
        //GameManager.instance.connection = GameManager.Connection.Online;
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

    public static void PlayClickSound()
    {
        GameManager.instance._clickSound.Play();
    }

    public static void PlayCancelSound()
    {
        GameManager.instance._cancelSound.Play();
    }











    public static void UpdateVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("volume") / 100f;
    }

    public static void LoadPlayerPrefs()
    {
        UpdateVolume();

        CurrentRoomManager.instance.playerDataCells[0].sens = PlayerPrefs.GetFloat("sens");
        //if (GameManager.instance.connection == Connection.Local)
        {
            CurrentRoomManager.instance.playerDataCells[1].sens = PlayerPrefs.GetFloat("sens");
            CurrentRoomManager.instance.playerDataCells[2].sens = PlayerPrefs.GetFloat("sens");
            CurrentRoomManager.instance.playerDataCells[3].sens = PlayerPrefs.GetFloat("sens");
        }
    }

    public static void SaveOptions(float vol = -1, float sens = -1)
    {
        if (vol != -1)
        {
            PlayerPrefs.SetFloat("volume", vol);
            UpdateVolume();
        }
        if (sens != -1)
        {
            PlayerPrefs.SetFloat("sens", sens);


            if (SceneManager.GetActiveScene().buildIndex > 0)
            {
                print("FIX THIS");
                //foreach (Player p in instance.localPlayers.Values)
                //{
                //    p.playerCamera.frontEndMouseSens = sens;
                //}
            }
        }
    }

    public static void PlayerBeeps()
    {
        if (!instance._beepConsecutiveAudioSource.isPlaying)
            instance._beepConsecutiveAudioSource.Play();
    }

    public static void StopBeeps()
    {
        instance._beepConsecutiveAudioSource.Stop();
    }


    public void ReEvaluatePhotonToPlayerDict()
    {
        print("ReEvaluatePhotonToPlayerDict");

        GameManager.instance.ClearPhotonIdToPlayerDict();
        foreach (Player p in FindObjectsOfType<Player>())
            GameManager.instance.AddToPhotonToPlayerDict(p.photonId, p);

    }


    public void AddToPhotonToPlayerDict(int photonId, Player p)
    {
        _allPlayers.Add(p);
        //_pid_player_Dict.Add(photonId, p);
    }

    public List<Player> GetAllPhotonPlayers()
    {
        return _allPlayers;
    }

    public void ClearPhotonIdToPlayerDict()
    {
        _allPlayers.Clear();
        //_pid_player_Dict.Clear();
    }

    public void CreateTeamsBecausePlayerJoined()
    {
        Dictionary<string, int> _teamDict = new Dictionary<string, int>();

        foreach (KeyValuePair<int, Photon.Realtime.Player> kvp in PhotonNetwork.CurrentRoom.Players)
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                Team t = Team.Red;

                if ((kvp.Key % 2 == 0) && gameMode != GameMode.Coop)
                    t = Team.Blue;

                CurrentRoomManager.GetPlayerDataWithId(int.Parse(kvp.Value.NickName), 0).team = t;

                _teamDict.Add(kvp.Value.NickName, (int)t);

                Debug.Log($"Player {kvp.Value.NickName} is part of {t} team");
            }

        UpdateNamePlateColorsAndSort();
    }


    void UpdateNamePlateColorsAndSort()
    {
        foreach (Transform child in Launcher.instance.namePlatesParent)
        {
            child.GetComponent<PlayerNamePlate>().UpdateColorPalette();
        }

        foreach (Transform child in Launcher.instance.namePlatesParent)
        {
            if (child.GetComponent<PlayerNamePlate>().playerDataCell.team == Team.Blue)
                child.SetAsLastSibling();
        }
    }
}