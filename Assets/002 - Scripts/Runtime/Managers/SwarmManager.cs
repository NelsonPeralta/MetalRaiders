using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Events;
using System.Linq;

public class SwarmManager : MonoBehaviourPunCallbacks
{
    public bool editMode;
    // Events 
    public delegate void SwarmManagerEvent(SwarmManager swarmManager);
    public SwarmManagerEvent OnBegin, OnWaveIncrease, OnWaveStart, OnWaveEnd, OnAiDeath, OnPlayerLivesChanged, OnAiSpawn, OnAIsCalculated;

    // public variables
    public static SwarmManager instance;

    public enum Difficulty { Normal, Heroic, Legendary }
    public enum AiType { Undead, AlienShooter, Breather, Helldog, FlameTyrant }

    [SerializeField] int _currentWave;
    public int currentWave
    {
        get { return _currentWave; }
        private set
        {
            int previousValue = _currentWave;
            _currentWave = value;

            if (previousValue < value)
            {
                waveEnded = false;
            }
        }
    }
    public int nextWaveDelay;


    [SerializeField] GameObject zombiePrefab;
    [SerializeField] GameObject knightPrefab;
    [SerializeField] GameObject alienShooterPrefab;
    [SerializeField] GameObject tyrantPrefab;
    [SerializeField] GameObject helldogPrefab;

    public List<Undead> zombieList { get { return _zombieList; } set { _zombieList = value; } }
    public List<Breather> knightPool { get { return _knightPool; } set { _knightPool = value; } }
    public List<AlienShooter> watcherPool { get { return _watcherPool; } set { _watcherPool = value; } }


    [Header("AI Pools")]
    List<Undead> _zombieList = new List<Undead>();
    public List<Breather> _knightPool = new List<Breather>();
    public List<AlienShooter> _watcherPool = new List<AlienShooter>();
    public List<Helldog> hellhoundPool = new List<Helldog>();
    public List<FlameTyrant> tyrantPool = new List<FlameTyrant>();

    // private variables
    PhotonView PV;
    int maxWave;
    int _livesLeft = 4;
    float _newWaveCountdown;
    bool _waveEnded;


    [SerializeField] int _zombiesLeft;
    int _watchersLeft;
    int _knightsLeft;
    int _hellhoundsLeft;
    [SerializeField] int _tyrantsLeft;

    int _zombiesAlive;
    int _watchersAlive;
    int _knightsAlive;
    int _hellhoundsAlive;
    int _tyrantsAlive;

    int _maxAliensEnabled;
    int _maxBreathersEnabled;
    int _maxZombieEnabled;

    public List<HealthPack> healthPacks = new List<HealthPack>();

    [SerializeField] AudioClip _ambiantMusic;
    [SerializeField] AudioClip _waveStartClip;
    [SerializeField] AudioClip _weaponDropClip;
    [SerializeField] AudioClip _livesAddedClip;
    [SerializeField] AudioClip _waveSuccessClip;


    // constants
    const int ZOMBIE_SPAWN_DELAY = 4;
    const int SHOOTER_SPAWN_DELAY = 8;
    const int KNIGHT_SPAWN_DELAY = 6;
    const int HELLHOUND_SPAWN_DELAY = 5;
    const int TYRANT_SPAWN_DELAY = 4;

    public int zombiesLeft
    {
        get { return _zombiesLeft; }
        private set
        {
            int previousValue = _zombiesLeft;
            _zombiesLeft = value;

            if (previousValue > value)
                _zombiesAlive++;
        }
    }
    public int hellhoundsLeft
    {
        get { return _hellhoundsLeft; }
        private set
        {
            int previousValue = _hellhoundsLeft;
            _hellhoundsLeft = value;

            if (previousValue > value)
                _hellhoundsAlive++;
        }
    }
    public int watchersLeft
    {
        get { return _watchersLeft; }
        private set
        {
            int previousValue = _watchersLeft;
            _watchersLeft = value;

            if (previousValue > value)
                _watchersAlive++;
        }
    }

    public int knightsLeft
    {
        get { return _knightsLeft; }
        private set
        {
            int previousValue = _knightsLeft;
            _knightsLeft = value;

            if (previousValue > value)
                _knightsAlive++;
        }
    }

