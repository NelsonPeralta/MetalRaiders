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
    public enum AiType { Watcher, Knight, Hellhound }

    public int currentWave;
    public int nextWaveDelay;

    [Header("AI Prefabs")]
    public Transform watcherPrefab;
    public Transform knightPrefab;
    public Transform hellhoundPrefab;

    [Header("AI Pools")]
    public Watcher[] watcherPool;
    public Knight[] knightPool;
    public Hellhound[] hellhoundPool;

    // private variables
    PhotonView PV;
    int maxWave;
    int livesLeft = 4;


    int watchersLeft;
    int knightsLeft;
    int hellhoundsLeft;

    int maxWatchersOnMap = 2;


    // constants
    const int WATCHER_SPAWN_DELAY = 8;
    const int KNIGHT_SPAWN_DELAY = 10;
    const int HELLHOUND_SPAWN_DELAY = 5;

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

        knightPool = FindObjectsOfType<Knight>();
        foreach (Knight w in knightPool)
            w.gameObject.SetActive(false);

        hellhoundPool = FindObjectsOfType<Hellhound>();
        foreach (Hellhound w in hellhoundPool)
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
        watchersLeft = FindObjectsOfType<PlayerProperties>().Length * 2 + (currentWave * 3);
        if (watchersLeft > watcherPool.Length)
            watchersLeft = watcherPool.Length;

        knightsLeft = FindObjectsOfType<PlayerProperties>().Length * 1 + (currentWave * 2);
        if (knightsLeft > knightPool.Length)
            knightsLeft = knightPool.Length;

        hellhoundsLeft = FindObjectsOfType<PlayerProperties>().Length * 3 + (currentWave * 4);
        if (hellhoundsLeft > hellhoundPool.Length)
            hellhoundsLeft = hellhoundPool.Length;

        if (editMode)
        {
            knightsLeft = 1;
            hellhoundsLeft = 1;
            watchersLeft = 1;
        }


        Debug.Log($"Watchers Left: {watchersLeft}. Knights left: {knightsLeft}. Hellhounds left: {hellhoundsLeft}");
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
        SpawnAi(AiType.Watcher);
        SpawnAi(AiType.Knight);
        SpawnAi(AiType.Hellhound);
    }
    void SpawnAi(AiType aiType)
    {
        //Debug.Log($"Spawning type of ai: {aiType}");
        if (!PhotonNetwork.IsMasterClient)
            return;

        int targetPhotonId = GetRandomPlayerPhotonId();

        List<SpawnPoint> aiSpawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
            if (sp.spawnPointType == SpawnPoint.SpawnPointType.Computer)
                aiSpawnPoints.Add(sp);
        int aiPhotonId = -1;
        Transform spawnPoint = aiSpawnPoints[Random.Range(0, aiSpawnPoints.Count)].transform;

        if (aiType == AiType.Watcher)
        {
            if (watchersLeft <= 0)
            {
                OnAiLeftZero?.Invoke(this);
                return;
            }

            foreach (Watcher w in watcherPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Watcher.ToString());
        }
        else if (aiType == AiType.Knight)
        {
            if (knightsLeft <= 0)
            {
                OnAiLeftZero?.Invoke(this);
                return;
            }

            foreach (Knight w in knightPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Knight.ToString());
        }
        else if (aiType == AiType.Hellhound)
        {
            if (hellhoundsLeft <= 0)
            {
                OnAiLeftZero?.Invoke(this);
                return;
            }

            foreach (Hellhound w in hellhoundPool)
                if (!w.gameObject.activeSelf)
                    aiPhotonId = w.GetComponent<PhotonView>().ViewID;

            PV.RPC("SpawnAi_RPC", RpcTarget.All, aiPhotonId, targetPhotonId, spawnPoint.position, spawnPoint.rotation, AiType.Hellhound.ToString());
        }
    }

    // https://docs.microsoft.com/en-us/dotnet/api/system.func-2?view=net-6.0
    // https://docs.microsoft.com/en-us/dotnet/api/system.action-1?view=net-6.0
    [PunRPC]
    void SpawnAi_RPC(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation, string aiType)
    {
        Debug.Log($"SpawnAi_RPC. AI pdi: {aiPhotonId}");
        StartCoroutine(SpawnWatcher_Coroutine(aiPhotonId, targetPhotonId, spawnPointPosition, spawnPointRotation, aiType));
    }
    IEnumerator SpawnWatcher_Coroutine(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation, string aiType)
    {
        AiType aiTypeEnum = (AiType)System.Enum.Parse(typeof(AiType), aiType);
        int delay = 10;

        if (aiTypeEnum == AiType.Watcher)
            delay = WATCHER_SPAWN_DELAY;
        else if (aiTypeEnum == AiType.Knight)
            delay = KNIGHT_SPAWN_DELAY;
        else if (aiTypeEnum == AiType.Hellhound)
            delay = HELLHOUND_SPAWN_DELAY;

        yield return new WaitForSeconds(delay);

        var newAiObj = PhotonView.Find(aiPhotonId).gameObject;
        newAiObj.GetComponent<AiAbstractClass>().Spawn(targetPhotonId, spawnPointPosition, spawnPointRotation);

        if (aiTypeEnum == AiType.Watcher)
            watchersLeft--;
        else if (aiTypeEnum == AiType.Knight)
            knightsLeft--;
        else if (aiTypeEnum == AiType.Hellhound)
            hellhoundsLeft--;
        SpawnAi(aiTypeEnum);
    }
    void AiLeftHitZero(SwarmManager swarmManager)
    {
        int watchersAlive = 0;
        int knightsAlive = 0;
        int hellhoundsAlive = 0;

        foreach (Watcher w in watcherPool)
            if (w.gameObject.activeSelf && !w.isDead)
                watchersAlive++;

        foreach (Knight w in knightPool)
            if (w.gameObject.activeSelf && !w.isDead)
                knightsAlive++;

        foreach (Hellhound w in hellhoundPool)
            if (w.gameObject.activeSelf && !w.isDead)
                hellhoundsAlive++;

        Debug.Log($"AI CHECK. Watchers left: {watchersLeft}. Watchers alive: {watchersAlive}. Knights left: {knightsLeft}. Knights alive: {knightsAlive}. Hellhounds left: {hellhoundsAlive}. Hellhounds left: {hellhoundsLeft}");

        if (watchersLeft <= 0 && watchersAlive <= 0 && knightsLeft <= 0 && knightsAlive <= 0 && hellhoundsLeft <= 0 && hellhoundsAlive <= 0)
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

    int GetRandomPlayerPhotonId()
    {
        PlayerProperties[] allPlayers = FindObjectsOfType<PlayerProperties>();
        int ran = Random.Range(0, allPlayers.Length);
        int targetPhotonId = allPlayers[ran].PV.ViewID;
        return targetPhotonId;
    }

    public Transform GetRandomPlayerTransform()
    {
        return PhotonView.Find(GetRandomPlayerPhotonId()).transform;
    }
}
