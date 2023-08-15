using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentRoomManager : MonoBehaviour
{
    public static CurrentRoomManager instance { get { return _instance; } }


    // Events
    public delegate void GameManagerEvent(CurrentRoomManager gme);
    public GameManagerEvent OnAllPlayersJoinedRoom, OnGameStarted;

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
                OnAllPlayersJoinedRoom?.Invoke(this);
            }
        }
    }

    public bool gameStarted
    {
        get { return _gameStarted; }
        private set
        {
            _gameStarted = value;
            if (value)
                OnGameStarted?.Invoke(this);
            else
            {
                _gameStartedTime = GameManager.GameStartDelay;
                allPlayersJoined = false;
            }
        }
    }

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
    public int expectedNbPlayers { get { return _expectedNbPlayers; } private set { _expectedNbPlayers = value; } }


    int expectedMapAddOns { get { return _expectedMapAddOns; } set { _expectedMapAddOns = value; } }

    [SerializeField] bool _gameStarted;
    [SerializeField] bool _mapIsReady;
    [SerializeField] bool _allPlayersJoined;
    [SerializeField] float _gameStartedTime;

    [SerializeField] int _expectedMapAddOns, _spawnedMapAddOns, _expectedNbPlayers, _nbPlayersJoined;


    static CurrentRoomManager _instance;

    Dictionary<string, int> _playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();










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


    private void Start()
    {
        OnAllPlayersJoinedRoom += OnAllPlayersJoinedGame;
    }

    private void Update()
    {
        if (allPlayersJoined && !gameStarted)
        {
            _gameStartedTime -= Time.deltaTime;

            if (_gameStartedTime <= 0)
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


        if (scene.buildIndex == 0)
        {
            expectedMapAddOns = 0; spawnedMapAddOns = 0;

            playerNicknameNbLocalPlayersDict = new Dictionary<string, int>();
            allPlayersJoined = false;
            OnAllPlayersJoinedRoom = null;


            OnAllPlayersJoinedRoom -= FindObjectOfType<SwarmManager>().OnAllPlayersJoinedRoom_Delegate;
            OnAllPlayersJoinedRoom += FindObjectOfType<SwarmManager>().OnAllPlayersJoinedRoom_Delegate;
        }
        else
        {
            expectedMapAddOns = FindObjectsOfType<NetworkGrenadeSpawnPoint>().Length
                + FindObjectsOfType<NetworkWeaponSpawnPoint>().Length + FindObjectsOfType<Hazard>().Length;
        }
    }

    void OnAllPlayersJoinedGame(CurrentRoomManager gme)
    {
        _gameStartedTime = GameManager.GameStartDelay;
    }

}
