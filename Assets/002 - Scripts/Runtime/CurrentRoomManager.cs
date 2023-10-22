using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentRoomManager : MonoBehaviour
{
    public static CurrentRoomManager instance { get { return _instance; } }


    // Events
    public delegate void GameManagerEvent(CurrentRoomManager gme);
    public GameManagerEvent OnGameStartedEarly, OnGameStarted, OnGameStartedLate;

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
            Debug.Log($"playerNicknameNbLocalPlayersDict: {playerNicknameNbLocalPlayersDict}");
            int c = 0;

            foreach (KeyValuePair<string, int> items in playerNicknameNbLocalPlayersDict)
            {
                print("You have " + items.Value + " " + items.Key);
                c += items.Value;
            }
            expectedNbPlayers = c;

            if (_vetoCountdown > 0) // Make sure this variable if greater than 0 by default
                _vetoCountdown = 9;
            _roomGameStartCountdown = 9;

            if (expectedNbPlayers > 0 && !_randomQuickMatchSeetingsChosen)
            {
                if (CurrentRoomManager.instance.roomType == RoomType.QuickMatch)
                    ChooseRandomMatchSettingsForQuickMatch();
            }
        }
    }
    public int expectedNbPlayers { get { return _expectedNbPlayers; } private set { _expectedNbPlayers = value; } }


    /// <summary>
    /// Step 1: Caluclate expected Map Add-Ons
    /// </summary>
    int expectedMapAddOns
    {
        get { return _expectedMapAddOns; }
        set
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
            Debug.Log($"spawnedMapAddOns: {value}");

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
                StartCoroutine(GameManager.instance.SpawnPlayers_Coroutine());
            }
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
                //OnAllPlayersJoinedRoom?.Invoke(this);
            }
        }
    }

    bool gameIsReady
    {
        get { return _gameIsReady; }
        set
        {
            bool _preVal = _gameIsReady;
            _gameIsReady = value;

            if (value && _preVal != value)
            {
                Debug.Log("gameIsReady");
                _gameIsReady = true;
                gameStart = true;
            }
        }
    }

    bool gameStart
    {
        get { return _gameStart; }
        set
        {
            bool _preVal = _gameStart;
            _gameStart = value;

            if (value && _preVal != value)
            {
                OnGameStartedEarly?.Invoke(this);
                _gameStartCountdown = GameManager.GameStartDelay;
            }
            _gameStart = value;
        }
    }

    bool reachedHalwayGameStartCountdown
    {
        get { return _reachedHalwayGameStartCountdown; }
        set
        {
            bool _preVal = _reachedHalwayGameStartCountdown;
            _reachedHalwayGameStartCountdown = value;

            if (value && _preVal != value)
            {
                MapCamera.instance.TriggerGameStartBehaviour();
            }
        }
    }

    public bool gameStarted
    {
        get { return _gameStarted; }
        private set
        {
            bool _preVal = _gameStarted;
            _gameStarted = value;

            if (value && _preVal != value)
            {
                _gameStarted = true;
                MapCamera.instance.gameObject.SetActive(false);
                foreach (Player p in GameManager.instance.localPlayers.Values)
                    p.TriggerGameStartBehaviour();

                if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                    SwarmManager.instance.Begin();
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


    [SerializeField] bool _mapIsReady, _allPlayersJoined, _gameIsReady;
    [SerializeField] bool _gameStart, _gameStarted, _gameOver;
    [SerializeField] float _gameStartCountdown, _roomGameStartCountdown, _vetoCountdown = 9;

    [SerializeField] int _expectedMapAddOns, _spawnedMapAddOns, _expectedNbPlayers, _nbPlayersJoined, _vetos;

    [SerializeField] RoomType _roomType;

    static CurrentRoomManager _instance;

    Dictionary<string, int> _playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

    [SerializeField] bool _reachedHalwayGameStartCountdown, _randomQuickMatchSeetingsChosen;
    //[SerializeField] Dictionary<string, PlayerDatabaseAdaptor.PlayerExtendedPublicData> _extendedPlayerData = new Dictionary<string, PlayerDatabaseAdaptor.PlayerExtendedPublicData>();

    [SerializeField] GameManager.GameType _vetoedGameType;
    [SerializeField] int _ran, _vetoedMapIndex;

    [SerializeField] List<ScriptObjPlayerData> _extendedPlayerData = new List<ScriptObjPlayerData>();

    int ran;






    void Awake()
    {
        _vetoCountdown = 9;

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log($"CurrentRoomManager Instance");
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        _vetoCountdown = 9;

        foreach (ScriptObjPlayerData sod in instance._extendedPlayerData)
        {
            sod.playerExtendedPublicData = null;
        }
    }

    private void Update()
    {
        if (gameStart && !gameStarted)
        {
            _gameStartCountdown -= Time.deltaTime;

            if (!reachedHalwayGameStartCountdown && _gameStartCountdown <= GameManager.GameStartDelay)
                reachedHalwayGameStartCountdown = true;

            if (_gameStartCountdown <= 0)
                gameStarted = true;
        }


        if (PhotonNetwork.InRoom)
        {

            if ((expectedNbPlayers - GameManager.instance.nbLocalPlayersPreset) >= 0) // At least one more stranger player is in the room
                if (_randomQuickMatchSeetingsChosen && _vetoCountdown > 0)
                {
                    _vetoCountdown -= Time.deltaTime;

                    Launcher.instance.gameCountdownText.text = $"VETO COUNTDOWN: {((int)_vetoCountdown)} seconds\nVetos: {instance.vetos} out of {instance.expectedNbPlayers}";

                    if (_vetoCountdown <= 0)
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




            if (_randomQuickMatchSeetingsChosen && _roomGameStartCountdown > 0 && _vetoCountdown <= 0)
            {
                _roomGameStartCountdown -= Time.deltaTime;

                Launcher.instance.gameCountdownText.text = $"Game Starts in: {((int)_roomGameStartCountdown)}";

                if (CurrentRoomManager.instance.roomType == RoomType.QuickMatch)
                    if (_roomGameStartCountdown <= 0 && PhotonNetwork.IsMasterClient)
                        Launcher.instance.StartGame();
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("GAME MANAGER EVENTS: OnSceneLoaded");

        gameStarted = false;
        _reachedHalwayGameStartCountdown = false;

        _expectedMapAddOns = _vetos = 0;

        if (scene.buildIndex == 0)
        {

            _mapIsReady = _allPlayersJoined = _gameIsReady = _gameStart = _gameOver = _gameStarted =
                _reachedHalwayGameStartCountdown = _randomQuickMatchSeetingsChosen = false;
            _gameStartCountdown = _expectedMapAddOns = _spawnedMapAddOns = _expectedNbPlayers = _nbPlayersJoined = 0;
            playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

            _vetoedGameType = GameManager.GameType.Unassgined; _vetoedMapIndex = 0;
        }
        else
        {
            expectedMapAddOns = FindObjectsOfType<NetworkGrenadeSpawnPoint>().Length
                + FindObjectsOfType<Hazard>().Length;
            Debug.Log(expectedMapAddOns);


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
        Debug.Log("ChooseRandomMatchSettingsForQuickMatch");
        _ran = Random.Range(0, 5);
        _ran = 2;

        if (expectedNbPlayers == 1)
        {
            GameManager.instance.gameMode = GameManager.GameMode.Swarm;
            GameManager.instance.difficulty = SwarmManager.Difficulty.Heroic;

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

        }
        //else // PvE
        //{
        //    GameManager.instance.gameMode = GameManager.GameMode.Swarm;
        //    GameManager.instance.difficulty = SwarmManager.Difficulty.Heroic;

        //    ChooseRandomPvEMap();
        //    if (_vetoedMapIndex != 0)
        //        while (_vetoedMapIndex == Launcher.instance.levelToLoadIndex)
        //        {
        //            ChooseRandomPvEMap();
        //        }
        //}



        _randomQuickMatchSeetingsChosen = true;


        FindObjectOfType<NetworkGameManager>().SendGameParams();
    }

    void ChooseRandomPvPGameType()
    {
        _ran = Random.Range(0, 100);

        if (_ran <= 25)
            GameManager.instance.gameType = GameManager.GameType.Slayer;
        else if (_ran <= 60)
            GameManager.instance.gameType = GameManager.GameType.Pro;
        else if (_ran <= 70)
            GameManager.instance.gameType = GameManager.GameType.Swat;
        else if (_ran <= 80)
            GameManager.instance.gameType = GameManager.GameType.Hill;
        else if (_ran <= 90)
            GameManager.instance.gameType = GameManager.GameType.Retro;
        else if (_ran <= 100)
            GameManager.instance.gameType = GameManager.GameType.Snipers;
    }

    void ChooseRandomPvPMap()
    {
        _ran = Random.Range(0, 100);

        if (_ran <= 10)
            Launcher.instance.ChangeLevelToLoadWithIndex(5);// Cargo
        else if (_ran <= 20)
            Launcher.instance.ChangeLevelToLoadWithIndex(6);// Oasis
        else if (_ran <= 30)
            Launcher.instance.ChangeLevelToLoadWithIndex(7);// Showdown
        else if (_ran <= 40)
            Launcher.instance.ChangeLevelToLoadWithIndex(8);// Babylon
        else if (_ran <= 50)
            Launcher.instance.ChangeLevelToLoadWithIndex(9);// Starship
        else if (_ran <= 60)
            Launcher.instance.ChangeLevelToLoadWithIndex(10);// Temple
        else if (_ran <= 70)
            Launcher.instance.ChangeLevelToLoadWithIndex(11);// Blizzard
        else if (_ran <= 80)
            Launcher.instance.ChangeLevelToLoadWithIndex(12);// Factory
        else if (_ran <= 90)
            Launcher.instance.ChangeLevelToLoadWithIndex(17);// Parasite
        else if (_ran <= 100)
            Launcher.instance.ChangeLevelToLoadWithIndex(18);// Shaman
    }

    void ChooseRandomPvEMap()
    {
        _ran = Random.Range(0, 2);

        if (_ran <= 1)
            Launcher.instance.ChangeLevelToLoadWithIndex(14);// Downpoor
        else if (_ran <= 2)
            Launcher.instance.ChangeLevelToLoadWithIndex(15);// Haunted
    }

    public void ResetRoomCountdowns()
    {
        _vetoCountdown = 9;
        _gameStartCountdown = 9;
    }

    public void AddExtendedPlayerData(PlayerDatabaseAdaptor.PlayerExtendedPublicData pepd)
    {
        Debug.Log(pepd.username.Equals(GameManager.ROOT_PLAYER_NAME));

        if (pepd.username.Equals(GameManager.ROOT_PLAYER_NAME))
            instance._extendedPlayerData[0].playerExtendedPublicData = pepd;
        else
            for (int i = 0; i < CurrentRoomManager.instance._extendedPlayerData.Count; i++)
            {
                Debug.Log($"Player Extended Public Data {CurrentRoomManager.instance._extendedPlayerData[i].playerExtendedPublicData.player_id}");
                if (i > 0 && !instance._extendedPlayerData[i].occupied)
                {
                    Debug.Log("Player Extended Public Data");
                    CurrentRoomManager.instance._extendedPlayerData[i].playerExtendedPublicData = pepd;
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

    public void SoftResetPlayerExtendedData()
    {
        for (int i = 0; i < instance._extendedPlayerData.Count; i++)
            if (i > 0)
            {
                instance._extendedPlayerData[i].playerExtendedPublicData = null;
            }
    }

    public PlayerDatabaseAdaptor.PlayerExtendedPublicData GetPLayerExtendedData(string u)
    {
        foreach (ScriptObjPlayerData pepd in instance._extendedPlayerData)
            if (pepd.occupied)
                if (pepd.playerExtendedPublicData.username.Equals(u))
                    return pepd.playerExtendedPublicData;

        return null;
    }
}
