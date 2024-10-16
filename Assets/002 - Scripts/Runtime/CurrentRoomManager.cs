using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

public class CurrentRoomManager : MonoBehaviour
{
    public static CurrentRoomManager instance { get { return _instance; } }


    // Events
    public delegate void GameManagerEvent(CurrentRoomManager gme);
    public GameManagerEvent OnGameIsReady, OnGameStarted, OnGameStartedLate;

    public enum RoomType
    {
        QuickMatch, Private, None
    }

    public RoomType roomType
    {
        get { return _roomType; }
        set
        {
            Debug.Log($"ROOM TYPE {value}");
            _roomType = value;
        }
    }


    /// <summary>
    /// Lobby
    /// </summary>
    public Dictionary<string, int> playerNicknameNbLocalPlayersDict
    {
        get { return _playerNicknameNbLocalPlayersDict; }
        set
        {
            _playerNicknameNbLocalPlayersDict = value;
            int c = 0;

            foreach (KeyValuePair<string, int> items in playerNicknameNbLocalPlayersDict)
            {
                print("You have " + items.Value + " " + items.Key);
                c += items.Value;
            }

            expectedNbPlayers = c;
        }
    }
    public int expectedNbPlayers
    {
        get { return _expectedNbPlayers; }
        set
        {
            int _preVal = _expectedNbPlayers;
            _expectedNbPlayers = value;


            if (PhotonNetwork.IsMasterClient && CurrentRoomManager.instance.roomType == RoomType.QuickMatch)
                if (_preVal == 0 && expectedNbPlayers == 1)
                {
                    //if (vetoCountdown > 0) // Make sure this variable if greater than 0 by default
                    vetoCountdown = 10;
                    roomGameStartCountdown = 10;
                    ChooseRandomMatchSettingsForQuickMatch();
                }
                else if (expectedNbPlayers > 1)
                {
                    if (_preVal < expectedNbPlayers)
                    {
                        vetoCountdown = 9;
                        roomGameStartCountdown = 9;
                    }


                    if (!_randomPvPSettingsChosen)
                        if (CurrentRoomManager.instance.roomType == RoomType.QuickMatch)
                            ChooseRandomMatchSettingsForQuickMatch();
                }
        }
    }


    /// <summary>
    /// Step 1: Caluclate expected Map Add-Ons
    /// </summary>
    public int expectedMapAddOns
    {
        get { return _expectedMapAddOns; }
        private set
        {
            _expectedMapAddOns = value;
        }
    }

    /// <summary>
    /// Step 2: Wait for spawned items to tell this when they are spawned. When all add-ons are spawned, tell GameManager to spawn local players
    /// </summary>
    public int spawnedMapAddOns
    {
        get { return _spawnedMapAddOns; }
        set
        {
            int _preVal = _spawnedMapAddOns;
            _spawnedMapAddOns = value;

            if (_preVal < _spawnedMapAddOns)
            {
                print($"_spawnedMapAddOns increase to {_spawnedMapAddOns}");
                print($"IceChunk: {FindObjectsOfType<IceChunk>().Length}");
                print($"ExplosiveBarrelSpawnPoint: {FindObjectsOfType<ExplosiveBarrelSpawnPoint>().Length}");
                print($"NetworkWeaponSpawnPoint: {FindObjectsOfType<NetworkWeaponSpawnPoint>().Length}");
                print($"NetworkGrenadeSpawnPoint: {FindObjectsOfType<NetworkGrenadeSpawnPoint>().Length}");
            }

            if (_spawnedMapAddOns == expectedMapAddOns)
            {
                Debug.Log($"spawnedMapAddOns: {value} ({expectedMapAddOns} needed)");
                mapIsReady = true;
            }
        }
    }

    public bool mapIsReady
    {
        get { return _mapIsReady; }
        private set
        {
            bool _preVal = _mapIsReady;
            _mapIsReady = value;

            if (value && _preVal != value)
            {
                Debug.Log($"mapIsReady");
                //StartCoroutine(GameManager.instance.SpawnPlayers_Coroutine());

            }
        }
    }

    public int playersLoadedScene
    {
        get { return _playersLoadedScene; }
        set
        {
            _playersLoadedScene = value;

            if (_playersLoadedScene == expectedNbPlayers)
                StartCoroutine(GameManager.instance.SpawnPlayersCheck_Coroutine());
        }
    }

