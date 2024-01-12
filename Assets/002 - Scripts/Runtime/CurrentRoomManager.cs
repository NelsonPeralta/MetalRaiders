using ExitGames.Client.Photon.Encryption;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentRoomManager : MonoBehaviour
{
    public static CurrentRoomManager instance { get { return _instance; } }


    // Events
    public delegate void GameManagerEvent(CurrentRoomManager gme);
    public GameManagerEvent OnGameIsReady, OnGameStarted, OnGameStartedLate;

    public enum RoomType
    {
        QuickMatch, Private
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
            Debug.Log(expectedMapAddOns);
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

            if (_spawnedMapAddOns == expectedMapAddOns)
            {
                Debug.Log($"Spawn Map Add-Ons: {value} ({expectedMapAddOns} needed)");
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
                StartCoroutine(GameManager.instance.SpawnPlayers_Coroutine());
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
                Debug.Log("OnAllPlayersJoinedRoom");
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
                Debug.Log("gameIsReady");
                _gameIsReady = true;

                OnGameIsReady?.Invoke(this);


                StartCoroutine(GameStartDelayMapCamera_Coroutine());
                StartCoroutine(GameStartDelay_Coroutine());
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
    public List<ScriptObjPlayerData> extendedPlayerData { get { return _extendedPlayerData; } }
    public List<ScriptObjBipedTeam> teamsData { get { return _bipedTeams; } }

    [SerializeField] bool _mapIsReady, _allPlayersJoined, _gameIsReady;
    [SerializeField] bool _gameStarted, _gameOver;
    [SerializeField] float _gameStartCountdown, _roomGameStartCountdown, _vetoCountdown = 9, _rpcCooldown;

    [SerializeField] int _expectedMapAddOns, _spawnedMapAddOns, _expectedNbPlayers, _playersLoadedScene, _nbPlayersJoined, _vetos;

    [SerializeField] RoomType _roomType;

    static CurrentRoomManager _instance;

    Dictionary<string, int> _playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

    [SerializeField] bool _reachedHalwayGameStartCountdown, _randomInitiQuickMatchSettingsChosen, _randomPvPSettingsChosen;
    //[SerializeField] Dictionary<string, PlayerDatabaseAdaptor.PlayerExtendedPublicData> _extendedPlayerData = new Dictionary<string, PlayerDatabaseAdaptor.PlayerExtendedPublicData>();

    [SerializeField] GameManager.GameType _vetoedGameType;
    [SerializeField] int _ran, _vetoedMapIndex;

    [SerializeField] List<ScriptObjPlayerData> _extendedPlayerData = new List<ScriptObjPlayerData>();
    [SerializeField] List<ScriptObjBipedTeam> _bipedTeams;


    int ran, _minH;
    bool _achievementUnlocked = false;
    string _tempAchievementName = "";





    void Awake()
    {
        _rpcCooldown = 0.3f;
        vetoCountdown = roomGameStartCountdown = 9;

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

        foreach (ScriptObjPlayerData sod in instance._extendedPlayerData)
            sod.Reset();
    }

    private void Update()
    {


        //if (gameStart && !gameStarted)
        //{
        //    _gameStartCountdown -= Time.deltaTime;

        //    if (!reachedHalwayGameStartCountdown && _gameStartCountdown <= GameManager.GameStartDelay)
        //        reachedHalwayGameStartCountdown = true;

        //    if (_gameStartCountdown <= 0)
        //        gameStarted = true;
        //}

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
                        //Launcher.instance.StartGame();
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
        _reachedHalwayGameStartCountdown = false;

        _expectedMapAddOns = _vetos = 0;

        if (scene.buildIndex == 0)
        {

            _mapIsReady = _allPlayersJoined = _gameIsReady = _gameOver = _gameStarted =
                _reachedHalwayGameStartCountdown = _randomInitiQuickMatchSettingsChosen = false;
            _gameStartCountdown = _expectedMapAddOns = _spawnedMapAddOns = _expectedNbPlayers = _nbPlayersJoined = _playersLoadedScene = 0;
            playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

            _vetoedGameType = GameManager.GameType.Unassgined; _vetoedMapIndex = 0;
        }
        else
        {
            for (int i = 0; i < GameManager.instance.nbLocalPlayersPreset; i++)
                NetworkGameManager.instance.AddPlayerLoadedScene();


            if (GameManager.instance.gameType != GameManager.GameType.GunGame && GameManager.instance.gameType != GameManager.GameType.Fiesta)
                expectedMapAddOns = FindObjectsOfType<NetworkGrenadeSpawnPoint>().Length
                + FindObjectsOfType<Hazard>().Length;
            Debug.Log(expectedMapAddOns);


            if (GameManager.instance.gameType != GameManager.GameType.GunGame && GameManager.instance.gameType != GameManager.GameType.Fiesta)
                foreach (NetworkWeaponSpawnPoint nwsp in FindObjectsOfType<NetworkWeaponSpawnPoint>())
                    if (nwsp.transform.root.GetComponent<Player>() == null)
                    {
                        Debug.Log(nwsp.name);
                        expectedMapAddOns += 1;
                    }
        }
    }

    void OnAllPlayersJoinedGame(CurrentRoomManager gme)
    {
        _gameStartCountdown = GameManager.GameStartDelay;
    }





    public void ChooseRandomMatchSettingsForQuickMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("ChooseRandomMatchSettingsForQuickMatch");
        _ran = Random.Range(0, 5);
        _ran = 2;

        if (expectedNbPlayers == 1)
        {
            GameManager.instance.gameMode = GameManager.GameMode.Swarm;
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
            GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;
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
        _ran = Random.Range(0, 2);

        if (_ran <= 1)
            Launcher.instance.ChangeLevelToLoadWithIndex(11);// Downpoor
        else if (_ran <= 2)
            Launcher.instance.ChangeLevelToLoadWithIndex(12);// Haunted
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
            instance._extendedPlayerData[0].playerExtendedPublicData = pepd;









            if (instance._extendedPlayerData[0].playerExtendedPublicData.level >= 5)
            {
                _achievementUnlocked = false; _tempAchievementName = "BABYSTEPS";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);

                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    //AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }


            if (instance._extendedPlayerData[0].playerExtendedPublicData.level >= 25)
            {
                _achievementUnlocked = false; _tempAchievementName = "WHWT";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    //AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }



            if (instance._extendedPlayerData[0].playerExtendedPublicData.level == 50)
            {
                _achievementUnlocked = false; _tempAchievementName = "MAXOUT";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    //AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }



            _minH = PlayerProgressionManager.GetMinHonorForRank("Sergeant");
            if (instance._extendedPlayerData[0].playerExtendedPublicData.honor >= _minH)
            {
                _achievementUnlocked = false; _tempAchievementName = "VETERAN";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    //AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }

            _minH = PlayerProgressionManager.GetMinHonorForRank("Second Lieutenant");
            if (instance._extendedPlayerData[0].playerExtendedPublicData.honor >= _minH)
            {
                _achievementUnlocked = false; _tempAchievementName = "SIR";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                Debug.Log($"Unlocked Achivement {_tempAchievementName} {_achievementUnlocked}");
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    //AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }


            _minH = PlayerProgressionManager.GetMinHonorForRank("General");
            if (instance._extendedPlayerData[0].playerExtendedPublicData.honor >= _minH)
            {
                _achievementUnlocked = false; _tempAchievementName = "LAIC";
                Steamworks.SteamUserStats.GetAchievement(_tempAchievementName, out _achievementUnlocked);
                if (!_achievementUnlocked)
                {
                    Debug.Log($"Unlocked Achivement {_tempAchievementName}");
                    //AchievementManager.UnlockAchievement(_tempAchievementName);
                }
            }





        }
        else
            for (int i = 0; i < instance._extendedPlayerData.Count; i++)
            {
                //Debug.Log($"Player Extended Public Data {i}");
                //Debug.Log($"Player Extended Public Data {instance._extendedPlayerData[i].occupied}");
                //if (instance._extendedPlayerData[i].occupied)
                //    Debug.Log($"Player Extended Public Data {instance._extendedPlayerData[i].playerExtendedPublicData.player_id}");

                if (i > 0 && !instance._extendedPlayerData[i].occupied)
                {
                    instance._extendedPlayerData[i].playerExtendedPublicData = pepd;
                    break;
                }
            }
    }

    public void RemoveExtendedPlayerData(string n)
    {
        for (int i = 0; i < instance._extendedPlayerData.Count; i++)
        {
            if (instance._extendedPlayerData[i].occupied)
                if (instance._extendedPlayerData[i].playerExtendedPublicData.username.Equals(n))
                {
                    instance._extendedPlayerData[i].playerExtendedPublicData = null;
                }
        }
    }

    public void RemoveExtendedPlayerData(int id)
    {
        for (int i = 0; i < instance._extendedPlayerData.Count; i++)
        {
            if (instance._extendedPlayerData[i].occupied)
                if (instance._extendedPlayerData[i].playerExtendedPublicData.player_id == id)
                {
                    instance._extendedPlayerData[i].occupied = false;
                    instance._extendedPlayerData[i].team = GameManager.Team.None;
                    instance._extendedPlayerData[i].playerExtendedPublicData = null;
                }
        }
    }

    public bool PlayerExtendedDataContainsPlayerName(string n)
    {
        //foreach (ScriptObjPlayerData pepd in instance._extendedPlayerData)
        //    if (pepd.playerExtendedPublicData.username.Equals(n))
        //        return true;


        for (int i = 0; i < instance._extendedPlayerData.Count; i++)
        {
            //Debug.Log(n == null);
            //Debug.Log($"PlayerExtendedDataContainsPlayerName {instance._extendedPlayerData[i] == null}");
            //Debug.Log($"PlayerExtendedDataContainsPlayerName {instance._extendedPlayerData[i].playerExtendedPublicData == null}");
            Debug.Log($"PlayerExtendedDataContainsPlayerName {instance._extendedPlayerData[i].playerExtendedPublicData.username == null}");
            if (instance._extendedPlayerData[i].occupied)
                if (instance._extendedPlayerData[i].playerExtendedPublicData.username != null)
                    if (instance._extendedPlayerData[i].playerExtendedPublicData.username.Equals(n))
                    {
                        return true;
                    }

        }



        return false;
    }

    public bool PlayerDataContains(int id)
    {
        for (int i = 0; i < instance._extendedPlayerData.Count; i++)
        {
            Debug.Log($"PlayerDataContains [{instance._extendedPlayerData[i].playerExtendedPublicData.player_id}:{id}]");
            if (instance._extendedPlayerData[i].occupied)
                if (instance._extendedPlayerData[i].playerExtendedPublicData.player_id == id)
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
    public void ResetAllPlayerDataExceptMine() // Called when leaving a room, so no need to destroy player
    {
        for (int i = 0; i < instance._extendedPlayerData.Count; i++)
            if (i > 0)
            {
                instance._extendedPlayerData[i].occupied = false;
                instance._extendedPlayerData[i].team = GameManager.Team.None;
                instance._extendedPlayerData[i].playerExtendedPublicData = null;
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


    IEnumerator GameStartDelay_Coroutine()
    {
        yield return new WaitForSeconds(8);

        MapCamera.instance.gameObject.SetActive(false);
        foreach (Player p in GameManager.instance.localPlayers.Values)
            p.TriggerGameStartBehaviour();

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            SwarmManager.instance.Begin();

        foreach (Player p in GameManager.instance.localPlayers.Values) p.playerInventory.TriggerStartGameBehaviour();

        gameStarted = true;
    }



    public PlayerDatabaseAdaptor.PlayerExtendedPublicData GetPLayerExtendedData(string u)
    {
        foreach (ScriptObjPlayerData pepd in instance._extendedPlayerData)
            if (pepd.occupied)
                if (pepd.playerExtendedPublicData.username.Equals(u))
                    return pepd.playerExtendedPublicData;

        return null;
    }
    public ScriptObjPlayerData GetPlayerDataWithId(int playerId)
    {
        Debug.Log($"GetPlayerDataWithId {playerId}");
        return instance.extendedPlayerData.FirstOrDefault(item => item.playerExtendedPublicData.player_id == playerId);
    }
}