    public int tyrantsLeft
    {
        get { return _tyrantsLeft; }
        private set
        {
            Debug.Log($"Tyrants Left: {value}");
            int previousValue = _tyrantsLeft;
            _tyrantsLeft = value;

            if (previousValue > value)
                _tyrantsAlive++;
        }
    }

    public int zombiesAlive
    {
        get { return _zombiesAlive; }
        private set { _zombiesAlive = value; }
    }
    public int hellhoundsAlive
    {
        get { return _hellhoundsAlive; }
        private set { _hellhoundsAlive = value; }
    }

    public int watchersAlive
    {
        get { return _watchersAlive; }
        private set { _watchersAlive = value; }
    }

    public int knightsAlive
    {
        get { return _knightsAlive; }
        private set { _knightsAlive = value; }
    }

    public int tyrantsAlive
    {
        get { return _tyrantsAlive; }
        private set { _tyrantsAlive = value; }
    }
    public int livesLeft
    {
        get { return _livesLeft; }
        set
        {
            _livesLeft = value;
            OnPlayerLivesChanged?.Invoke(this);
        }
    }

    public bool waveEnded // PlayerUI detects OnAiDeath after OnWaveEnd, overwriting the bonus points message at the end of a wave. This bool is used to check if it is the end of a wave to handle that bug
    {
        get { return _waveEnded; }
        private set { _waveEnded = value; }
    }