    /// <summary>
    /// Step 3: Wait for players to tell this when they have spawned
    /// </summary>
    public int nbPlayersJoined
    {
        get { return _nbPlayersJoined; }
        set
        {
            _nbPlayersJoined = value;

            if (nbPlayersJoined == expectedNbPlayers)
                StartCoroutine(TriggerAllPlayersJoined_Coroutine());
        }
    }

    public int nbPlayersSet
    {
        get { return _nbPlayersSet; }
        set
        {
            _nbPlayersSet = value;
            print($"_nbPlayersSet {_nbPlayersSet}");

            if (_nbPlayersSet == expectedNbPlayers && nbPlayersJoined == expectedNbPlayers)
                allPlayersJoined = true;
        }
    }






    public bool allPlayersJoined
    {
        get { return _allPlayersJoined; }
        set
        {
            bool _preVal = _allPlayersJoined;
            _allPlayersJoined = value;
            if (value && _preVal != value)
            {
                Debug.Log("CurrentRoomManager: OnAllPlayersJoinedRoom");
                gameIsReady = true;
                //StartCoroutine(GameIsReadyDelay_Coroutine());

                //OnAllPlayersJoinedRoom?.Invoke(this);
            }
        }
    }

    public bool gameIsReady
    {
        get { return _gameIsReady; }
        private set
        {
            bool _preVal = _gameIsReady;
            _gameIsReady = value;

            if (value && _preVal != value)
            {
                print($"CURRENT ROOM MANAGER: GAME IS READY {GameManager.instance.GetAllPhotonPlayers().Count}");
                _gameIsReady = true;

                OnGameIsReady?.Invoke(this);


                if (GameManager.instance.GetAllPhotonPlayers().Count != PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    GameManager.instance.ReEvaluatePhotonToPlayerDict();

                }


                foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
                    if (p && p.isMine)
                    {
                        p.allPlayerScripts.scoreboardManager.SetScoreboardRows();
                        p.SetupMotionTracker();
                    }

                StartCoroutine(GameStartDelayMapCamera_Coroutine());
                TriggerGameStartCountdown();
            }
        }
    }

    //public bool gameStart
    //{
    //    get { return _gameStart; }
    //    private set
    //    {
    //        bool _preVal = _gameStart;
    //        _gameStart = value;

    //        if (value && _preVal != value)
    //        {
    //            Debug.Log("gameStart");
    //            OnGameStartedEarly?.Invoke(this);
    //            _gameStartCountdown = GameManager.GameStartDelay;
    //        }
    //        _gameStart = value;
    //    }
    //}

    //bool reachedHalwayGameStartCountdown
    //{
    //    get { return _reachedHalwayGameStartCountdown; }
    //    set
    //    {
    //        bool _preVal = _reachedHalwayGameStartCountdown;
    //        _reachedHalwayGameStartCountdown = value;

    //        if (value && _preVal != value)
    //        {
    //            Debug.Log("reachedHalwayGameStartCountdown");
    //            //MapCamera.instance.TriggerGameStartBehaviour();

    //        }
    //    }
    //}

    public bool gameStarted
    {
        get { return _gameStarted; }
        private set
        {
            bool _preVal = _gameStarted;
            _gameStarted = value;

            if (value && _preVal != value)
            {
                Debug.Log("gameStarted");
                _gameStarted = true;

                //StartCoroutine(GameStartDelayMapCamera_Coroutine());
                //StartCoroutine(GameStartDelay_Coroutine());


                //MapCamera.instance.gameObject.SetActive(false);
                //foreach (Player p in GameManager.instance.localPlayers.Values)
                //    p.TriggerGameStartBehaviour();

                //if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                //    SwarmManager.instance.Begin();
            }
        }
    }

    public bool gameOver
    {
        get { return _gameOver; }
        set
        {
            bool _preVal = _gameOver;
            _gameOver = value;

            if (value && _preVal != value)
            {
                print("Game Over bool");
                _gameOver = true;
            }
        }
    }

    public int vetos
    {
        get { return _vetos; }
        set
        {
            _vetos = value;

        }
    }

    public float vetoCountdown
    {
        get { return _vetoCountdown; }
        set
        {
            _vetoCountdown = value;
        }
    }

    public float roomGameStartCountdown
    {
        get { return _roomGameStartCountdown; }
        set
        {
            _roomGameStartCountdown = value;
        }
    }

