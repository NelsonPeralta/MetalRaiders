using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class SwarmManager : MonoBehaviourPunCallbacks
{
    public bool editMode;
    // Events 
    public delegate void SwarmManagerEvent(SwarmManager swarmManager);
    public SwarmManagerEvent OnBegin, OnWaveIncrease, OnWaveStart, OnWaveEnd, OnAiLeftZero;

    // public variables
    public static SwarmManager instance;

    public int currentWave;
    public int nextWaveDelay;

    [Header("AI Prefabs")]
    public Transform watcherPrefab;

    [Header("AI Pools")]
    public Watcher[] watcherPool;

    // private variables
    PhotonView PV;
    int maxWave;
    int livesLeft = 4;


    int watchersLeft;

    int maxWatchersOnMap = 2;


    // constants
    const int WATCHER_SPAWN_DELAY = 5;

    private void Awake()
    {
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
        PV = GetComponent<PhotonView>();

        GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;

        OnWaveStart += SpawnAIs;
        OnAiLeftZero += AiLeftHitZero;
        OnWaveEnd += OnWaveEnd_Delegate;

        CreateAIPool();
    }

    void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Swarm)
                return;

            if (GameManager.instance.swarmMode == GameManager.SwarmMode.Survival)
                maxWave = 999999;
        }
        else // We are in the menu
        {
            maxWave = 0;
            return;
        }

        Begin();
    }

    void CreateAIPool()
    {
        // Watcher GameObject must be active in order to be found with FindObjectsOfType
        watcherPool = FindObjectsOfType<Watcher>();
        foreach (Watcher w in watcherPool)
            w.gameObject.SetActive(false);


    }
    void Begin()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PV.RPC("Begin_RPC", RpcTarget.All);
    }

    [PunRPC]
    void Begin_RPC()
    {
        Debug.Log("Begin_RPC");
        // eg: Disable player shields

        IncreaseWave();
    }

    void IncreaseWave()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PV.RPC("IncreaseWave_RPC", RpcTarget.All);
    }

    [PunRPC]
    void IncreaseWave_RPC()
    {
        currentWave++;
        CalculateNumberOfAIsForNextWave();
        Debug.Log("Wave Increased");


        OnWaveIncrease?.Invoke(this);
        StartNewWave();
    }

    void CalculateNumberOfAIsForNextWave()
    {
        watchersLeft = FindObjectsOfType<PlayerProperties>().Length * 2 + (currentWave * 2);
        if (watchersLeft > watcherPool.Length)
            watchersLeft = watcherPool.Length;
        if (editMode)
            watchersLeft = 1;
        Debug.Log($"Watchers Left: {watchersLeft}");
    }

    void StartNewWave()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PV.RPC("StartNewWave_RPC", RpcTarget.All);
    }

    [PunRPC]
    void StartNewWave_RPC()
    {
        Debug.Log($"StartNewWave_RPC");
        StartCoroutine(StartNewWave_Coroutine());
    }
    IEnumerator StartNewWave_Coroutine()
    {
        int delay = FindObjectsOfType<PlayerProperties>().Length * 3;
        yield return new WaitForSeconds(delay);

        OnWaveStart?.Invoke(this);
    }

    void SpawnAIs(SwarmManager swarmManager)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        Debug.Log("Spawning Ais");
        SpawnWatcher();
    }

    void SpawnWatcher()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        //if(FindObjectsOfType<Watcher>().Length >= maxWatchersOnMap)
        //{
        //    StartCoroutine(SpawnWatcher_Coroutine());
        //    return;
        //}

        if (watchersLeft <= 0)
        {
            OnAiLeftZero?.Invoke(this);
            return;
        }
        PlayerProperties[] allPlayers = FindObjectsOfType<PlayerProperties>();
        int ran = Random.Range(0, allPlayers.Length);
        int targetPhotonId = allPlayers[ran].PV.ViewID;

        List<SpawnPoint> aiSpawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
            if (sp.spawnPointType == SpawnPoint.SpawnPointType.Computer)
                aiSpawnPoints.Add(sp);

        int aiPhotonId = -1;

        foreach (Watcher w in watcherPool)
            if (!w.gameObject.activeSelf)
                aiPhotonId = w.GetComponent<PhotonView>().ViewID;

        Transform spawnPoint = aiSpawnPoints[Random.Range(0, aiSpawnPoints.Count)].transform;

        PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation);
    }

    [PunRPC]
    void SpawnAi_RPC(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        Debug.Log($"SpawnAi_RPC. AI pdi: {aiPhotonId}");
        StartCoroutine(SpawnWatcher_Coroutine(aiPhotonId, targetPhotonId, spawnPointPosition, spawnPointRotation));
    }
    IEnumerator SpawnWatcher_Coroutine(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        yield return new WaitForSeconds(WATCHER_SPAWN_DELAY);

        var newWatcher = PhotonView.Find(aiPhotonId).gameObject;
        newWatcher.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);
        
        watchersLeft--;

        SpawnWatcher();
    }

    IEnumerator SpawnWatcher_Coroutine()
    {
        yield return new WaitForSeconds(WATCHER_SPAWN_DELAY);
        SpawnWatcher();
    }

    void AiLeftHitZero(SwarmManager swarmManager)
    {
        int watchersAlive = 0;
        foreach (Watcher w in watcherPool)
        {
            Debug.Log($"{w.name} is active: {w.gameObject.activeSelf} and is dead: { w.isDead}");
            if (w.gameObject.activeSelf && !w.isDead)
            {
                watchersAlive++;

            }
        }
        Debug.Log($"Watchers Left: {watchersLeft}. Watchers alive: {watchersAlive}");
        if (watchersLeft <= 0 && watchersAlive <= 0)
            EndWave();
    }

    public void OnAiDeath()
    {
        Debug.Log("Swarm Manager OnAiDeath");
        AiLeftHitZero(this);
    }
    void EndWave()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PV.RPC("EndWave_RPC", RpcTarget.All);
    }

    [PunRPC]
    void EndWave_RPC()
    {
        Debug.Log("Wave End");
        StartCoroutine(EndWave_Coroutine());
    }

    IEnumerator EndWave_Coroutine()
    {
        nextWaveDelay = FindObjectsOfType<PlayerProperties>().Length * 10;
        yield return new WaitForSeconds(nextWaveDelay);
        OnWaveEnd?.Invoke(this);
    }

    void OnWaveEnd_Delegate(SwarmManager swarmManager)
    {
        IncreaseWave();
    }
}