    private void OnEnable()
    {
        //currentWave = 0;
        nextWaveDelay = 5;
    }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        GameManager.instance.OnSceneLoadedEvent -= OnSceneLoaded;
        GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;

        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom -= OnAllPlayersJoinedRoom_Delegate;
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom += OnAllPlayersJoinedRoom_Delegate;
    }

    NetworkSwarmManager _networkSwarmManager;

    private void Update()
    {
        NewWaveCountdown();
    }

    public void OnAllPlayersJoinedRoom_Delegate(GameManagerEvents gme)
    {
        Debug.Log("SWARM MANAGER: OnAllPlayersJoinedRoom_Delegate");
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            Begin();
    }

    void NewWaveCountdown()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            if (_newWaveCountdown > 0)
            {
                _newWaveCountdown -= Time.deltaTime;

                if (_newWaveCountdown <= 0)
                    IncreaseWave();
            }
    }
    void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        //currentWave = 0;
        nextWaveDelay = 5;

        if (editMode)
            currentWave = 9;

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            Debug.Log("SWARM MANAGER: OnSceneLoaded");
            if (GameManager.instance.gameMode != GameManager.GameMode.Swarm)
                return;

            instance = this;
            if (PhotonNetwork.IsMasterClient)
            {
                _networkSwarmManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkSwarmManager"), Vector3.zero, Quaternion.identity).GetComponent<NetworkSwarmManager>();
                PV = _networkSwarmManager.GetComponent<PhotonView>();

                for (int i = 0; i < 50; i++)
                {
                    //Transform sp = SpawnManager.spawnManagerInstance.GetRandomComputerSpawnPoint();
                    GameObject z = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", zombiePrefab.name), Vector3.zero, Quaternion.identity);
                    GameObject w = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", knightPrefab.name), Vector3.zero, Quaternion.identity);
                    GameObject a = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", alienShooterPrefab.name), Vector3.zero, Quaternion.identity);
                    GameObject t = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", tyrantPrefab.name), Vector3.zero, Quaternion.identity);
                    GameObject h = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", helldogPrefab.name), Vector3.zero, Quaternion.identity);
                    //GameObject z = Instantiate(zombiePrefab, Vector3.zero, Quaternion.identity);
                    //_zombieList.Add(z.GetComponent<Undead>());
                    //z.transform.parent = transform;
                    //z.SetActive(false);
                }

            }



            //if (GameManager.instance.gameType == GameManager.GameType.Survival)
            maxWave = 999999;

            foreach (HealthPack h in FindObjectsOfType<HealthPack>())
                healthPacks.Add(h);

            OnWaveStart -= SpawnAIs_Delegate;
            OnWaveStart += SpawnAIs_Delegate;
            OnAiDeath -= OnAiDeath_Delegate;
            OnAiDeath += OnAiDeath_Delegate;
            OnWaveEnd -= OnWaveEnd_Delegate;
            OnWaveEnd += OnWaveEnd_Delegate;

            if (PhotonNetwork.IsMasterClient)
                CreateAIPool();
        }
        else // We are in the menu
        {
            maxWave = 0;
            return;
        }

        //Begin();
    }

    public void CreateAIPool(bool caller = true)
    {
        if (!caller)
        {
            Debug.Log("Creating AI Pool");
            foreach (Undead z in FindObjectsOfType<Undead>().ToList())
            {
                //z.transform.position = new Vector3(0, -10, 0);
                _zombieList.Add(z);
                z.transform.parent = transform;
                z.gameObject.SetActive(false);
            }
            //_zombieList = FindObjectsOfType<Undead>();
            //foreach (Undead w in _zombieList)
            //    w.gameObject.SetActive(false);

            // Watcher GameObject must be active in order to be found with FindObjectsOfType
            foreach (AlienShooter w in FindObjectsOfType<AlienShooter>().ToList())
            {
                _watcherPool.Add(w);
                w.transform.parent = transform;
                w.gameObject.SetActive(false);
            }

            foreach (Breather w in FindObjectsOfType<Breather>(true).ToList())
            {
                _knightPool.Add(w);
                w.transform.parent = transform;
                w.gameObject.SetActive(false);
            }

            foreach (Helldog w in FindObjectsOfType<Helldog>(true).ToList())
            {
                hellhoundPool.Add(w);
                w.transform.parent = transform;
                w.gameObject.SetActive(false);
            }


            foreach (FlameTyrant w in FindObjectsOfType<FlameTyrant>(true).ToList())
            {
                tyrantPool.Add(w);
                w.transform.parent = transform;
                w.gameObject.SetActive(false);
            }
        }
        else
            PV.RPC("CreateAIPool", RpcTarget.All);
    }

    void Begin()
    {

        _maxBreathersEnabled = 2 + FindObjectsOfType<Player>().Count();
        _maxAliensEnabled = 1 + FindObjectsOfType<Player>().Count();
        _maxZombieEnabled = 3 + FindObjectsOfType<Player>().Count();
        if (editMode)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;
        Debug.Log("SWARM MANAGER: Begin");

        _networkSwarmManager.GetComponent<PhotonView>().RPC("IncreaseWave_RPC", RpcTarget.All);
    }

    public void IncreaseWave()
    {
        currentWave++;
        CalculateNumberOfAIsForNextWave();
        Debug.Log("Wave Increased");


        OnWaveIncrease?.Invoke(this);
        if (PhotonNetwork.IsMasterClient)
            _networkSwarmManager.GetComponent<PhotonView>().RPC("StartNewWave_RPC", RpcTarget.All);
    }

    void CalculateNumberOfAIsForNextWave()
    {
        if (currentWave % 5 != 0)
        {
            int nbPlayers = FindObjectsOfType<Player>().Length;
            if (nbPlayers <= 0)
                nbPlayers = 1;

            zombiesLeft = (nbPlayers * currentWave) + (int)Mathf.Floor((currentWave * 3));
            if (zombiesLeft > _zombieList.Count)
                zombiesLeft = _zombieList.Count;

            Debug.Log($"SwarmManager CalculateNumberOfAIsForNextWave");


            knightsLeft = nbPlayers * 3 + (int)(currentWave * 1.7f);
            if (knightsLeft > knightPool.Count)
                knightsLeft = knightPool.Count;

            watchersLeft = nbPlayers * 2 + (int)(currentWave * 1.2f);
            if (watchersLeft > watcherPool.Count)
                watchersLeft = knightPool.Count;

            //hellhoundsLeft = FindObjectsOfType<Player>().Length + (currentWave * 3);
            //if (hellhoundsLeft > hellhoundPool.Length)
            //    hellhoundsLeft = hellhoundPool.Length;
        }
        else
        {
            tyrantsLeft = 2;
            if (tyrantsLeft > tyrantPool.Count)
                tyrantsLeft = tyrantPool.Count;
            tyrantsLeft = 1;
        }


        if (editMode)
        {
            zombiesLeft = 0;
            knightsLeft = 0;
            hellhoundsLeft = 0;
            watchersLeft = 1;
            tyrantsLeft = 0;
        }

        OnAIsCalculated?.Invoke(this);
    }

    public void StartNewWave()
    {
        Debug.Log($"StartNewWave_RPC");
        StartCoroutine(StartNewWave_Coroutine());
    }
    IEnumerator StartNewWave_Coroutine()
    {
        int delay = FindObjectsOfType<Player>().Length * 3;
        yield return new WaitForSeconds(delay);
        try
        {
            GetComponent<AudioSource>().clip = _ambiantMusic;
            GetComponent<AudioSource>().Play();

        }
        catch (System.Exception ex)
        {

        }

        OnWaveStart?.Invoke(this);
    }

    void SpawnAIs_Delegate(SwarmManager swarmManager)
    {
        if (currentWave > 1)
            GameManager.GetMyPlayer().announcer.AddClip(_waveStartClip);
        if (!PhotonNetwork.IsMasterClient)
            return;
        Debug.Log("Spawning Ais");
        SpawnAi(AiType.Undead);
        SpawnAi(AiType.AlienShooter);
        SpawnAi(AiType.Breather);
        SpawnAi(AiType.Helldog);
        SpawnAi(AiType.FlameTyrant);
    }
    public void SpawnAi(AiType aiType, Transform transform = null)
    {
        //Debug.Log($"Spawning type of ai: {aiType}");
        if (!PhotonNetwork.IsMasterClient)
            return;

        int targetPhotonId = 0;
        try
        {
            targetPhotonId = GetRandomPlayerPhotonId();
        }
        catch { targetPhotonId = 0; }
        int pdelay = -1;

        int aiPhotonId = -1;
        Transform spawnPoint = SpawnManager.spawnManagerInstance.GetRandomComputerSpawnPoint();
        if (transform)
        {
            spawnPoint = transform;
            pdelay = 0;
        }

        if (aiType == AiType.Undead)
        {
            if (zombiesLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            int breathersEnabled = FindObjectsOfType<Undead>().Count();
            if (breathersEnabled >= _maxZombieEnabled)
            {
                StartCoroutine(SpawnAISkip_Coroutine(aiType));
                return;
            }

            foreach (Undead w in _zombieList)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Undead.ToString(), pdelay);
        }
        else if (aiType == AiType.AlienShooter)
        {
            if (watchersLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            int breathersEnabled = FindObjectsOfType<AlienShooter>().Count();
            if (breathersEnabled >= _maxAliensEnabled)
            {
                StartCoroutine(SpawnAISkip_Coroutine(aiType));
                return;
            }

            foreach (AlienShooter w in watcherPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.AlienShooter.ToString(), pdelay);
        }
        else if (aiType == AiType.Breather)
        {
            if (knightsLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            int breathersEnabled = FindObjectsOfType<Breather>().Count();
            if (breathersEnabled >= _maxBreathersEnabled)
            {
                StartCoroutine(SpawnAISkip_Coroutine(aiType));
                return;
            }

            foreach (Breather w in knightPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Breather.ToString(), pdelay);
        }
        else if (aiType == AiType.Helldog)
        {
            if (hellhoundsLeft <= 0 && pdelay < 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            foreach (Helldog w in hellhoundPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Helldog.ToString(), pdelay);
        }
        else if (aiType == AiType.FlameTyrant)
        {
            if (tyrantsLeft <= 0)
            {
                foreach (Helldog h in FindObjectsOfType<Helldog>())
                    h.Damage(999, 0);
                OnAiDeath?.Invoke(this);
                return;
            }

            foreach (FlameTyrant w in tyrantPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.FlameTyrant.ToString(), pdelay);
        }
    }

    // https://docs.microsoft.com/en-us/dotnet/api/system.func-2?view=net-6.0
    // https://docs.microsoft.com/en-us/dotnet/api/system.action-1?view=net-6.0
    public void SpawnAi(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation, string aiType, int pdelay = -1)
    {
        //Debug.Log($"SpawnAi_RPC. AI pdi: {aiPhotonId}");
        StartCoroutine(SpawnAI_Coroutine(aiPhotonId, targetPhotonId, spawnPointPosition, spawnPointRotation, aiType, pdelay));
    }
    IEnumerator SpawnAI_Coroutine(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation, string aiType, int pdelay = -1)
    {
        if (SceneManager.GetActiveScene().buildIndex <= 0)
        {
            Debug.LogWarning("Trying to spawn AI in Menu");
            yield return new WaitForSeconds(1);
        }

        //Debug.Log($"BEFORE DELAY. SpawnAI_Coroutine. AI pdi: {aiPhotonId}. AI type: {aiType}");
        AiType aiTypeEnum = (AiType)System.Enum.Parse(typeof(AiType), aiType);
        float delay = 10;
        float waveSpawnDelay = (currentWave / 5);

        if (aiTypeEnum == AiType.Undead)
        {
            delay = ZOMBIE_SPAWN_DELAY + waveSpawnDelay;
        }
        else if (aiTypeEnum == AiType.AlienShooter)
        {
            delay = SHOOTER_SPAWN_DELAY + waveSpawnDelay;
        }
        else if (aiTypeEnum == AiType.Breather)
        {
            delay = KNIGHT_SPAWN_DELAY + waveSpawnDelay;
        }
        else if (aiTypeEnum == AiType.Helldog)
        {
            delay = HELLHOUND_SPAWN_DELAY + waveSpawnDelay;
        }
        else if (aiTypeEnum == AiType.FlameTyrant)
        {
            delay = TYRANT_SPAWN_DELAY + waveSpawnDelay;
        }

        if (pdelay >= 0)
            delay = pdelay;

        yield return new WaitForSeconds(delay);

        Debug.Log($"AFTER DELAY. SpawnAI_Coroutine. AI pdi: {aiPhotonId}. AI type: {aiType}. {PhotonView.Find(aiPhotonId).name}");
        try
        {
            var newAiObj = PhotonView.Find(aiPhotonId).gameObject;
            newAiObj.GetComponent<Actor>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);

            if (pdelay < 0)
            {
                if (aiTypeEnum == AiType.Undead)
                    zombiesLeft--;
                else if (aiTypeEnum == AiType.AlienShooter)
                    watchersLeft--;
                else if (aiTypeEnum == AiType.Breather)
                    knightsLeft--;
                else if (aiTypeEnum == AiType.Helldog)
                    hellhoundsLeft--;
                else if (aiTypeEnum == AiType.FlameTyrant)
                    tyrantsLeft--;
                SpawnAi(aiTypeEnum);
            }

            OnAiSpawn?.Invoke(this);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR SpawnAI_Coroutine {e}");

            //if (aiTypeEnum == AiType.Undead)
            //{
            //    foreach (Undead w in _zombieList)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");
            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }
            //}
            //else if (aiTypeEnum == AiType.Watcher)
            //{
            //    foreach (Watcher w in watcherPool)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");

            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }

            //}
            //else if (aiTypeEnum == AiType.Breather)
            //{
            //    foreach (Breather w in knightPool)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");

            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }
            //}
            //else if (aiTypeEnum == AiType.Helldog)
            //{
            //    foreach (Helldog w in hellhoundPool)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");

            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }
            //}
            //else if (aiTypeEnum == AiType.FlameTyrant)
            //{
            //    foreach (FlameTyrant w in tyrantPool)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");

            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }
            //}

            //if (pdelay < 0)
            //{
            //    if (aiTypeEnum == AiType.Undead)
            //        zombiesLeft--;
            //    else if (aiTypeEnum == AiType.Watcher)
            //        watchersLeft--;
            //    else if (aiTypeEnum == AiType.Breather)
            //        knightsLeft--;
            //    else if (aiTypeEnum == AiType.Helldog)
            //        hellhoundsLeft--;
            //    else if (aiTypeEnum == AiType.FlameTyrant)
            //        tyrantsLeft--;
            //    SpawnAi(aiTypeEnum);
            //}

            //OnAiSpawn?.Invoke(this);
        }
    }


    IEnumerator SpawnAISkip_Coroutine(AiType aiType)
    {
        int delay = 0;

        if (aiType == AiType.Undead)
            delay = ZOMBIE_SPAWN_DELAY;
        else if (aiType == AiType.AlienShooter)
            delay = SHOOTER_SPAWN_DELAY;
        else if (aiType == AiType.Breather)
            delay = KNIGHT_SPAWN_DELAY;
        else if (aiType == AiType.Helldog)
            delay = HELLHOUND_SPAWN_DELAY;
        else if (aiType == AiType.FlameTyrant)
            delay = TYRANT_SPAWN_DELAY;

        yield return new WaitForSeconds(delay);

        SpawnAi(aiType);
    }
    void OnAiDeath_Delegate(SwarmManager swarmManager)
    {
        Debug.Log("OnAiDeath_Delegate");

        int __zombiesAlive = 0;
        int __watchersAlive = 0;
        int __knightsAlive = 0;
        int __hellhoundsAlive = 0;
        int __tyrantsAlive = 0;


        foreach (Undead w in _zombieList)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __zombiesAlive++;

        foreach (AlienShooter w in watcherPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __watchersAlive++;

        foreach (Breather w in knightPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __knightsAlive++;

        foreach (Helldog w in hellhoundPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __hellhoundsAlive++;

        foreach (FlameTyrant w in tyrantPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __tyrantsAlive++;

        zombiesAlive = __zombiesAlive;
        hellhoundsAlive = __hellhoundsAlive;
        watchersAlive = __watchersAlive;
        knightsAlive = __knightsAlive;
        tyrantsAlive = __tyrantsAlive;

        if (watchersLeft <= 0 && watchersAlive <= 0 && knightsLeft <= 0 && knightsAlive <= 0 && hellhoundsLeft <= 0 && hellhoundsAlive <= 0 && tyrantsLeft <= 0 && tyrantsAlive <= 0 && zombiesLeft <= 0 && zombiesAlive <= 0)
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Swarm Manager EndWave");
                _networkSwarmManager.GetComponent<PhotonView>().RPC("EndWave_RPC", RpcTarget.All);
            }
    }

    public void InvokeOnAiDeath() // Called multiple times on an ai death. TODO: Find independant solution
    {
        Debug.Log("Swarm Manager OnAiDeath");
        OnAiDeath?.Invoke(this);
    }

    public void EndWave()
    {
        Debug.Log("EndWave_RPC");
        nextWaveDelay = FindObjectsOfType<Player>().Length * 10;
        OnWaveEnd?.Invoke(this);
        _newWaveCountdown = nextWaveDelay;

        //Note: Avoid starting coroutine through RPCs. If bugs: a coroutine could be called multiple times. With countdowns, it is only reset instead of starting a coroutine alongside with the older one.
    }

    void OnWaveEnd_Delegate(SwarmManager swarmManager)
    {
        try
        {
            GetComponent<AudioSource>().clip = _waveSuccessClip;
            GetComponent<AudioSource>().Play();
        }
        catch (System.Exception ex)
        {

        }
        waveEnded = true;
        int ranBonusPoints = Random.Range(currentWave * 500, currentWave * 1000 + 1);
        foreach (Player p in FindObjectsOfType<Player>())
            p.GetComponent<PlayerSwarmMatchStats>().AddPoints(ranBonusPoints, true);
        RespawnHealthPacks_MasterCall();
    }

    void RespawnHealthPacks_MasterCall()
    {
        if (PhotonNetwork.IsMasterClient)
            _networkSwarmManager.GetComponent<PhotonView>().RPC("RespawnHealthPacks_RPC", RpcTarget.All);
    }

    public void RespawnHealthPacks()
    {
        Debug.Log("Respawn Health Packs RPC");
        if (currentWave % 10 == 0)
        {
            EndGame();
        }
        else if (currentWave % 5 == 0)
        {
            int livesToAdd = FindObjectsOfType<Player>().Length;
            livesLeft += livesToAdd;
            foreach (KillFeedManager p in FindObjectsOfType<KillFeedManager>())
            {
                p.EnterNewFeed($"Lives added: {livesToAdd}");
                p.EnterNewFeed("Health Packs Respawned");
            }
            foreach (HealthPack hp in healthPacks)
                if (!hp.gameObject.activeSelf)
                    hp.gameObject.SetActive(true);

            FindObjectOfType<NetworkSwarmManager>().EnableStartingNetworkWeapons();

            try
            {
                WebManager.webManagerInstance.SaveSwarmStats(GameManager.GetMyPlayer().GetComponent<PlayerSwarmMatchStats>());
            }
            catch (System.Exception e) { Debug.LogException(e); }
        }
    }
    int GetRandomPlayerPhotonId()
    {
        List<Player> allPlayers = FindObjectsOfType<Player>().ToList();
        List<Player> allAlivePlayers = new List<Player>();
        foreach (Player p in allPlayers)
            if (!p.isDead && !p.isRespawning)
                allAlivePlayers.Add(p);

        int ran = Random.Range(0, allAlivePlayers.Count);
        int targetPhotonId = allAlivePlayers[ran].PV.ViewID;
        return targetPhotonId;
    }

    public Transform GetRandomPlayerTransform()
    {
        return PhotonView.Find(GetRandomPlayerPhotonId()).transform;
    }

    public void RespawnHealthPack_MasterCall(Vector3 hpPosition, int time)
    {
        _networkSwarmManager.GetComponent<PhotonView>().RPC("RespawnHealthPack_RPC", RpcTarget.All, hpPosition, time);
    }

    public void RespawnHealthPack(Vector3 hpPosition, int time)
    {
        StartCoroutine(RespawnHealthPack_Coroutine(hpPosition, time));
    }

    IEnumerator RespawnHealthPack_Coroutine(Vector3 hpPosition, int time)
    {
        yield return new WaitForSeconds(time);

        foreach (HealthPack hp in healthPacks)
            if (hp.transform.position == hpPosition)
                hp.gameObject.SetActive(true);
    }

    public void DisableHealthPack_MasterCall(Vector3 hpPosition)
    {
        _networkSwarmManager.GetComponent<PhotonView>().RPC("DisableHealthPack_RPC", RpcTarget.All, hpPosition);
    }

    public void DisableHealthPack(Vector3 hpPosition)
    {
        foreach (HealthPack hp in healthPacks)
            if (hp.transform.position == hpPosition)
                hp.gameObject.SetActive(false);
    }

    public void DropRandomLoot(Vector3 position, Quaternion rotation)
    {
        int chanceToDrop = UnityEngine.Random.Range(0, 30);
        string ammoType = "";

        if (chanceToDrop == 0)
            ammoType = "power";
        else if (chanceToDrop == 1)
            ammoType = "grenade";
        else if (chanceToDrop == 2 || chanceToDrop == 3)
            ammoType = "heavy";
        else if (chanceToDrop >= 4 && chanceToDrop <= 6)
            ammoType = "small";


        if (!PhotonNetwork.IsMasterClient)
            return;
        _networkSwarmManager.GetComponent<PhotonView>().RPC("DropRandomLoot_RPC", RpcTarget.All, ammoType, position, rotation);
    }

    public void DropRandomLoot(string ammotype, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"{name} spawned random loot {ammotype}");
        GameObject loot = GameManager.instance.lightAmmoPack.gameObject;
        Quaternion rotFix = new Quaternion(0, 0, 0, 0);
        rotFix.eulerAngles = new Vector3(0, 180, 0);

        if (ammotype == "power")
            loot = Instantiate(GameManager.instance.powerAmmoPack.gameObject, position, rotation * rotFix);
        else if (ammotype == "heavy")
            loot = Instantiate(GameManager.instance.heavyAmmoPack.gameObject, position, rotation * rotFix);
        else if (ammotype == "small")
            loot = Instantiate(GameManager.instance.lightAmmoPack.gameObject, position, rotation * rotFix);
        else if (ammotype == "grenade")
            loot = Instantiate(GameManager.instance.grenadeAmmoPack.gameObject, position, rotation * rotFix);
        else
            return;

        Destroy(loot, 60);
    }

    public void EndGame(bool saveXp = true)
    {
        foreach (Player pp in FindObjectsOfType<Player>())
        {
            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine)
                continue;


            if (saveXp)
            {
                pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER!");
                WebManager.webManagerInstance.SaveSwarmStats(pp.GetComponent<PlayerSwarmMatchStats>());
                pp.LeaveRoomWithDelay();
            }
            else
                GameManager.instance.LeaveRoom();
        }
    }
}