    public bool leftRoomManually { get { return _leftRoomManually; } set { _leftRoomManually = value; } }

    public bool halfOfPlayersInRoomAreRandos
    {
        get
        {
            print($"halfOfPlayersAreRandos : {GameManager.instance.GetAllPhotonPlayers().Where(item => !item.isMine).Count()} / {instance.expectedNbPlayers / 2f} " +
                $"{GameManager.instance.GetAllPhotonPlayers().Where(item => !item.isMine).Count() >= (CurrentRoomManager.instance.expectedNbPlayers / 2f)}");
            if (GameManager.instance.GetAllPhotonPlayers().Where(item => !item.isMine).Count() >= (CurrentRoomManager.instance.expectedNbPlayers / 2f))
                return true;
            else
                return false;
        }
    }

    public bool youHaveInvites
    {
        get
        {
            if (GameManager.instance.GetAllPhotonPlayers().Where(item => item.isMine).Count() > 1)
                return true;
            else
                return false;
        }
    }






    public List<ScriptObjPlayerData> playerDataCells { get { return _playerDataCells; } }
    public List<ScriptObjBipedTeam> teamsData { get { return _bipedTeams; } }
    public bool matchSettingsSet { get { return _matchSettingsSet; } set { _matchSettingsSet = value; } }

    [SerializeField] bool _mapIsReady, _allPlayersJoined, _gameIsReady;
    [SerializeField] bool _matchSettingsSet, _gameStarted, _gameOver;
    [SerializeField] float _gameStartCountdown, _roomGameStartCountdown, _vetoCountdown = 9, _rpcCooldown;

    [SerializeField] int _expectedMapAddOns, _spawnedMapAddOns, _expectedNbPlayers, _playersLoadedScene, _nbPlayersJoined, _nbPlayersSet, _vetos;

    [SerializeField] RoomType _roomType;

    static CurrentRoomManager _instance;

    Dictionary<string, int> _playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

    [SerializeField] bool _reachedHalwayGameStartCountdown, _randomInitiQuickMatchSettingsChosen, _randomPvPSettingsChosen;
    //[SerializeField] Dictionary<string, PlayerDatabaseAdaptor.PlayerExtendedPublicData> _extendedPlayerData = new Dictionary<string, PlayerDatabaseAdaptor.PlayerExtendedPublicData>();

    [SerializeField] GameManager.GameType _vetoedGameType;
    [SerializeField] int _ran, _vetoedMapIndex;

    [SerializeField] List<ScriptObjPlayerData> _playerDataCells = new List<ScriptObjPlayerData>();
    [SerializeField] List<ScriptObjBipedTeam> _bipedTeams;


    int ran, _minH;
    bool _achievementUnlocked = false;
    string _tempAchievementName = "";

    bool _leftRoomManually, _playerDataRetrieved;


    void Awake()
    {
        _rpcCooldown = 0.3f;
        vetoCountdown = roomGameStartCountdown = 9;
        _roomType = RoomType.None;

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        vetoCountdown = 9;
    }

    public static void InitializeAllPlayerDataCells()
    {
        foreach (ScriptObjPlayerData sod in instance._playerDataCells)
            sod.InitialReset();
    }

    private void Update()
    {

        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            if (gameIsReady && _gameStartCountdown > 0)
            {
                _gameStartCountdown -= Time.deltaTime;

                if (_gameStartCountdown <= 3)
                {
                    GameManager.PlayerBeeps();
                    //MapCamera.PlayerBeeps();
                }

                if (_gameStartCountdown <= 0)
                {
                    MapCamera.instance.gameObject.SetActive(false);
                    foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
                        if (p.isMine)
                            p.TriggerGameStartBehaviour();

                    if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                        SwarmManager.instance.Begin();

                    foreach (Player p in GameManager.GetLocalPlayers()) p.playerInventory.TriggerStartGameBehaviour();

                    gameStarted = true;
                }
            }
        }


        if (SceneManager.GetActiveScene().buildIndex > 0) return;
        if (roomType == RoomType.Private) return;

        if (_rpcCooldown > 0) _rpcCooldown -= Time.deltaTime;


