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
    int expectedMapAddOns { get { return _expectedMapAddOns; } set { _expectedMapAddOns = value; } }

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
            if (_randomQuickMatchSeetingsChosen && _vetoCountdown > 0)
            {
                _vetoCountdown -= Time.deltaTime;

                Launcher.instance.gameCountdownText.text = $"VETO COUNTDOWN: {((int)_vetoCountdown)} seconds\nVetos: {instance.vetos} out of {instance.expectedNbPlayers}";

                if (_vetoCountdown <= 0)
                {
                    Launcher.instance.vetoBtn.SetActive(false);

                    if (_vetos > _expectedNbPlayers * 0.5f && PhotonNetwork.IsMasterClient)
                        ChooseRandomMatchSettingsForQuickMatch();
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

        if (scene.buildIndex == 0)
        {

            _mapIsReady = _allPlayersJoined = _gameIsReady = _gameStart = _gameOver = _gameStarted =
                _reachedHalwayGameStartCountdown = _randomQuickMatchSeetingsChosen = false;
            _gameStartCountdown = _expectedMapAddOns = _spawnedMapAddOns = _expectedNbPlayers = _nbPlayersJoined = 0;
            playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();
        }
        else
        {
            expectedMapAddOns = FindObjectsOfType<NetworkGrenadeSpawnPoint>().Length
                + FindObjectsOfType<NetworkWeaponSpawnPoint>().Length + FindObjectsOfType<Hazard>().Length;
        }
    }

    void OnAllPlayersJoinedGame(CurrentRoomManager gme)
    {
        _gameStartCountdown = GameManager.GameStartDelay;
    }


    public void ChooseRandomMatchSettingsForQuickMatch()
    {
        Debug.Log("ChooseRandomMatchSettingsForQuickMatch");
        var ran = Random.Range(0, 5);

        if (ran > 1) // PvP
        {
            GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;

            ran = Random.Range(0, 100);

            if (ran <= 25)
                GameManager.instance.gameType = GameManager.GameType.Slayer;
            else if (ran <= 60)
                GameManager.instance.gameType = GameManager.GameType.Pro;
            else if (ran <= 70)
                GameManager.instance.gameType = GameManager.GameType.Swat;
            else if (ran <= 80)
                GameManager.instance.gameType = GameManager.GameType.Hill;
            else if (ran <= 90)
                GameManager.instance.gameType = GameManager.GameType.Retro;
            else if (ran <= 100)
                GameManager.instance.gameType = GameManager.GameType.Snipers;





            ran = Random.Range(0, 100);

            if (ran <= 10)
                Launcher.instance.ChangeLevelToLoadWithIndex(5);// Cargo
            else if (ran <= 20)
                Launcher.instance.ChangeLevelToLoadWithIndex(6);// Oasis
            else if (ran <= 30)
                Launcher.instance.ChangeLevelToLoadWithIndex(7);// Showdown
            else if (ran <= 40)
                Launcher.instance.ChangeLevelToLoadWithIndex(8);// Babylon
            else if (ran <= 50)
                Launcher.instance.ChangeLevelToLoadWithIndex(9);// Starship
            else if (ran <= 60)
                Launcher.instance.ChangeLevelToLoadWithIndex(10);// Temple
            else if (ran <= 70)
                Launcher.instance.ChangeLevelToLoadWithIndex(11);// Blizzard
            else if (ran <= 80)
                Launcher.instance.ChangeLevelToLoadWithIndex(12);// Factory
            else if (ran <= 90)
                Launcher.instance.ChangeLevelToLoadWithIndex(17);// Parasite
            else if (ran <= 100)
                Launcher.instance.ChangeLevelToLoadWithIndex(18);// Shaman

        }
        else // PvE
        {
            GameManager.instance.gameMode = GameManager.GameMode.Swarm;
            GameManager.instance.difficulty = SwarmManager.Difficulty.Heroic;
        }



        _randomQuickMatchSeetingsChosen = true;


        FindObjectOfType<NetworkGameManager>().SendGameParams();
    }

    public void ResetRoomCountdowns()
    {
        _vetoCountdown = 9;
        _gameStartCountdown = 9;
    }
}
