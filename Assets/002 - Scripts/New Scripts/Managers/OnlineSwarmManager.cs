using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class OnlineSwarmManager : MonoBehaviour
{
    public static OnlineSwarmManager onlineSwarmManagerInstance;
    public PhotonView PV;
    public bool editMode = true;
    //public bool gameIsPaused;
    public MyPlayerManager pManager;
    public SplitScreenManager ssManager;

    [Header("AI  Pools")]
    public GameObjectPool zombiePool;
    public GameObjectPool skeletonPool;
    public GameObjectPool watcherPool;
    public GameObjectPool trollPool;

    [Header("AI Mini-Bosses Pools")]
    public GameObjectPool hellhoundPool;

    [Header("AI Bosses Pools")]
    public GameObjectPool blackKnightPrefab;
    public GameObjectPool flameTyrantPrefab;
    //public GameObject dragon;    

    [Header("Players")]
    public List<PlayerProperties> allPlayers = new List<PlayerProperties>();
    public List<OnlinePlayerSwarmScript> allPlayerSwarmScripts = new List<OnlinePlayerSwarmScript>();
    public int playerLives;
    List<WaveCounter> allPlayerWaveCounters = new List<WaveCounter>();

    [Header("AI Spawns")]
    public GameObject[] ZombieSpawns;
    public GameObject[] skeletonSpawns;
    public GameObject[] watcherSpawns;
    public GameObject[] hellhoundSpawns;
    public GameObject[] trollSpawns;


    [Header("Spawns (NEEDS TO BE FILLED FOR PLAYER RESPAWNS TO WORK")]
    public GameObject[] GenericSpawns = new GameObject[10];

    bool spawningZombieInProgress = false;
    bool gameHasStarted = false;

    [Header("In-Game Information")]
    public int waveNumber = 0;
    public bool waveInProgress = false;
    public float newWaveDelay;
    public float bossWaveTimer;
    public int zombiesAlive;
    public int skeletonsAlive;
    public int watchersAlive;
    public int hellhoundsAlive;
    public int trollsAlive;

    [Header("AIs Left To Spawn")]
    public int zombiesLeftToSpawn;
    public int skeletonsLeftToSpawn;
    public int watchersLeftToSpawn;
    public int hellhoundsLeftToSpawn;
    public int trollsLeftToSpawn;

    int maxZombiesForRound;
    int maxSkeletonsForRound;
    int maxWatchersForRound;
    int maxHellhoundsForRound;
    int maxTrollsForRound;

    public bool isBossWave;
    int maxBlackKnightsForRound;
    int blackKnightsAlive;
    int maxFlameTyrantsForRound;
    int flameTyrantsAlive;


    [Header("AIs Open Space")]
    public int maxZombiesOnMap;
    public int maxSkeletonsOnMap;
    public int maxWatchersOnMap;
    public int maxHellhoundsOnMap;
    public int maxTrollsOnMap;
    bool hasSpaceToSpawnZombie;
    bool hasSpaceToSpawnSkeleton;
    bool hasSpaceToSpawnWatcher;
    bool hasSpaceToSpawnHellhound;
    bool hasSpaceToSpawnTroll;

    [Header("AI Spawn Times and Delays")]
    public float zombieSpawnDelay = 2f;
    public float nextZombieSpawnTime = 0f;
    public float skeletonSpawnDelay = 3f;
    public float nextSkeletonSpawnTime = 0f;
    public float watcherSpawnDelay = 6;
    public float nextWatcherSpawnTime = 0f;
    public float hellhoundSpawnDelay = 2;
    public float nextHellhoundSpawnTime = 0f;
    public float trollSpawnDelay = 5;
    public float nextTrollSpawnTime = 0f;

    [Header("Weapon Drop")]
    public GameObject[] parachuteWeaponCrates = new GameObject[3];
    public GameObject RandwomWeaponDrop;
    public GameObject dropLocation;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] ambientMusics;
    public AudioClip[] bossMusics;
    public AudioClip bonusPointsClip;

    [Header("MANUAL LINKING")]
    public GameTime gameTime;
    [Space(10)]
    public Text gameInformerPlayer1;
    public Text gameInformerPlayer2;
    public Text gameInformerPlayer3;
    public Text gameInformerPlayer4;
    [Space(10)]
    public int player0Points;
    public int player1Points;
    public int player2Points;
    public int player3Points;
    [Space(10)]
    public int player0TotalPoints;
    public int player1TotalPoints;
    public int player2TotalPoints;
    public int player3TotalPoints;
    [Space(10)]
    public Text player0PointsText;
    public Text player1PointsText;
    public Text player2PointsText;
    public Text player3PointsText;

    public OnlineGameTime onlineGameTimeInstance;
    public AISpawnManager aiSpawnManagerInstance;
    public AIPool aiPool;
    int totalGameTime;
    int timeWaveStarted;
    int timeWaveEnded;

    [Header("Health Packs")]
    public List<HealthPack> healthPacks = new List<HealthPack>();
    private void Awake()
    {
        PV = gameObject.GetComponent<PhotonView>();
        if (PV.Owner.IsMasterClient)
        {
            onlineSwarmManagerInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        healthPacks = GetAllHealthPacks();
        aiPool = AIPool.aIPoolInstance;
        if (playerLives == 0)
            playerLives = 5;
        onlineGameTimeInstance = OnlineGameTime.onlineGameTimeInstance;
        aiSpawnManagerInstance = AISpawnManager.aISpawnManagerInstance;
        allPlayers = GetAllPlayers();
        foreach (PlayerProperties pp in allPlayers)
            pp.needsHealthPack = true;
        ResetPoints();
        UpdatePlayerLives();

        if (!PhotonNetwork.IsMasterClient)
            return;

        PlayAmbientSound();

        if (PV.IsMine)
            IncreaseWave(waveNumber);
    }

    void Update()
    {
        if (!PV.IsMine)
            return;
        if (waveInProgress /*&& !editMode*/)
        {
            CheckMaxAIsOnMap();
            totalGameTime = onlineGameTimeInstance.totalTime;

            if (zombiesLeftToSpawn > 0)
                if (totalGameTime > nextZombieSpawnTime)
                    SpawnZombie();


            //if (skeletonsLeftToSpawn > 0)
            //{
            //    if (totalGameTime > nextSkeletonSpawnTime)
            //    {
            //        SpawnSkeleton();
            //    }
            //}

            //if (watchersLeftToSpawn > 0)
            //{
            //    if (totalGameTime > nextWatcherSpawnTime)
            //    {
            //        SpawnWatcher();
            //    }
            //}

            //if (hellhoundsLeftToSpawn > 0)
            //{
            //    if (totalGameTime > nextHellhoundSpawnTime)
            //    {
            //        SpawnHellhound();
            //    }
            //}

            //if (trollsLeftToSpawn > 0)
            //{
            //    if (totalGameTime > nextTrollSpawnTime)
            //    {
            //        Debug.Log("Trying to spawn Troll");
            //        SpawnTroll();
            //    }
            //}

            //if (isBossWave)
            //{
            //    if (blackKnightsAlive == 0 && flameTyrantsAlive == 0)
            //    {
            //        waveInProgress = false;
            //        isBossWave = false;
            //        StartCoroutine(WaveEnd());
            //    }
            //}
            //else
            //{
            if (skeletonsLeftToSpawn == 0 && skeletonsAlive == 0 && watchersLeftToSpawn == 0 && watchersAlive == 0 &&
                            hellhoundsLeftToSpawn == 0 && hellhoundsAlive == 0 && trollsLeftToSpawn == 0 && trollsAlive == 0 && zombiesLeftToSpawn == 0 && zombiesAlive == 0)
            {
                waveInProgress = false;
                WaveEnd(onlineGameTimeInstance.totalTime);
            }
            //}
        }
    }

    void IncreaseWave(int _currentWave)
    {
        PV.RPC("IncreaseWave_RPC", RpcTarget.All, _currentWave);
    }

    [PunRPC]
    void IncreaseWave_RPC(int _currentWave)
    {

        StartCoroutine(IncreaseWave_Coroutine(_currentWave));
    }

    IEnumerator IncreaseWave_Coroutine(int param1)
    {
        waveNumber = waveNumber + 1;
        allPlayerWaveCounters = GetAllPlayerWaveCounters();

        yield return new WaitForSeconds(newWaveDelay);

        if (allPlayerWaveCounters.Count > 0)
            foreach (WaveCounter wc in allPlayerWaveCounters)
            {
                wc.gameObject.SetActive(true);
                wc.waveText.text = $"Wave: {waveNumber}";
            }


        CalculateMaxDefaultAIsForRound();

        //if (waveNumber % 5 == 0)
        //{
        //    Debug.Log("Weapon Drop");
        //    foreach (GameObject crate in parachuteWeaponCrates)
        //        crate.GetComponent<ParachuteWeaponDrop>().spawnCrate();

        //    if (RandwomWeaponDrop)
        //    {
        //        var drop = Instantiate(RandwomWeaponDrop, dropLocation.transform.position, dropLocation.transform.rotation);
        //        Destroy(drop, 10);
        //    }

        //    yield return new WaitForSeconds(25f);
        //}
        //else if (waveNumber == 1)
        //    yield return new WaitForSeconds(15f);
        //else
        //{
        //    yield return new WaitForSeconds(6.5f);
        //}

        if (PV.IsMine)
            WaveStart(onlineGameTimeInstance.totalTime);
    }

    void CalculateMaxDefaultAIsForRound()
    {
        allPlayers = GetAllPlayers();
        maxZombiesForRound = zombiesLeftToSpawn = allPlayers.Count * 5 + (waveNumber * 2);
        //maxSkeletonsForRound = skeletonsLeftToSpawn = allPlayers.Count * 4 + Mathf.CeilToInt(waveNumber / 2);
        //maxWatchersForRound = watchersLeftToSpawn = allPlayers.Count * 3 + Mathf.CeilToInt(waveNumber / 2);
        //maxHellhoundsForRound = hellhoundsLeftToSpawn = 0;
        //maxTrollsForRound = trollsLeftToSpawn = 0;

        //if (waveNumber % 5 == 0) //&& waveNumber % 10 != 0
        //{
        //    int randomSound = Random.Range(0, bossMusics.Length);
        //    audioSource.clip = bossMusics[randomSound];
        //    audioSource.Play();

        //    Debug.Log("Calculatin Hellhounds");
        //    maxHellhoundsForRound = hellhoundsLeftToSpawn = allPlayers.Count * 5 + Mathf.CeilToInt(waveNumber / 2);
        //}

        //if (waveNumber % 3 == 0)
        //{
        //    maxTrollsForRound = (Mathf.FloorToInt(waveNumber / 3));
        //    trollsLeftToSpawn = maxTrollsForRound;
        //}

        //if (waveNumber % 5 == 0)
        //{
        //    maxZombiesForRound = 0;
        //    zombiesLeftToSpawn = 0;
        //    maxSkeletonsForRound = 0;
        //    skeletonsLeftToSpawn = 0;
        //    maxWatchersForRound = 0;
        //    watchersLeftToSpawn = 0;
        //}

        //if (waveNumber % 10 == 0)
        //{
        //    StartCoroutine(CalculateBossWave());
        //}
    }

    IEnumerator CalculateBossWave()
    {
        //isBossWave = true;
        int randomSound = Random.Range(0, bossMusics.Length);

        audioSource.clip = bossMusics[randomSound];
        audioSource.Play();

        int randomBoss = Random.Range(1, 3);

        SpawnHellhound();
        yield return new WaitForSeconds(newWaveDelay * 3);
        if (randomBoss == 1)
        {
            //maxBlackKnightsForRound = (waveNumber / 10) * ssManager.numberOfPlayers;
            //SpawnBlackKnight();
        }
        if (randomBoss == 2)
        {
            //maxFlameTyrantsForRound = (waveNumber / 10) * ssManager.numberOfPlayers;
            //SpawnFlameTyrant();
        }
    }

    void WaveStart(int tgt)
    {
        PV.RPC("WaveStart_RPC", RpcTarget.All, tgt);
    }

    [PunRPC]
    void WaveStart_RPC(int tgt)
    {
        if (!onlineGameTimeInstance)
            onlineGameTimeInstance = OnlineGameTime.onlineGameTimeInstance;
        waveInProgress = true;
        totalGameTime = tgt;
        timeWaveStarted = tgt;

        nextZombieSpawnTime = totalGameTime + zombieSpawnDelay;
        nextSkeletonSpawnTime = totalGameTime + skeletonSpawnDelay;
        nextWatcherSpawnTime = totalGameTime + watcherSpawnDelay;
        nextHellhoundSpawnTime = totalGameTime + hellhoundSpawnDelay;
        nextTrollSpawnTime = totalGameTime + trollSpawnDelay;

        if (waveNumber > 1) // Already Called on Start
            PlayAmbientSound();
    }

    void SpawnZombie()
    {
        Debug.Log("RPC Call: SpawnZombie_RPC");
        nextZombieSpawnTime = totalGameTime + zombieSpawnDelay;
        if (!hasSpaceToSpawnZombie)
            return;
        allPlayers = GetAllPlayers();
        int ran = Random.Range(0, allPlayers.Count);
        int targetPhotonId = allPlayers[ran].PV.ViewID;

        Transform spawnPoint = aiSpawnManagerInstance.GetGenericSpawnpoint();
        PV.RPC("SpawnZombie_RPC", RpcTarget.All, aiPool.GetRandomZombiePhotonId(), targetPhotonId, spawnPoint.position, spawnPoint.rotation);
    }

    [PunRPC]
    void SpawnZombie_RPC(int AIPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {


        var newZombie = PhotonView.Find(AIPhotonId).gameObject;
        newZombie.GetComponent<ZombieScript>().EnableThisAi(targetPhotonId, spawnPointPosition, spawnPointRotation);

        if (!newZombie.GetComponent<ZombieScript>().onlineSwarmManager)
            newZombie.GetComponent<ZombieScript>().onlineSwarmManager = this;

        if (editMode)
            newZombie.GetComponent<ZombieScript>().defaultSpeed = 0.01f;
        zombiesAlive++;
        zombiesLeftToSpawn--;
    }

    void SpawnSkeleton()
    {
        if (hasSpaceToSpawnSkeleton)
        {
            //int i = Random.Range(0, skeletonSpawns.Length);
            //int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            //var newSkeleton = skeletonPool.SpawnPooledGameObject();
            //newSkeleton.transform.position = skeletonSpawns[i].transform.position;
            //newSkeleton.transform.rotation = skeletonSpawns[i].transform.rotation;
            //newSkeleton.SetActive(true);
            //if (!newSkeleton.GetComponent<Skeleton>().swarmMode)
            //    newSkeleton.GetComponent<Skeleton>().swarmMode = this;
            //newSkeleton.GetComponent<Skeleton>().target = pManager.allPlayers[b].transform;
            //skeletonsAlive ++;
            //skeletonsLeftToSpawn --;
        }

        nextSkeletonSpawnTime = totalGameTime + skeletonSpawnDelay;
    }

    void SpawnWatcher()
    {
        if (hasSpaceToSpawnWatcher)
        {
            //int i = Random.Range(0, watcherSpawns.Length);
            //int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            //var newWatcher = watcherPool.SpawnPooledGameObject();
            //newWatcher.transform.position = watcherSpawns[i].transform.position;
            //newWatcher.transform.rotation = watcherSpawns[i].transform.rotation;
            //newWatcher.SetActive(true);
            //if (!newWatcher.GetComponent<Watcher>().swarmMode)
            //    newWatcher.GetComponent<Watcher>().swarmMode = this;
            //newWatcher.GetComponent<Watcher>().target = pManager.allPlayers[b].transform;
            //watchersAlive++;
            //watchersLeftToSpawn--;
        }

        nextWatcherSpawnTime = gameTime.totalGameTime + watcherSpawnDelay;
    }

    void SpawnHellhound()
    {
        if (hasSpaceToSpawnHellhound)
        {
            //int i = Random.Range(0, hellhoundSpawns.Length);
            //int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            //var newHound = hellhoundPool.SpawnPooledGameObject();
            //newHound.transform.position = hellhoundSpawns[i].transform.position;
            //newHound.transform.rotation = hellhoundSpawns[i].transform.rotation;
            //newHound.SetActive(true);
            //if (!newHound.GetComponent<Hellhound>().swarmMode)
            //    newHound.GetComponent<Hellhound>().swarmMode = this;
            //newHound.GetComponent<Hellhound>().target = pManager.allPlayers[b].transform;
            //hellhoundsAlive++;
            //hellhoundsLeftToSpawn--;
        }
        nextHellhoundSpawnTime = gameTime.totalGameTime + hellhoundSpawnDelay;
    }

    void SpawnTroll()
    {
        if (hasSpaceToSpawnTroll)
        {
            //int i = Random.Range(0, trollSpawns.Length);
            //int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            //var newTroll = trollPool.SpawnPooledGameObject();
            //newTroll.transform.position = trollSpawns[i].transform.position;
            //newTroll.transform.rotation = trollSpawns[i].transform.rotation;
            //newTroll.SetActive(true);
            //if (!newTroll.GetComponent<Troll>().swarmMode)
            //    newTroll.GetComponent<Troll>().swarmMode = this;
            //newTroll.GetComponent<Troll>().target = pManager.allPlayers[b].transform;
            //trollsAlive++;
            //trollsLeftToSpawn--;
        }
        nextTrollSpawnTime = gameTime.totalGameTime + trollSpawnDelay;
    }

    void SpawnBlackKnight()
    {
        int a = Random.Range(0, skeletonSpawns.Length);
        int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

        /*
        for (int i = 0; i < maxBlackKnightsForRound; i++)
        {
            GameObject blackKnight = Instantiate(blackKnightPrefab, skeletonSpawns[a].gameObject.transform.position, skeletonSpawns[a].gameObject.transform.rotation);
            blackKnight.GetComponent<BlackKnight>().target = pManager.allPlayers[b].transform;
            blackKnight.GetComponent<BlackKnight>().swarmMode = this;

            blackKnightsAlive = blackKnightsAlive + 1;
        }*/

        maxHellhoundsForRound = ssManager.numberOfPlayers * 5 + Mathf.CeilToInt(waveNumber / 2);
        hellhoundsLeftToSpawn = maxHellhoundsForRound;
    }

    void SpawnFlameTyrant()
    {
        int a = Random.Range(0, skeletonSpawns.Length);
        int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

        /*
        for (int i = 0; i < maxFlameTyrantsForRound; i++)
        {
            GameObject flameTyrant = Instantiate(flameTyrantPrefab, skeletonSpawns[a].gameObject.transform.position, skeletonSpawns[a].gameObject.transform.rotation);
            flameTyrant.GetComponent<FlameTyrant>().target = pManager.allPlayers[b].transform;
            flameTyrant.GetComponent<FlameTyrant>().swarmMode = this;

            flameTyrantsAlive = flameTyrantsAlive + 1;
        }*/
    }

    void WaveEnd(int _timeWaveEnded)
    {
        PV.RPC("WaveEnd_RPC", RpcTarget.All, _timeWaveEnded);
    }

    [PunRPC]
    void WaveEnd_RPC(int _timeWaveEnded)
    {
        StartCoroutine(WaveEnd_Coroutine(_timeWaveEnded));
    }
    IEnumerator WaveEnd_Coroutine(int _timeWaveEnded)
    {
        allPlayerWaveCounters = GetAllPlayerWaveCounters();
        timeWaveEnded = _timeWaveEnded;

        int bonusPoints = (1000 * waveNumber) - ((timeWaveEnded - timeWaveStarted - waveNumber) * 10);
        if (bonusPoints > 0)
            foreach (WaveCounter wc in allPlayerWaveCounters)
                wc.waveText.text = $"Wave complete! Bonus points: {bonusPoints}";
        else if (bonusPoints <= 0)
            foreach (WaveCounter wc in allPlayerWaveCounters)
                wc.waveText.text = $"No bonus points. Finish the wave faster";

        if(PV.IsMine)
            GivePlayerBonusPoints(bonusPoints);

        yield return new WaitForSeconds(newWaveDelay);
        Debug.Log("Reinforcements (Voice)");
        IncreaseWave(waveNumber);
    }

    void GivePlayerBonusPoints(int points)
    {
        PV.RPC("GivePlayerBonusPoints_RPC", RpcTarget.All, points);
    }

    [PunRPC]
    void GivePlayerBonusPoints_RPC(int points)
    {
        allPlayers = GetAllPlayers();
        foreach (PlayerProperties pp in allPlayers)
            pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(points);
        PlayerBonusPointsSound();
    }

    void PlayerBonusPointsSound()
    {
        audioSource.Stop();
        audioSource.volume = 1;
        audioSource.clip = bonusPointsClip;
        audioSource.Play();
    }

    void PlayAmbientSound()
    {
        PV.RPC("PlayAmbientSound_RPC", RpcTarget.All);
    }

    [PunRPC]

    void PlayAmbientSound_RPC()
    {
        if (ambientMusics.Length > 0)
        {
            audioSource.volume = 0.1f;
            int randomSound = Random.Range(0, ambientMusics.Length - 1);
            audioSource.clip = ambientMusics[randomSound];
            audioSource.Play();

            //if (waveNumber % 5 != 0 || waveNumber % 10 != 0)
            //{
            //    int randomSound = Random.Range(0, ambientMusics.Length - 1);
            //    audioSource.clip = ambientMusics[randomSound];
            //    audioSource.Play();
            //}
        }
    }

    void ResetPoints()
    {
        allPlayers = GetAllPlayers();
        allPlayerSwarmScripts = GetAllPlayerSwarmScripts();

        foreach (PlayerProperties pp in allPlayers)
        {
            pp.GetComponent<OnlinePlayerSwarmScript>().ResetPoints();
            pp.allPlayerScripts.playerUIComponents.multiplayerPoints.SetActive(false);
            pp.allPlayerScripts.playerUIComponents.swarmPoints.SetActive(true);
            pp.allPlayerScripts.playerUIComponents.swarmPointsText.text = 0.ToString();
        }
    }

    void CheckMaxAIsOnMap()
    {
        if (zombiesAlive >= maxZombiesOnMap)
        {
            hasSpaceToSpawnZombie = false;
        }
        else
        {
            hasSpaceToSpawnZombie = true;
        }

        if (skeletonsAlive >= maxSkeletonsOnMap)
        {
            hasSpaceToSpawnSkeleton = false;
        }
        else
        {
            hasSpaceToSpawnSkeleton = true;
        }

        if (watchersAlive >= maxWatchersOnMap)
        {
            hasSpaceToSpawnWatcher = false;
        }
        else
        {
            hasSpaceToSpawnWatcher = true;
        }

        if (hellhoundsAlive >= maxHellhoundsOnMap)
        {
            hasSpaceToSpawnHellhound = false;
        }
        else
        {
            hasSpaceToSpawnHellhound = true;
        }

        if (trollsAlive >= maxTrollsOnMap)
        {
            hasSpaceToSpawnTroll = false;
        }
        else
        {
            hasSpaceToSpawnTroll = true;
        }
    }

    public void UpdatePlayerLives()
    {
        allPlayers = GetAllPlayers();
        foreach (PlayerProperties player in allPlayers)
        {
            player.allPlayerScripts.playerUIComponents.swarmLivesHolder.SetActive(true);
            player.allPlayerScripts.playerUIComponents.swarmLivesText.text = playerLives.ToString();
        }
    }

    public Transform NewTargetFromSwarmScript()
    {
        int newTarget = Random.Range(0, 4);
        Transform newTargetPlayer = null;

        if (pManager.allPlayers[newTarget].gameObject.activeSelf)
        {
            if (pManager.allPlayers[newTarget].gameObject.GetComponent<PlayerProperties>() != null)
            {
                if (!pManager.allPlayers[newTarget].gameObject.GetComponent<PlayerProperties>().isDead)
                {
                    newTargetPlayer = pManager.allPlayers[newTarget].gameObject.transform;
                }
            }
        }

        return newTargetPlayer;
    }

    public List<PlayerProperties> GetAllPlayers()
    {
        List<PlayerProperties> allPlayers = new List<PlayerProperties>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
            allPlayers.Add(go.GetComponent<PlayerProperties>());

        return allPlayers;
    }

    public List<HealthPack> GetAllHealthPacks()
    {
        List<HealthPack> allHealthPacks = new List<HealthPack>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("health_pack"))
            allHealthPacks.Add(go.GetComponent<HealthPack>());

        return allHealthPacks;
    }

    public List<WaveCounter> GetAllPlayerWaveCounters()
    {
        List<WaveCounter> waveCounters = new List<WaveCounter>();

        allPlayers = GetAllPlayers();
        foreach (PlayerProperties player in allPlayers)
        {
            if (player.GetComponent<AllPlayerScripts>().waveCounter)
                waveCounters.Add(player.GetComponent<AllPlayerScripts>().waveCounter);
        }

        return waveCounters;
    }

    public List<OnlinePlayerSwarmScript> GetAllPlayerSwarmScripts()
    {
        List<OnlinePlayerSwarmScript> allPlayerSwarmScripts = new List<OnlinePlayerSwarmScript>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
            allPlayerSwarmScripts.Add(go.GetComponent<OnlinePlayerSwarmScript>());

        return allPlayerSwarmScripts;
    }

    public void RespawnHealthPack(Vector3 hpPosition, int time)
    {
        PV.RPC("RespawnHealthPack_RPC", RpcTarget.All, hpPosition, time);
    }

    [PunRPC]
    void RespawnHealthPack_RPC(Vector3 hpPosition, int time)
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

    public void DisableHealthPack(Vector3 hpPosition)
    {
        PV.RPC("DisableHealthPack_RPC", RpcTarget.All, hpPosition);
    }

    [PunRPC]
    void DisableHealthPack_RPC(Vector3 hpPosition)
    {
        foreach (HealthPack hp in healthPacks)
            if (hp.transform.position == hpPosition)
                hp.gameObject.SetActive(false);
    }

    public void RemovePlayerLife()
    {
        if (PV.IsMine)
            PV.RPC("RemovePlayerLife_RPC", RpcTarget.All);
    }

    [PunRPC]
    void RemovePlayerLife_RPC()
    {
        playerLives--;
        UpdatePlayerLives();

        if (playerLives <= 0)
            EndGame();
    }

    void EndGame()
    {
        PV.RPC("EndGame_RPC", RpcTarget.All);
    }

    [PunRPC]
    void EndGame_RPC()
    {
        Debug.Log("Ending Game");
        List<PlayerProperties> allPlayers = new List<PlayerProperties>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
            allPlayers.Add(go.GetComponent<PlayerProperties>());

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (!allPlayers[i].PV.IsMine)
                return;
            allPlayers[i].allPlayerScripts.announcer.PlayGameOverClip();
            allPlayers[i].LeaveRoomWithDelay();
        }
    }
}