        if (PhotonNetwork.InRoom)
        {
            if ((expectedNbPlayers /*- GameManager.instance.nbLocalPlayersPreset*/) > 0) // At least one more stranger player is in the room
                if (_randomInitiQuickMatchSettingsChosen && vetoCountdown > 0)
                {

                    if (PhotonNetwork.IsMasterClient)
                    {
                        vetoCountdown -= Time.deltaTime;

                        if (_rpcCooldown <= 0)
                        {
                            // TODO

                            NetworkGameManager.instance.UpdateRoomCountdowns((int)vetoCountdown, (int)roomGameStartCountdown);
                        }
                    }

                    Launcher.instance.gameCountdownText.text = $"VETO COUNTDOWN: {((int)vetoCountdown)} seconds\nVetos: {instance.vetos} out of {instance.expectedNbPlayers}";

                    if (vetoCountdown <= 0)
                    {
                        Launcher.instance.vetoBtn.SetActive(false);

                        if (_vetos > _expectedNbPlayers * 0.5f && PhotonNetwork.IsMasterClient)
                        {
                            _vetoedMapIndex = Launcher.instance.levelToLoadIndex;
                            _vetoedGameType = GameManager.instance.gameType;
                            ChooseRandomMatchSettingsForQuickMatch();
                        }
                    }
                }




            if (_randomInitiQuickMatchSettingsChosen && roomGameStartCountdown > 0 && vetoCountdown <= 0)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    roomGameStartCountdown -= Time.deltaTime;

                    if (_rpcCooldown <= 0)
                    {
                        // TODO
                        NetworkGameManager.instance.UpdateRoomCountdowns((int)vetoCountdown, (int)roomGameStartCountdown);
                    }
                }

                Launcher.instance.gameCountdownText.text = $"Game Starts in: {((int)roomGameStartCountdown)}";

