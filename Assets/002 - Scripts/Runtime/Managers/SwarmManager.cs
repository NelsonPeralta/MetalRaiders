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
    public enum AiType { Undead, AlienShooter, Knight, Hellhound, Tyrant }

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

    public List<Undead> zombieList { get { return _zombieList; } set { _zombieList = value; } }
    public List<Knight> knightPool { get { return _knightPool; } set { _knightPool = value; } }
    public List<SimpleAlienShooter> watcherPool { get { return _watcherPool; } set { _watcherPool = value; } }


    [Header("AI Pools")]
    List<Undead> _zombieList = new List<Undead>();
    public List<Knight> _knightPool = new List<Knight>();
    public List<SimpleAlienShooter> _watcherPool = new List<SimpleAlienShooter>();
    public Hellhound[] hellhoundPool;
    public Tyrant[] tyrantPool;

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
    int _tyrantsLeft;

    int _zombiesAlive;
    int _watchersAlive;
    int _knightsAlive;
    int _hellhoundsAlive;
    int _tyrantsAlive;

    public List<HealthPack> healthPacks = new List<HealthPack>();

    [SerializeField] AudioClip _ambiantMusic;
    [SerializeField] AudioClip _waveStartClip;
    [SerializeField] AudioClip _weaponDropClip;
    [SerializeField] AudioClip _livesAddedClip;
    [SerializeField] AudioClip _waveSuccessClip;


    // constants
    const int ZOMBIE_SPAWN_DELAY = 2;
    const int SHOOTER_SPAWN_DELAY = 7;
    const int KNIGHT_SPAWN_DELAY = 6;
    const int HELLHOUND_SPAWN_DELAY = 5;
    const int TYRANT_SPAWN_DELAY = 20;

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
        currentWave = 0;
        nextWaveDelay = 5;
    }
    private void Awake()
    {

        currentWave = 0;
        nextWaveDelay = 5;
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

    void OnAllPlayersJoinedRoom_Delegate(GameManagerEvents gme)
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            Begin();
    }

    void NewWaveCountdown()
    {
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

        currentWave = 0;
        nextWaveDelay = 5;

        if (editMode)
            currentWave = 9;

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
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
                    //GameObject w = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", knightPrefab.name), Vector3.zero, Quaternion.identity);
                    //GameObject a = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", alienShooterPrefab.name), Vector3.zero, Quaternion.identity);
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

            CreateAIPool();
        }
        else // We are in the menu
        {
            maxWave = 0;
            return;
        }

        //Begin();
    }

    void CreateAIPool()
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
        foreach (SimpleAlienShooter w in FindObjectsOfType<SimpleAlienShooter>().ToList())
        {
            _watcherPool.Add(w);
            w.transform.parent = transform;
            w.gameObject.SetActive(false);
        }

        foreach (Knight w in FindObjectsOfType<Knight>(true).ToList())
        {
            _knightPool.Add(w);
            w.transform.parent = transform;
            w.gameObject.SetActive(false);
        }

        hellhoundPool = FindObjectsOfType<Hellhound>();
        foreach (Hellhound w in hellhoundPool)
            w.gameObject.SetActive(false);

        tyrantPool = FindObjectsOfType<Tyrant>();
        foreach (Tyrant w in tyrantPool)
            w.gameObject.SetActive(false);
    }

    void Begin()
    {
        if (editMode)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;
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
        //if (currentWave % 5 != 0)
        {
            int nbPlayers = FindObjectsOfType<Player>().Length;
            if (nbPlayers <= 0)
                nbPlayers = 1;

            zombiesLeft = (nbPlayers * currentWave) + (int)Mathf.Floor((currentWave * 2));
            if (zombiesLeft > _zombieList.Count)
                zombiesLeft = _zombieList.Count;

            Debug.Log($"SwarmManager CalculateNumberOfAIsForNextWave");


            knightsLeft = nbPlayers * 2 + (currentWave);
            if (knightsLeft > knightPool.Count)
                knightsLeft = knightPool.Count;

            watchersLeft = nbPlayers * 3 + (currentWave * 2);
            if (watchersLeft > watcherPool.Count)
                watchersLeft = knightPool.Count;

            //hellhoundsLeft = FindObjectsOfType<Player>().Length + (currentWave * 3);
            //if (hellhoundsLeft > hellhoundPool.Length)
            //    hellhoundsLeft = hellhoundPool.Length;
        }
        //else
        //{
        //    tyrantsLeft = FindObjectsOfType<Player>().Length * 2;
        //    if (tyrantsLeft > tyrantPool.Length)
        //        tyrantsLeft = tyrantPool.Length;
        //}


        //if (editMode)
        {
            zombiesLeft = 1;
            knightsLeft = 0;
            hellhoundsLeft = 0;
            watchersLeft = 0;
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
        SpawnAi(AiType.Knight);
        //SpawnAi(AiType.Hellhound);
        //SpawnAi(AiType.Tyrant);
    }
    public void SpawnAi(AiType aiType, Transform transform = null)
    {
        //Debug.Log($"Spawning type of ai: {aiType}");
        if (!PhotonNetwork.IsMasterClient)
            return;

        int targetPhotonId = GetRandomPlayerPhotonId();
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

            foreach (SimpleAlienShooter w in watcherPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.AlienShooter.ToString(), pdelay);
        }
        else if (aiType == AiType.Knight)
        {
            if (knightsLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            foreach (Knight w in knightPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Knight.ToString(), pdelay);
        }
        else if (aiType == AiType.Hellhound)
        {
            if (hellhoundsLeft <= 0 && pdelay < 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            foreach (Hellhound w in hellhoundPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Hellhound.ToString(), pdelay);
        }
        else if (aiType == AiType.Tyrant)
        {
            if (tyrantsLeft <= 0)
            {
                foreach (Hellhound h in FindObjectsOfType<Hellhound>())
                    h.Damage(999, 0);
                OnAiDeath?.Invoke(this);
                return;
            }

            foreach (Tyrant w in tyrantPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            FindObjectOfType<NetworkSwarmManager>().GetComponent<PhotonView>().RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Tyrant.ToString(), pdelay);
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
        else if (aiTypeEnum == AiType.Knight)
        {
            delay = KNIGHT_SPAWN_DELAY + waveSpawnDelay;
        }
        else if (aiTypeEnum == AiType.Hellhound)
        {
            delay = HELLHOUND_SPAWN_DELAY + waveSpawnDelay;
        }
        else if (aiTypeEnum == AiType.Tyrant)
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
                else if (aiTypeEnum == AiType.Knight)
                    knightsLeft--;
                else if (aiTypeEnum == AiType.Hellhound)
                    hellhoundsLeft--;
                else if (aiTypeEnum == AiType.Tyrant)
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
            //else if (aiTypeEnum == AiType.Knight)
            //{
            //    foreach (Knight w in knightPool)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");

            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }
            //}
            //else if (aiTypeEnum == AiType.Hellhound)
            //{
            //    foreach (Hellhound w in hellhoundPool)
            //    {
            //        Debug.Log($"{aiTypeEnum} Pid: {w.GetComponent<PhotonView>().ViewID}");
            //        if (w.GetComponent<PhotonView>().ViewID == aiPhotonId)
            //        {
            //            Debug.Log("Found");

            //            w.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
            //        }
            //    }
            //}
            //else if (aiTypeEnum == AiType.Tyrant)
            //{
            //    foreach (Tyrant w in tyrantPool)
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
            //    else if (aiTypeEnum == AiType.Knight)
            //        knightsLeft--;
            //    else if (aiTypeEnum == AiType.Hellhound)
            //        hellhoundsLeft--;
            //    else if (aiTypeEnum == AiType.Tyrant)
            //        tyrantsLeft--;
            //    SpawnAi(aiTypeEnum);
            //}

            //OnAiSpawn?.Invoke(this);
        }
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

        foreach (SimpleAlienShooter w in watcherPool)
            if (w.gameObject.activeSelf && !w.isDead)
                __watchersAlive++;

        foreach (Knight w in knightPool)
            if (w.gameObject.activeSelf && !w.isDead)
                __knightsAlive++;

        foreach (Hellhound w in hellhoundPool)
            if (w.gameObject.activeSelf && !w.isDead)
                __hellhoundsAlive++;

        foreach (Tyrant w in tyrantPool)
            if (w.gameObject.activeSelf && !w.isDead)
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

    public void Invoke_OnAiDeath() // Called multiple times on an ai death. TODO: Find independant solution
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
