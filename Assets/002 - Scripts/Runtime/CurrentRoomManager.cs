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
            }
        }
    }







    [SerializeField] bool _mapIsReady, _allPlayersJoined, _gameIsReady;
    [SerializeField] bool _gameStart, _gameStarted;
    [SerializeField] float _gameStartCountdown;

    [SerializeField] int _expectedMapAddOns, _spawnedMapAddOns, _expectedNbPlayers, _nbPlayersJoined;


    static CurrentRoomManager _instance;

    Dictionary<string, int> _playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();

    bool _reachedHalwayGameStartCountdown;








    void Awake()
    {
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


    private void Update()
    {
        if (gameStart)
        {
            _gameStartCountdown -= Time.deltaTime;

            if (!reachedHalwayGameStartCountdown && _gameStartCountdown <= GameManager.GameStartDelay)
                reachedHalwayGameStartCountdown = true;

            if (_gameStartCountdown <= 0)
                gameStarted = true;
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
            expectedMapAddOns = 0; spawnedMapAddOns = 0;

            playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();
            allPlayersJoined = false;
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

}