                if (CurrentRoomManager.instance.roomType == RoomType.QuickMatch)
                    if (_roomGameStartCountdown <= 0 && PhotonNetwork.IsMasterClient)
                    {
                        Debug.Log("START GAME!!!!");
                        Launcher.instance.StartGame();
                    }
            }
        }

        if (_rpcCooldown <= 0) _rpcCooldown = 0.3f;





    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {

        gameStarted = false;
        _reachedHalwayGameStartCountdown = _leftRoomManually = false;

        _expectedMapAddOns = _vetos = 0;

        if (scene.buildIndex == 0)
        {

            _matchSettingsSet = _mapIsReady = _allPlayersJoined = _gameIsReady = _gameOver = _gameStarted =
                  _reachedHalwayGameStartCountdown = _randomInitiQuickMatchSettingsChosen = false;
            _gameStartCountdown = _expectedMapAddOns = _spawnedMapAddOns = _expectedNbPlayers = _nbPlayersJoined = _nbPlayersSet = _playersLoadedScene = 0;
            playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

            _vetoedGameType = GameManager.GameType.Unassgined; _vetoedMapIndex = 0;
        }
        else
        {
            for (int i = 0; i < GameManager.instance.nbLocalPlayersPreset; i++)
                NetworkGameManager.instance.AddPlayerLoadedScene();


            if (GameManager.instance.gameType != GameManager.GameType.GunGame && GameManager.instance.gameType != GameManager.GameType.Fiesta)
                expectedMapAddOns = FindObjectsOfType<NetworkGrenadeSpawnPoint>().Length + FindObjectsOfType<Hazard>().Length;


            if (GameManager.instance.gameType != GameManager.GameType.GunGame && GameManager.instance.gameType != GameManager.GameType.Fiesta)
                foreach (NetworkWeaponSpawnPoint nwsp in FindObjectsOfType<NetworkWeaponSpawnPoint>())
                    if (nwsp.transform.root.GetComponent<Player>() == null)
                        expectedMapAddOns += 1;
        }
    }



    IEnumerator TriggerAllPlayersJoined_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
            if (p.isMine)
            {
                print($"nbPlayersJoined {p.name}");
                p.TriggerAllPlayersJoinedBehaviour();

            }
    }




    public void ChooseRandomMatchSettingsForQuickMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("ChooseRandomMatchSettingsForQuickMatch");
        _ran = Random.Range(0, 5);
        _ran = 2;

        if (expectedNbPlayers == 1)
        {
            GameManager.instance.gameMode = GameManager.GameMode.Coop;
            GameManager.instance.difficulty = SwarmManager.Difficulty.Heroic;
            GameManager.instance.teamMode = GameManager.TeamMode.Classic;

            ChooseRandomPvEMap();
            if (_vetoedMapIndex != 0)
                while (_vetoedMapIndex == Launcher.instance.levelToLoadIndex)
                {
                    ChooseRandomPvEMap();
                }
        }
        else // PvP
        {
            GameManager.instance.gameMode = GameManager.GameMode.Versus;
            GameManager.instance.teamMode = GameManager.TeamMode.None;


            ChooseRandomPvPGameType();

            if (_vetoedGameType != GameManager.GameType.Unassgined)
                while (_vetoedGameType == GameManager.instance.gameType)
                {
                    ChooseRandomPvPGameType();
                }



            ChooseRandomPvPMap();

            if (_vetoedMapIndex != 0)
                while (_vetoedMapIndex == Launcher.instance.levelToLoadIndex)
                {
                    ChooseRandomPvPGameType();
                }
            _randomPvPSettingsChosen = true;
        }


        _randomInitiQuickMatchSettingsChosen = true;

        Debug.Log("ChooseRandomMatchSettingsForQuickMatch END");
        FindObjectOfType<NetworkGameManager>().SendGameParams();
    }

    void ChooseRandomPvPGameType()
    {
        _ran = Random.Range(0, 100);

        if (_ran <= 50)
            GameManager.instance.gameType = GameManager.GameType.Pro;
        else if (_ran <= 60)
            GameManager.instance.gameType = GameManager.GameType.Swat;
        else if (_ran <= 80)
            GameManager.instance.gameType = GameManager.GameType.Hill;
        else if (_ran <= 100)
            GameManager.instance.gameType = GameManager.GameType.Snipers;
    }

    void ChooseRandomPvPMap()
    {
        _ran = Random.Range(0, 100);

        //if (_ran <= 10)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(5);// Lightouse
        //else if (_ran <= 20)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(6);// Oasis
        //else if (_ran <= 30)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(7);// Canyon
        //else if (_ran <= 40)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(8);// Babylon
        //else if (_ran <= 50)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(9);// Starship
        //else if (_ran <= 60)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(10);// Temple
        //else if (_ran <= 70)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(11);// Blizzard
        //else if (_ran <= 80)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(12);// Factory
        //else if (_ran <= 90)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(17);// Parasite
        //else if (_ran <= 100)
        //    Launcher.instance.ChangeLevelToLoadWithIndex(18);// Shaman



        if (_ran <= 25)
            Launcher.instance.ChangeLevelToLoadWithIndex(2);// Oasis
        else if (_ran <= 50)
            Launcher.instance.ChangeLevelToLoadWithIndex(3);// Factory
        else if (_ran <= 75)
            Launcher.instance.ChangeLevelToLoadWithIndex(9);// Factory
        else if (_ran <= 100)
            Launcher.instance.ChangeLevelToLoadWithIndex(10);// Shaman
    }

    void ChooseRandomPvEMap()
    {
        Debug.Log("ChooseRandomPvEMap");
        _ran = Random.Range(0, 7);

        if (_ran <= 1)
            Launcher.instance.ChangeLevelToLoadWithIndex(11);// Downpoor
        else if (_ran <= 2)
            Launcher.instance.ChangeLevelToLoadWithIndex(12);// Haunted
        else if (_ran <= 3)
            Launcher.instance.ChangeLevelToLoadWithIndex(13);// Haunted
        else if (_ran <= 4)
            Launcher.instance.ChangeLevelToLoadWithIndex(14);// Haunted
        else if (_ran <= 5)
            Launcher.instance.ChangeLevelToLoadWithIndex(15);// Haunted
        else if (_ran <= 6)
            Launcher.instance.ChangeLevelToLoadWithIndex(16);// Haunted
        else if (_ran <= 7)
            Launcher.instance.ChangeLevelToLoadWithIndex(17);// Haunted
    }

    public void ResetRoomCountdowns()
    {
        vetoCountdown = 9;
        _gameStartCountdown = 9;
    }

    public void AddExtendedPlayerData(PlayerDatabaseAdaptor.PlayerExtendedPublicData pepd)
    {
        Debug.Log($"AddExtendedPlayerData {pepd.player_id} {pepd.username}");
        Debug.Log(GameManager.ROOT_PLAYER_NAME);


        if (pepd.username.Equals(GameManager.ROOT_PLAYER_NAME))
        {
            Debug.Log($"UPDATING ROOT PLAYER DATA");
            instance._playerDataCells[0].playerExtendedPublicData = pepd;









            if (instance._playerDataCells[0].playerExtendedPublicData.level >= 5)
            {
                _achievementUnlocked = false; _tempAchievementName = "BABYSTEPS";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);

                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }


            if (instance._playerDataCells[0].playerExtendedPublicData.level >= 25)
            {
                _achievementUnlocked = false; _tempAchievementName = "WHWT";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    AchievementManager.UnlockAchievement(_tempAchievementName);
                }

                if (!instance._playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("burning_helmet"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-burning_helmet-"));
            }



            if (instance._playerDataCells[0].playerExtendedPublicData.level == 50)
            {
                _achievementUnlocked = false; _tempAchievementName = "MAXEDOUT";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    AchievementManager.UnlockAchievement(_tempAchievementName);
                }

                if (!instance._playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("flaming_helmet"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-flaming_helmet-"));
            }



            _minH = PlayerProgressionManager.GetMinHonorForRank("Sergeant");
            if (instance._playerDataCells[0].playerExtendedPublicData.honor >= _minH)
            {
                _achievementUnlocked = false; _tempAchievementName = "VETERAN";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }

            _minH = PlayerProgressionManager.GetMinHonorForRank("Second Lieutenant");
            if (instance._playerDataCells[0].playerExtendedPublicData.honor >= _minH)
            {
                _achievementUnlocked = false; _tempAchievementName = "SIR";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    AchievementManager.UnlockAchievement(_tempAchievementName);
                }

                if (!instance._playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("sword1_ca"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-sword1_ca-"));
            }


            _minH = PlayerProgressionManager.GetMinHonorForRank("General");
            if (instance._playerDataCells[0].playerExtendedPublicData.honor >= _minH)
            {
                _achievementUnlocked = false; _tempAchievementName = "LAIC";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    AchievementManager.UnlockAchievement(_tempAchievementName);
                }

                if (!instance._playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("katana_ca"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-katana_ca-"));


                if (instance._playerDataCells[0].playerExtendedPublicData.level == 50)
                    if (!instance._playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("haunted_hc"))
                        WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-haunted_hc-"));
            }





        }
        else
        {
            //if (instance._playerDataCells.Where(item => item.occupied && item.playerExtendedPublicData.player_id == pepd.player_id).Count() == 1)
            //{
            //    instance._playerDataCells.Where(item => item.occupied && item.playerExtendedPublicData.player_id == pepd.player_id).FirstOrDefault().playerExtendedPublicData = pepd;
            //}
            //else
            //{
                for (int i = 0; i < instance._playerDataCells.Count; i++)
                {
                    //Debug.Log($"Player Extended Public Data {i}");
                    //Debug.Log($"Player Extended Public Data {instance._extendedPlayerData[i].occupied}");
                    //if (instance._extendedPlayerData[i].occupied)
                    //    Debug.Log($"Player Extended Public Data {instance._extendedPlayerData[i].playerExtendedPublicData.player_id}");

                    if (i > 0 && !instance._playerDataCells[i].occupied)
                    {
                        instance._playerDataCells[i].playerExtendedPublicData = pepd;
                        break;
                    }
                }
            //}
        }
    }


    public void RemoveExtendedPlayerData(int id)
    {
        for (int i = 0; i < instance._playerDataCells.Count; i++)
        {
            if (instance._playerDataCells[i].occupied)
                if (instance._playerDataCells[i].playerExtendedPublicData.player_id == id)
                {
                    instance._playerDataCells[i].occupied = false;
                    instance._playerDataCells[i].local = false;
                    instance._playerDataCells[i].team = GameManager.Team.None;
                    instance._playerDataCells[i].playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
                }
        }
    }

    public bool PlayerExtendedDataContainsPlayerName(string n)
    {
        //foreach (ScriptObjPlayerData pepd in instance._extendedPlayerData)
        //    if (pepd.playerExtendedPublicData.username.Equals(n))
        //        return true;


        for (int i = 0; i < instance._playerDataCells.Count; i++)
        {
            //Debug.Log(n == null);
            //Debug.Log($"PlayerExtendedDataContainsPlayerName {instance._extendedPlayerData[i] == null}");
            //Debug.Log($"PlayerExtendedDataContainsPlayerName {instance._extendedPlayerData[i].playerExtendedPublicData == null}");
            Debug.Log($"PlayerExtendedDataContainsPlayerName {instance._playerDataCells[i].playerExtendedPublicData.username == null}");
            if (instance._playerDataCells[i].occupied)
                if (instance._playerDataCells[i].playerExtendedPublicData.username != null)
                    if (instance._playerDataCells[i].playerExtendedPublicData.username.Equals(n))
                    {
                        return true;
                    }

        }



        return false;
    }

    public bool PlayerDataContains(int id)
    {
        for (int i = 0; i < instance._playerDataCells.Count; i++)
        {
            Debug.Log($"PlayerDataContains [{instance._playerDataCells[i].playerExtendedPublicData.player_id}:{id}]");
            if (instance._playerDataCells[i].occupied)
                if (instance._playerDataCells[i].playerExtendedPublicData.player_id == id)
                    return true;
        }
        return false;
    }










    public void AddTeamData(string pn, GameManager.Team t)
    {
        ScriptObjBipedTeam bt = _bipedTeams.FirstOrDefault(i => i.playerName == pn);
        if (bt != null)
        {
            bt.team = t;
        }
        else
        {
            bt = _bipedTeams.FirstOrDefault(i => i.playerName == "");
            bt.playerName = pn; bt.team = t;
        }
    }




    public void CreateCarnageReportData()
    {
        print($"CreateCarnageReportData {instance._playerDataCells.Where(item => item.occupied).Count()}");
        for (int i = 0; i < instance._playerDataCells.Count; i++)
        {
            if (instance._playerDataCells[i].occupied)
            {
                MenuManager.Instance.GetMenu("carnage report").GetComponent<CarnageReportMenu>().AddStruct(
                    new CarnageReportStruc(instance._playerDataCells[i].playerCurrentGameScore,
                    instance._playerDataCells[i].playerExtendedPublicData.username,
                    instance._playerDataCells[i].playerExtendedPublicData.armor_color_palette, instance._playerDataCells[i].team));
            }
        }
    }

    public void ResetAllPlayerDataExceptMine() // Called when leaving a room, so no need to destroy player
    {

        for (int i = 0; i < instance._playerDataCells.Count; i++)
        {
            instance._playerDataCells[i].team = GameManager.Team.None;
            instance._playerDataCells[i].playerCurrentGameScore = new PlayerCurrentGameScore();

            if (i > 0)
            {

                {
                    instance._playerDataCells[i].occupied = false;
                    instance._playerDataCells[i].local = false;
                    instance._playerDataCells[i].rewiredId = 0;
                    instance._playerDataCells[i].photonRoomIndex = -999;
                    instance._playerDataCells[i].playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
                }
            }
        }
    }



    public void UpdateCountdowns(int vetoC, int roomGameStartC)
    {
        _vetoCountdown = vetoC;
        _roomGameStartCountdown = roomGameStartC;
    }

    IEnumerator GameStartDelayMapCamera_Coroutine()
    {
        yield return new WaitForSeconds(2);

        MapCamera.instance.TriggerGameStartBehaviour();
    }


    void TriggerGameStartCountdown()
    {
        _gameStartCountdown = 8;
    }


    public PlayerDatabaseAdaptor.PlayerExtendedPublicData GetPLayerExtendedData(string usernam)
    {
        foreach (ScriptObjPlayerData pepd in instance._playerDataCells)
            if (pepd.occupied)
                if (pepd.playerExtendedPublicData.username.Equals(usernam))
                    return pepd.playerExtendedPublicData;

        return null;
    }
    public static ScriptObjPlayerData GetDataCellWithDatabaseIdAndRewiredId(int playerId, int rewiredId)
    {
        Debug.Log($"GetPlayerDataWithId {playerId} {rewiredId}");
        //foreach (ScriptObjPlayerData s in instance.playerDataCells)
        //    if (s.occupied)
        //        print($"{s.playerExtendedPublicData.player_id} {s.rewiredId}");

        return CurrentRoomManager.instance.playerDataCells.FirstOrDefault(item => item.playerExtendedPublicData.player_id == playerId && item.rewiredId == rewiredId);
        //return instance.playerDataCells.FirstOrDefault(item => item.playerExtendedPublicData.player_id == playerId && item.rewiredId == rewiredId);
    }

    public static ScriptObjPlayerData GetLocalPlayerData(int _id)
    {
        return instance.playerDataCells[_id];
    }

    public static int GetUnoccupiedDataCell()
    {
        foreach (ScriptObjPlayerData s in instance._playerDataCells)
        {
            if (!s.occupied) return instance._playerDataCells.IndexOf(s);
        }

        return -1;
    }
}
