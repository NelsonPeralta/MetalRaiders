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
    public static SwarmManager instance { get { return _instance; } }
    static SwarmManager _instance;


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
    public int ranClipInt { get { return _ranClipInt; } set { _ranClipInt = value; print($"ranclip set to {value}"); } }


    [SerializeField] GameObject zombiePrefab;
    [SerializeField] GameObject knightPrefab;
    [SerializeField] GameObject alienShooterPrefab;
    [SerializeField] GameObject tyrantPrefab;
    [SerializeField] GameObject helldogPrefab;
    int _ranClipInt;

    public List<Undead> zombieList { get { return _zombiesPool; } set { _zombiesPool = value; } }
    public List<Breather> breathersPool { get { return _breathersPool; } set { _breathersPool = value; } }
    public List<AlienShooter> ribbiansPool { get { return _ribbianPool; } set { _ribbianPool = value; } }


    [Header("AI Pools")]
    List<Undead> _zombiesPool = new List<Undead>();
    public List<Breather> _breathersPool = new List<Breather>();
    public List<AlienShooter> _ribbianPool = new List<AlienShooter>();
    public List<Helldog> hellhoundPool = new List<Helldog>();
    public List<Tyrant> tyrantPool = new List<Tyrant>();

    // private variables
    PhotonView networkSwarmManagerPV;
    int maxWave;
    int _livesLeft = 4;
    float _newWaveCountdown, _waveEndCountdown;
    bool _waveEnded;


    int _zombiesLeft, _watchersLeft, _knightsLeft, _hellhoundsLeft, _tyrantsLeft;

    int _zombiesAlive, _watchersAlive, _knightsAlive, _hellhoundsAlive, _tyrantsAlive;

    int _maxAliensEnabled;
    int _maxBreathersEnabled;
    int _maxZombieEnabled;

    public List<HealthPack> healthPacks = new List<HealthPack>();

    public AudioClip[] bossMusicsIntros, bossMusicLoops, bossMusicOutros;

    [SerializeField] AudioSource _musicAudioSource;
    [SerializeField] List<AudioClip> _openingClips;
    [SerializeField] List<AudioClip> _ambiantClips;
    [SerializeField] AudioClip _waveStartClip;
    [SerializeField] AudioClip _weaponDropClip;
    [SerializeField] AudioClip _livesAddedClip;
    [SerializeField] AudioClip _waveSuccessClip;
    [SerializeField] List<ActorDropship> _actorDropships = new List<ActorDropship>();


    public float globalActorGrenadeCooldown;

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
    public int ribbiansLeft
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

    public int breathersLeft
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

    public int ribbiansAlive
    {
        get { return _watchersAlive; }
        private set { _watchersAlive = value; }
    }

    public int breathersAlive
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

    protected List<AudioClip> clips
    {
        get { return _clips; }
        set
        {
            Debug.Log(_clips.Count);
            int preCount = _clips.Count - 1;
            Debug.Log(preCount);
            _clips = value;

            if (_clips.Count > 0)
            {
                try
                {
                    StartCoroutine(PlayClip_Coroutine(_clipTimeRemaining, _clips.Last()));
                    _clipTimeRemaining += _clips.Last().length + 0;
                    StartCoroutine(RemoveClip_Coroutine(_clipTimeRemaining * 1, _clips.Last()));
                }
                catch { }

            }
        }
    }

    public float TimeSinceEnemiesDropped { get { return _timeSinceEnemiesDroped; } }

    List<AudioClip> _clips = new List<AudioClip>();

    float _clipTimeRemaining;

    List<int> _reservedActorPhotonIdsForWave = new List<int>();



    float _timeSinceEnemiesDroped, _checkForOutOflives;
    public bool gameWon;


    public List<Biped> actorsAliveList = new List<Biped>();


    private void OnEnable()
    {
        //currentWave = 0;
        nextWaveDelay = 5;
    }
    private void Awake()
    {
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
        GameManager.instance.OnGameManagerFinishedLoadingScene_Late -= OnSceneLoaded;
        GameManager.instance.OnGameManagerFinishedLoadingScene_Late += OnSceneLoaded;
    }

    NetworkSwarmManager _networkSwarmManager;

    private void Update()
    {
        if (_checkForOutOflives > 0)
        {
            _checkForOutOflives -= Time.deltaTime;
            if (_checkForOutOflives < 0)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (_livesLeft <= 0)
                        EndGame();
                    else
                        _checkForOutOflives = 4;
                }
            }
        }

        if (globalActorGrenadeCooldown > 0) globalActorGrenadeCooldown -= Time.deltaTime;

        if (!waveEnded)
        {
            _timeSinceEnemiesDroped += Time.deltaTime;
        }


        if (_clipTimeRemaining > 0)
        {
            _clipTimeRemaining -= Time.deltaTime;
            if (_clipTimeRemaining <= 0)
                _clipTimeRemaining = 0;
        }

        NewWaveCountdown();
    }

    public void OnAllPlayersJoinedRoom_Delegate(CurrentRoomManager gme)
    {

    }

    void NewWaveCountdown()
    {
        if (GameManager.instance.gameMode != GameManager.GameMode.Coop)
            return;
        if (_newWaveCountdown > 0)
        {
            _newWaveCountdown -= Time.deltaTime;

            if (_newWaveCountdown <= 0)
                IncreaseWave();
        }

        if (_waveEndCountdown > 0)
        {
            _waveEndCountdown -= Time.deltaTime;

            if (_waveEndCountdown <= 0)
            {
                if (!CurrentRoomManager.instance.gameOver)
                    if (PhotonNetwork.IsMasterClient) _networkSwarmManager.GetComponent<PhotonView>().RPC("EndWave_RPC", RpcTarget.All);

                //EndWave();
            }
        }
    }



    void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        currentWave = 0;
        gameWon = false;
        nextWaveDelay = 5;
        _checkForOutOflives = 4;

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            Debug.Log("SWARM MANAGER: OnSceneLoaded");
            if (GameManager.instance.gameMode != GameManager.GameMode.Coop)
                return;

            _instance = this;
            if (PhotonNetwork.IsMasterClient)
            {
                _networkSwarmManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkSwarmManager"), Vector3.zero, Quaternion.identity).GetComponent<NetworkSwarmManager>();
                networkSwarmManagerPV = _networkSwarmManager.GetComponent<PhotonView>();

                for (int i = 0; i < 32; i++)
                {
                    //Transform sp = SpawnManager.spawnManagerInstance.GetRandomComputerSpawnPoint();
                    //GameObject z = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", zombiePrefab.name), Vector3.zero, Quaternion.identity);
                    //_zombiesPool.Add(z.GetComponent<Undead>());
                    //z.transform.parent = transform;
                    //z.gameObject.SetActive(false);

                    GameObject w = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", knightPrefab.name), Vector3.zero + new Vector3(0, -i * 10, -1), Quaternion.identity);
                    //_breathersPool.Add(w.GetComponent<Breather>());
                    //w.transform.parent = transform;
                    //w.gameObject.SetActive(false);

                    GameObject a = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", alienShooterPrefab.name), Vector3.zero + new Vector3(0, -i * 10, -2), Quaternion.identity);
                    //_ribbianPool.Add(a.GetComponent<AlienShooter>());
                    //a.transform.parent = transform;
                    //a.gameObject.SetActive(false);


                    //GameObject t = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", helldogPrefab.name), Vector3.zero + new Vector3(0, -i * 10, -3), Quaternion.identity);
                    //hellhoundPool.Add(t.GetComponent<Helldog>());
                    //t.transform.parent = transform;
                    //t.gameObject.SetActive(false);

                    //GameObject h = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", tyrantPrefab.name), Vector3.zero + new Vector3(0, -i * 10, -4), Quaternion.identity);
                    //tyrantPool.Add(h.GetComponent<Tyrant>());
                    //h.transform.parent = transform;
                    //h.gameObject.SetActive(false);
                    //GameObject z = Instantiate(zombiePrefab, Vector3.zero, Quaternion.identity);
                    //_zombieList.Add(z.GetComponent<Undead>());
                    //z.transform.parent = transform;
                    //z.SetActive(false);
                }

            }

            _actorDropships = FindObjectsOfType<ActorDropship>().ToList();
            foreach (ActorDropship a in _actorDropships) a.gameObject.SetActive(false);



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

            //if (PhotonNetwork.IsMasterClient)
            //    CreateAIPool();
        }
        else // We are in the menu
        {
            breathersPool.Clear(); ribbiansPool.Clear(); hellhoundPool.Clear(); tyrantPool.Clear(); healthPacks.Clear();
            _breathersPool.Clear(); _ribbianPool.Clear();
            _reservedActorPhotonIdsForWave.Clear();
            actorsAliveList.Clear();
            _actorDropships.Clear();

            maxWave = 0;
            return;
        }

        //Begin();
    }

    public void PlayOpeningMusic()
    {
        ranClipInt = Random.Range(0, _openingClips.Count);
        _musicAudioSource.clip = _openingClips[ranClipInt];
        _musicAudioSource.Play();
    }

    public void CreateAIPool(bool caller = true)
    {
        if (!caller)
        {
            Debug.Log("Creating AI Pool");
        }
        else
            networkSwarmManagerPV.RPC("CreateAIPool", RpcTarget.All);
    }

    public void Begin()
    {

        _maxBreathersEnabled = 6;
        _maxAliensEnabled = 6;
        _maxZombieEnabled = 3;
        _maxBreathersEnabled = 6;
        //if (editMode)
        //    return;

        if (!PhotonNetwork.IsMasterClient)
            return;
        Debug.Log("SWARM MANAGER: Begin");

        if (PhotonNetwork.IsMasterClient) _networkSwarmManager.GetComponent<PhotonView>().RPC("IncreaseWave_RPC", RpcTarget.All);
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
            //int nbPlayers = FindObjectsOfType<Player>().Length;
            //if (nbPlayers <= 0)
            //    nbPlayers = 1;

            zombiesLeft = (currentWave) + (int)Mathf.Floor((currentWave * 2.5f));
            if (zombiesLeft > _zombiesPool.Count)
                zombiesLeft = _zombiesPool.Count;

            Debug.Log($"SwarmManager CalculateNumberOfAIsForNextWave");


            breathersLeft = 3 + (int)(currentWave * 1.4f);
            if (breathersLeft > breathersPool.Count)
                breathersLeft = breathersPool.Count;


            ribbiansLeft = 2 + (int)(currentWave * 1.1f);
            if (ribbiansLeft > ribbiansPool.Count)
                ribbiansLeft = ribbiansPool.Count;

            //zombiesLeft = breathersLeft = ribbiansLeft = 0;
            zombiesLeft = 0;

            //hellhoundsLeft = FindObjectsOfType<Player>().Length + (currentWave * 3);
            //if (hellhoundsLeft > hellhoundPool.Length)
            //    hellhoundsLeft = hellhoundPool.Length;
        }
        else
        {
            //tyrantsLeft = 1;
            //if (tyrantsLeft > tyrantPool.Count)
            //    tyrantsLeft = 1;


            ribbiansLeft = 2 + (int)(currentWave * 1.1f);
            ribbiansLeft *= 2;
            if (ribbiansLeft > ribbiansPool.Count)
                ribbiansLeft = ribbiansPool.Count;
        }


        if (editMode)
        {
            //zombiesLeft = 0;
            //hellhoundsLeft = 0;
            //tyrantsLeft = 0;

            breathersLeft = 1;
            ribbiansLeft = 2;
        }

        OnAIsCalculated?.Invoke(this);
    }

    public void StartNewWave()
    {
        Debug.Log($"StartNewWave_RPC");
        if (!CurrentRoomManager.instance.gameOver)
            StartCoroutine(StartNewWave_Coroutine());
    }
    IEnumerator StartNewWave_Coroutine()
    {
        yield return new WaitForSeconds(0);

        //print($"StartNewWave_Coroutine {currentWave}");
        if (currentWave % 5 == 0)
        {
            ranClipInt = Random.Range(0, bossMusicsIntros.Length); if (bossMusicsIntros.Length == 1) ranClipInt = 0;
            print($"StartNewWave_Coroutine {currentWave} {ranClipInt} {bossMusicsIntros.Length}");
            AddClip(bossMusicsIntros[ranClipInt]);
            AddClip(bossMusicLoops[ranClipInt]);
        }
        try
        {





            //if (currentWave % 5 == 3)
            //{
            //    _ranClipInt = Random.Range(0, _ambiantClips.Count);
            //    _musicAudioSource.clip = _ambiantClips[_ranClipInt];
            //    _musicAudioSource.Play();
            //}
            //else if (currentWave % 5 == 0 && currentWave > 1)
            //{
            //    int ran = Random.Range(0, bossMusics.Count);

            //    AddClip(bossMusics[_ranClipInt].intro);
            //    AddClip(bossMusics[_ranClipInt].loop);
            //}
        }
        catch (System.Exception ex)
        {

        }

        OnWaveStart?.Invoke(this);
    }

    void SpawnAIs_Delegate(SwarmManager swarmManager)
    {
        if (currentWave > 1)
            GameManager.GetRootPlayer().announcer.AddClip(_waveStartClip);

        actorsAliveList.Clear();
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Spawning Ais");
            _reservedActorPhotonIdsForWave.Clear();

            for (int i = 0; i < _actorDropships.Count; i++)
            {
                StartCoroutine(EnableDropship_Coroutine(_actorDropships[i].gameObject, i + 1));
            }
        }

        for (int i = 0; i < _actorDropships.Count; i++)
        {
            StartCoroutine(EnableDropship_Coroutine(_actorDropships[i].gameObject, i + 1));
        }
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
        Transform spawnPoint;
        if (transform)
        {
            spawnPoint = transform;
            pdelay = 0;
        }
        else
        {
            spawnPoint = AISpawnManager.aISpawnManagerInstance.GetGenericSpawnpoint();
        }

        if (aiType == AiType.Undead)
        {
            if (zombiesLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }


            int zombiesEnabled = 0; foreach (Undead u in zombieList) if (u.gameObject.activeInHierarchy) zombiesEnabled++;

            if (zombiesEnabled >= _maxZombieEnabled)
            {
                if (!CurrentRoomManager.instance.gameOver)
                    StartCoroutine(SpawnAISkip_Coroutine(aiType));
                return;
            }

            foreach (Undead w in _zombiesPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Undead.ToString(), pdelay);
        }
        else if (aiType == AiType.AlienShooter)
        {
            if (ribbiansLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            int ribbiansEnabled = 0; foreach (AlienShooter u in ribbiansPool) if (u.gameObject.activeInHierarchy) ribbiansEnabled++;

            if (ribbiansEnabled >= _maxAliensEnabled)
            {
                StartCoroutine(SpawnAISkip_Coroutine(aiType));
                return;
            }

            foreach (AlienShooter w in ribbiansPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.AlienShooter.ToString(), pdelay);
        }
        else if (aiType == AiType.Breather)
        {
            if (breathersLeft <= 0)
            {
                OnAiDeath?.Invoke(this);
                return;
            }

            int breathersEnabled = 0; foreach (Breather u in breathersPool) if (u.gameObject.activeInHierarchy) breathersEnabled++;
            if (breathersEnabled >= _maxBreathersEnabled)
            {
                StartCoroutine(SpawnAISkip_Coroutine(aiType));
                return;
            }

            foreach (Breather w in breathersPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Breather.ToString(), pdelay);
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

            networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Helldog.ToString(), pdelay);
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

            foreach (Tyrant w in tyrantPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.FlameTyrant.ToString(), pdelay);
        }
    }

    // https://docs.microsoft.com/en-us/dotnet/api/system.func-2?view=net-6.0
    // https://docs.microsoft.com/en-us/dotnet/api/system.action-1?view=net-6.0
    public void SpawnAi(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation, string aiType, int pdelay = -1)
    {
        //Debug.Log($"SpawnAi_RPC. AI pdi: {aiPhotonId}");
        if (!CurrentRoomManager.instance.gameOver)
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

        if (currentWave % 10 == 0)
        {
            delay = 2;
        }

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
                    ribbiansLeft--;
                else if (aiTypeEnum == AiType.Breather)
                    breathersLeft--;
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

        yield return new WaitForSeconds(1);

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


        foreach (Undead w in _zombiesPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __zombiesAlive++;

        foreach (AlienShooter w in ribbiansPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __watchersAlive++;

        foreach (Breather w in breathersPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __knightsAlive++;

        try
        {
            foreach (Helldog w in hellhoundPool)
                if (w.gameObject.activeSelf && w.hitPoints > 0)
                    __hellhoundsAlive++;
        }
        catch { }

        foreach (Tyrant w in tyrantPool)
            if (w.gameObject.activeSelf && w.hitPoints > 0)
                __tyrantsAlive++;

        zombiesAlive = __zombiesAlive;
        hellhoundsAlive = __hellhoundsAlive;
        ribbiansAlive = __watchersAlive;
        breathersAlive = __knightsAlive;
        tyrantsAlive = __tyrantsAlive;

        if (ribbiansLeft <= 0 && ribbiansAlive <= 0 && breathersLeft <= 0 && breathersAlive <= 0 && hellhoundsLeft <= 0 && hellhoundsAlive <= 0 && tyrantsLeft <= 0 && tyrantsAlive <= 0 && zombiesLeft <= 0 && zombiesAlive <= 0)
            if (PhotonNetwork.IsMasterClient)
            {
                //Debug.Log("Swarm Manager EndWave");
                //_waveEndCountdown = nextWaveDelay;
                //_networkSwarmManager.GetComponent<PhotonView>().RPC("EndWave_RPC", RpcTarget.All);
            }

        if (ribbiansAlive <= 0 && breathersAlive <= 0)
        {
            Debug.Log("Swarm Manager EndWave");
            _waveEndCountdown = nextWaveDelay;
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
        print($"EndWave_RPC {currentWave} {ranClipInt}");
        if (currentWave % 5 == 0 && currentWave > 1) AddClip(bossMusicOutros[ranClipInt]);

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
        foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
            if (p)
                p.GetComponent<PlayerSwarmMatchStats>().AddPoints(ranBonusPoints, true);
        RespawnHealthPacksCheck();
    }

    void RespawnHealthPacks_MasterCall()
    {
        if (PhotonNetwork.IsMasterClient)
            _networkSwarmManager.GetComponent<PhotonView>().RPC("RespawnHealthPacks_RPC", RpcTarget.All);
    }

    public void RespawnHealthPacksCheck()
    {
        if (currentWave % 10 == 0 && GameManager.instance.gameType == GameManager.GameType.Survival)
        {
            bool _achievementUnlocked = false;
            Steamworks.SteamUserStats.GetAchievement("YAWA", out _achievementUnlocked);

            if (!_achievementUnlocked)
            {
                Debug.Log("Unlocked Achievement You and What Army");
                AchievementManager.UnlockAchievement("YAWA");
            }

            if (GameManager.instance.gameMode == GameManager.GameMode.Coop && GameManager.instance.gameType != GameManager.GameType.Endless)
            {
                gameWon = true;
                EndGame();
            }
        }
        else if (currentWave % 5 == 0)
        {
            NetworkSwarmManager.instance.EnableStartingNetworkWeapons();
            NetworkGameManager.instance.EnableGrenadePacks();

            int livesToAdd = GameManager.instance.GetAllPhotonPlayers().Count;
            livesLeft += livesToAdd;
            GameManager.GetRootPlayer().announcer.AddClip(_livesAddedClip);
            GameManager.GetRootPlayer().announcer.AddClip(_weaponDropClip);
            foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
            {
                if (p)
                {
                    p.killFeedManager.EnterNewFeed($"<color=#31cff9>Lives added: {livesToAdd}");
                    p.killFeedManager.EnterNewFeed("<color=#31cff9>Health Packs Respawned");
                    p.killFeedManager.EnterNewFeed("<color=#31cff9>Weapons respawned");
                }
            }
            foreach (HealthPack hp in healthPacks)
                if (!hp.gameObject.activeSelf)
                    hp.gameObject.SetActive(true);
        }
    }
    int GetRandomPlayerPhotonId()
    {
        List<Player> allPlayers = GameManager.instance.GetAllPhotonPlayers();
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
        if (PhotonNetwork.IsMasterClient) _networkSwarmManager.GetComponent<PhotonView>().RPC("RespawnHealthPack_RPC", RpcTarget.All, hpPosition, time);
    }

    public void RespawnHealthPack(Vector3 hpPosition, int time)
    {
        if (!CurrentRoomManager.instance.gameOver)
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
        if (PhotonNetwork.IsMasterClient) _networkSwarmManager.GetComponent<PhotonView>().RPC("DisableHealthPack_RPC", RpcTarget.All, hpPosition);
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
        if (PhotonNetwork.IsMasterClient) _networkSwarmManager.GetComponent<PhotonView>().RPC("DropRandomLoot_RPC", RpcTarget.All, ammoType, position, rotation);
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

    public void EndGame(bool saveXp = true, bool actuallyQuit = false)
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            SwarmManager.instance.StopAllMusic();


        CurrentRoomManager.instance.gameOver = true;
        StopAllCoroutines();
        foreach (Player pp in GameManager.instance.GetAllPhotonPlayers())
        {
            if (pp)
            {

                // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
                // return will stop this method, break will stop the loop, continue will stop the current iteration
                if (!pp.PV.IsMine)
                    continue;

                pp.playerUI.gamepadCursor.gameObject.SetActive(false);


                if (saveXp)
                {
                    pp.allPlayerScripts.announcer.PlayGameOverClip();
                    pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>Objective complete! Game Over.");

                    if (gameWon) WebManager.webManagerInstance.SaveSwarmStats(pp.GetComponent<PlayerSwarmMatchStats>(), gameWon);


                    pp.LeaveLevelButStayInRoom();
                }
                else
                {
                    GameManager.instance.LeaveCurrentRoomAndLoadLevelZero();
                }
            }
        }
    }





    public void AddClip(AudioClip ac)
    {
        List<AudioClip> c = clips;
        c.Add(ac);
        clips = c;
    }

    IEnumerator PlayClip_Coroutine(float delay, AudioClip clip)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            _musicAudioSource.clip = clip;
            _musicAudioSource.Play();
            _musicAudioSource.loop = bossMusicLoops.Contains(clip);
        }
        catch (System.Exception ex) { Debug.Log(ex); }

    }

    IEnumerator RemoveClip_Coroutine(float delay, AudioClip clip)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            clips.Remove(clip);
        }
        catch (System.Exception ex) { Debug.Log(ex); }
    }


    IEnumerator EnableDropship_Coroutine(GameObject go, int d = 1)
    {
        yield return new WaitForSeconds(d);
        go.SetActive(true);
    }

    public void SpawnActorsFromDropship(ActorDropship ad)
    {

        if (!PhotonNetwork.IsMasterClient)
            return;

        int targetPhotonId, aiPhotonId = -1;
        try { targetPhotonId = GetRandomPlayerPhotonId(); } catch { targetPhotonId = 0; }
        print($"SpawnActorsFromDropship: targetPhotonId {targetPhotonId}");


        foreach (SpawnPoint sp in ad.ribbianSpawnPoints)
        {
            foreach (AlienShooter w in ribbiansPool)
            {
                if (!w.gameObject.activeInHierarchy && !_reservedActorPhotonIdsForWave.Contains(w.GetComponent<PhotonView>().ViewID))
                {
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;
                    _reservedActorPhotonIdsForWave.Add(aiPhotonId);
                    break;
                }

            }

            networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId,
                sp.transform.position, sp.transform.rotation, AiType.AlienShooter.ToString(), 0);

            //if (editMode) break;
        }
        foreach (SpawnPoint s in ad.breathersSpawnPoints)
        {

            if (currentWave > 1 && currentWave % 5 == 0) // Spawn Ribbians instead
            {
                foreach (AlienShooter w in ribbiansPool)
                {
                    if (!w.gameObject.activeInHierarchy && !_reservedActorPhotonIdsForWave.Contains(w.GetComponent<PhotonView>().ViewID))
                    {
                        aiPhotonId = w.GetComponent<PhotonView>().ViewID;
                        _reservedActorPhotonIdsForWave.Add(aiPhotonId);
                        break;
                    }
                }

                networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId,
                    s.transform.position, s.transform.rotation, AiType.AlienShooter.ToString(), 0);

                //if (editMode) break;
            }
            else // Spawn breathers normally
            {
                foreach (Breather w in breathersPool)
                {
                    if (!w.gameObject.activeInHierarchy && !_reservedActorPhotonIdsForWave.Contains(w.GetComponent<PhotonView>().ViewID))
                    {
                        aiPhotonId = w.GetComponent<PhotonView>().ViewID;
                        _reservedActorPhotonIdsForWave.Add(aiPhotonId);
                        break;
                    }
                }

                networkSwarmManagerPV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId,
                    s.transform.position, s.transform.rotation, AiType.Breather.ToString(), 0);

                //if (editMode) break;
            }
        }




        _timeSinceEnemiesDroped = 0;

        //foreach (Player p in GameManager.instance.pid_player_Dict.Values)
        //    if (p && p.isMine)
        //        p.SetupMotionTracker();
    }





    public void StopAllMusic()
    {
        _musicAudioSource.Stop();
        _musicAudioSource.clip = null;
    }
}
