using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwarmMode : MonoBehaviour
{
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
    public GameObject[] players;
    public int playerLives;

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

    [Header("General Game Information")]
    public int maxZombiesForRound;
    public int maxSkeletonsForRound;
    public int maxWatchersForRound;
    public int maxHellhoundsForRound;
    public int maxTrollsForRound;

    [Header("Boss AIs Information")]
    public bool isBossWave;
    public int maxBlackKnightsForRound;
    public int blackKnightsAlive;
    public int maxFlameTyrantsForRound;
    public int flameTyrantsAlive;

    [Header("AIs Left To Spawn")]
    public int zombiesLeftToSpawn;
    public int skeletonsLeftToSpawn;
    public int watchersLeftToSpawn;
    public int hellhoundsLeftToSpawn;
    public int trollsLeftToSpawn;

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

    void Start()
    {
        pManager = GameObject.FindGameObjectWithTag("Player Manager").gameObject.GetComponent<MyPlayerManager>();
        Cursor.visible = false;

        ResetPoints();
        StartCoroutine(PlayAmbientSound());
        StartCoroutine(UpdateWaveNumber(waveNumber));
        GivePlayersThis();
        UpdatePlayerLives();
    }

    void Update()
    {
        if (waveInProgress && !editMode)
        {
            CheckMaxAIsOnMap();

            if (zombiesLeftToSpawn > 0)
            {
                if (gameTime.totalGameTime > nextZombieSpawnTime)
                {
                    SpawnZombie();
                }
            }


            if (skeletonsLeftToSpawn > 0)
            {
                if (gameTime.totalGameTime > nextSkeletonSpawnTime)
                {
                    SpawnSkeleton();
                }
            }

            if (watchersLeftToSpawn > 0)
            {
                if (gameTime.totalGameTime > nextWatcherSpawnTime)
                {
                    SpawnWatcher();
                }
            }

            if (hellhoundsLeftToSpawn > 0)
            {
                if (gameTime.totalGameTime > nextHellhoundSpawnTime)
                {
                    SpawnHellhound();
                }
            }

            if (trollsLeftToSpawn > 0)
            {
                if (gameTime.totalGameTime > nextTrollSpawnTime)
                {
                    Debug.Log("Trying to spawn Troll");
                    SpawnTroll();
                }
            }



            if (isBossWave)
            {
                if (blackKnightsAlive == 0 && flameTyrantsAlive == 0)
                {
                    waveInProgress = false;
                    isBossWave = false;
                    StartCoroutine(WaveEnd());
                }
            }
            else
            {
                if (skeletonsLeftToSpawn == 0 && skeletonsAlive == 0 && watchersLeftToSpawn == 0 && watchersAlive == 0 &&
                                hellhoundsLeftToSpawn == 0 && hellhoundsAlive == 0 && trollsLeftToSpawn == 0 && trollsAlive == 0)
                {
                    waveInProgress = false;
                    StartCoroutine(WaveEnd());
                }
            }
        }
    }

    IEnumerator UpdateWaveNumber(int param1)
    {
        waveNumber = waveNumber + 1;
        List<WaveCounter> waveCounters = new List<WaveCounter>();

        yield return new WaitForSeconds(3.5f);

        if (pManager)
        {
            foreach(GameObject player in pManager.allPlayers)
            {
                if (player.GetComponent<AllPlayerScripts>().waveCounter)
                    waveCounters.Add(player.GetComponent<AllPlayerScripts>().waveCounter);
            }
        }

        if(waveCounters.Count > 0)
            foreach(WaveCounter wc in waveCounters)
                wc.gameObject.SetActive(false);

        if (waveCounters.Count > 0)
            foreach (WaveCounter wc in waveCounters)
                StartCoroutine(wc.UpdateWaveNumber(waveNumber));

        /*

        if (players[0] != null)
        {
            if (players[0].gameObject.GetComponent<WaveCounter>() != null)
            {
                WaveCounter wc = players[0].gameObject.GetComponent<WaveCounter>();
                StartCoroutine(wc.UpdateWaveNumber(waveNumber));
                
            }
        }

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.SetActive(false);
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.SetActive(false);
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.SetActive(false);
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.SetActive(false);

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.GetComponent<Text>().text = "Wave " + waveNumber;
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.GetComponent<Text>().text = "Wave " + waveNumber;
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.GetComponent<Text>().text = "Wave " + waveNumber;
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.GetComponent<Text>().text = "Wave " + waveNumber;
        yield return new WaitForSeconds(.25f);

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.SetActive(true);
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.SetActive(true);
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.SetActive(true);
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.SetActive(true);
        yield return new WaitForSeconds(.25f);

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.SetActive(false);
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.SetActive(false);
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.SetActive(false);
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.SetActive(false);
        yield return new WaitForSeconds(.25f);

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.SetActive(true);
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.SetActive(true);
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.SetActive(true);
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.SetActive(true);
        yield return new WaitForSeconds(.25f);

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.SetActive(false);
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.SetActive(false);
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.SetActive(false);
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.SetActive(false);
        yield return new WaitForSeconds(.25f);

        if (gameInformerPlayer1 != null)
            gameInformerPlayer1.gameObject.SetActive(true);
        if (gameInformerPlayer2 != null)
            gameInformerPlayer2.gameObject.SetActive(true);
        if (gameInformerPlayer3 != null)
            gameInformerPlayer3.gameObject.SetActive(true);
        if (gameInformerPlayer4 != null)
            gameInformerPlayer4.gameObject.SetActive(true);
        yield return new WaitForSeconds(.25f);

        */

        CalculateMaxDefaultAIsForRound();

        if (waveNumber % 5 == 0)
        {
            Debug.Log("Weapon Drop");
            foreach (GameObject crate in parachuteWeaponCrates)
                crate.GetComponent<ParachuteWeaponDrop>().spawnCrate();

            if (RandwomWeaponDrop)
            {
                var drop = Instantiate(RandwomWeaponDrop, dropLocation.transform.position, dropLocation.transform.rotation);
                Destroy(drop, 10);
            }

            yield return new WaitForSeconds(25f);
        }
        else if(waveNumber == 1)
            yield return new WaitForSeconds(15f);
        else
        {
            yield return new WaitForSeconds(6.5f);
        }

        WaveStart();
    }

    void CalculateMaxDefaultAIsForRound()
    {
        maxZombiesForRound = ssManager.numberOfPlayers * 3 + Mathf.CeilToInt(waveNumber / 2);
        zombiesLeftToSpawn = maxZombiesForRound;

        maxSkeletonsForRound = ssManager.numberOfPlayers * 4 + Mathf.CeilToInt(waveNumber / 2);
        skeletonsLeftToSpawn = maxSkeletonsForRound;

        maxWatchersForRound = ssManager.numberOfPlayers * 3 + Mathf.CeilToInt(waveNumber / 2);
        watchersLeftToSpawn = maxWatchersForRound;

        if (waveNumber % 5 == 0) //&& waveNumber % 10 != 0
        {
            int randomSound = Random.Range(0, bossMusics.Length);
            audioSource.clip = bossMusics[randomSound];
            audioSource.Play();

            Debug.Log("Calculatin Hellhounds");
            maxHellhoundsForRound = ssManager.numberOfPlayers * 5 + Mathf.CeilToInt(waveNumber / 2);
            hellhoundsLeftToSpawn = maxHellhoundsForRound;
        }
        else
        {
            Debug.Log("Hellhounds at 0" + waveNumber);
            maxHellhoundsForRound = 0;
            hellhoundsLeftToSpawn = 0;
        }

        if (waveNumber > 5)
        {
            if (waveNumber % 2 == 0)
            {
                maxTrollsForRound = (Mathf.FloorToInt(waveNumber / 3));
                trollsLeftToSpawn = maxTrollsForRound;
            }
            if (waveNumber % 5 == 0)
            {
                maxTrollsForRound = 0;
                trollsLeftToSpawn = 0;
            }
        }
        else
        {
            maxTrollsForRound = 0;
            trollsLeftToSpawn = 0;
        }

        if (waveNumber % 5 == 0 || waveNumber % 10 == 0)
        {
            maxZombiesForRound = 0;
            zombiesLeftToSpawn = 0;
            maxSkeletonsForRound = 0;
            skeletonsLeftToSpawn = 0;
            maxWatchersForRound = 0;
            watchersLeftToSpawn = 0;
        }

        if (waveNumber % 10 == 0)
        {
            StartCoroutine(CalculateBossWave());
        }
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

    void WaveStart()
    {
        waveInProgress = true;

        nextZombieSpawnTime = gameTime.totalGameTime + zombieSpawnDelay;
        nextSkeletonSpawnTime = gameTime.totalGameTime + skeletonSpawnDelay;
        nextWatcherSpawnTime = gameTime.totalGameTime + watcherSpawnDelay;
        nextHellhoundSpawnTime = gameTime.totalGameTime + hellhoundSpawnDelay;
        nextTrollSpawnTime = gameTime.totalGameTime + trollSpawnDelay;
    }

    void SpawnZombie()
    {
        if (hasSpaceToSpawnZombie)
        {
            int i = Random.Range(0, ZombieSpawns.Length);
            int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            var newZombie = zombiePool.SpawnPooledGameObject();
            newZombie.transform.position = ZombieSpawns[i].transform.position;
            newZombie.transform.rotation = ZombieSpawns[i].transform.rotation;
            newZombie.SetActive(true);
            if (!newZombie.GetComponent<ZombieScript>().swarmMode)
                newZombie.GetComponent<ZombieScript>().swarmMode = this;
            newZombie.GetComponent<ZombieScript>().target = pManager.allPlayers[b].transform;
            zombiesAlive ++;
            zombiesLeftToSpawn --;

            //if (ZombieSpawns[i] != null)
            //{
            //    GameObject zombie = Instantiate(zombiePrefab, ZombieSpawns[i].gameObject.transform.position, ZombieSpawns[i].gameObject.transform.rotation);
            //    zombie.GetComponent<ZombieScript>().swarmMode = this;
            //    zombie.GetComponent<ZombieScript>().target = pManager.allPlayers[b].transform;
            //    zombiesAlive = zombiesAlive + 1;
            //    zombiesLeftToSpawn = zombiesLeftToSpawn - 1;
            //}
        }

        nextZombieSpawnTime = gameTime.totalGameTime + zombieSpawnDelay;
    }

    void SpawnSkeleton()
    {
        if (hasSpaceToSpawnSkeleton)
        {
            int i = Random.Range(0, skeletonSpawns.Length);
            int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            var newSkeleton = skeletonPool.SpawnPooledGameObject();
            newSkeleton.transform.position = skeletonSpawns[i].transform.position;
            newSkeleton.transform.rotation = skeletonSpawns[i].transform.rotation;
            newSkeleton.SetActive(true);
            if (!newSkeleton.GetComponent<Skeleton>().swarmMode)
                newSkeleton.GetComponent<Skeleton>().swarmMode = this;
            newSkeleton.GetComponent<Skeleton>().target = pManager.allPlayers[b].transform;
            skeletonsAlive ++;
            skeletonsLeftToSpawn --;

            //if (skeletonSpawns[i] != null)
            //{
            //    GameObject skeleton = Instantiate(skeletonPrefab, skeletonSpawns[i].gameObject.transform.position, skeletonSpawns[i].gameObject.transform.rotation);
            //    skeleton.GetComponent<Skeleton>().swarmMode = this;
            //    skeleton.GetComponent<Skeleton>().target = pManager.allPlayers[b].transform;
            //    /*
            //    skeleton.GetComponent<AIFieldOfVision>().player0 = pManager.allPlayers[0].transform;
            //    skeleton.GetComponent<AIFieldOfVision>().player1 = pManager.allPlayers[1].transform;
            //    skeleton.GetComponent<AIFieldOfVision>().player2 = pManager.allPlayers[2].transform;
            //    skeleton.GetComponent<AIFieldOfVision>().player3 = pManager.allPlayers[3].transform;*/
            //    skeletonsAlive = skeletonsAlive + 1;
            //    skeletonsLeftToSpawn = skeletonsLeftToSpawn - 1;
            //}
        }

        nextSkeletonSpawnTime = gameTime.totalGameTime + skeletonSpawnDelay;
    }

    void SpawnWatcher()
    {
        if (hasSpaceToSpawnWatcher)
        {
            int i = Random.Range(0, watcherSpawns.Length);
            int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            var newWatcher = watcherPool.SpawnPooledGameObject();
            newWatcher.transform.position = watcherSpawns[i].transform.position;
            newWatcher.transform.rotation = watcherSpawns[i].transform.rotation;
            newWatcher.SetActive(true);
            if (!newWatcher.GetComponent<Watcher>().swarmMode)
                newWatcher.GetComponent<Watcher>().swarmMode = this;
            newWatcher.GetComponent<Watcher>().target = pManager.allPlayers[b].transform;
            watchersAlive++;
            watchersLeftToSpawn--;

            //if (watcherSpawns[i] != null)
            //{
            //    GameObject watcher = Instantiate(watcherPrefab, watcherSpawns[i].gameObject.transform.position, watcherSpawns[i].gameObject.transform.rotation);
            //    watcher.GetComponent<Watcher>().swarmMode = this;
            //    watcher.GetComponent<Watcher>().target = pManager.allPlayers[b].transform;
            //    watchersAlive = watchersAlive + 1;
            //    watchersLeftToSpawn = watchersLeftToSpawn - 1;
            //}
        }

        nextWatcherSpawnTime = gameTime.totalGameTime + watcherSpawnDelay;
    }

    void SpawnHellhound()
    {
        if (hasSpaceToSpawnHellhound)
        {
            int i = Random.Range(0, hellhoundSpawns.Length);
            int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            var newHound = hellhoundPool.SpawnPooledGameObject();
            newHound.transform.position = hellhoundSpawns[i].transform.position;
            newHound.transform.rotation = hellhoundSpawns[i].transform.rotation;
            newHound.SetActive(true);
            if (!newHound.GetComponent<Hellhound>().swarmMode)
                newHound.GetComponent<Hellhound>().swarmMode = this;
            newHound.GetComponent<Hellhound>().target = pManager.allPlayers[b].transform;
            hellhoundsAlive++;
            hellhoundsLeftToSpawn--;

            //if (hasSpaceToSpawnHellhound)
            //{
            //    int i = Random.Range(0, hellhoundSpawns.Length);
            //    int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            //    if (hellhoundSpawns[i] != null)
            //    {
            //        GameObject hellhound = Instantiate(hellhoundPrefab, hellhoundSpawns[i].gameObject.transform.position, hellhoundSpawns[i].gameObject.transform.rotation);
            //        hellhound.GetComponent<Hellhound>().swarmMode = this;
            //        hellhound.GetComponent<Hellhound>().target = pManager.allPlayers[b].transform;
            //        hellhoundsAlive = hellhoundsAlive + 1;
            //        hellhoundsLeftToSpawn = hellhoundsLeftToSpawn - 1;
            //    }
            //}
        }
        nextHellhoundSpawnTime = gameTime.totalGameTime + hellhoundSpawnDelay;
    }

    void SpawnTroll()
    {
        if (hasSpaceToSpawnTroll)
        {
            int i = Random.Range(0, trollSpawns.Length);
            int b = Random.Range(0, pManager.GetComponent<SplitScreenManager>().numberOfPlayers);

            var newTroll = trollPool.SpawnPooledGameObject();
            newTroll.transform.position = trollSpawns[i].transform.position;
            newTroll.transform.rotation = trollSpawns[i].transform.rotation;
            newTroll.SetActive(true);
            if (!newTroll.GetComponent<Troll>().swarmMode)
                newTroll.GetComponent<Troll>().swarmMode = this;
            newTroll.GetComponent<Troll>().target = pManager.allPlayers[b].transform;
            trollsAlive++;
            trollsLeftToSpawn--;

            //if (trollSpawns[i] != null)
            //{
            //    GameObject troll = Instantiate(trollPrefab, trollSpawns[i].gameObject.transform.position, trollSpawns[i].gameObject.transform.rotation);
            //    troll.GetComponent<Troll>().swarmMode = this;
            //    troll.GetComponent<Troll>().target = pManager.allPlayers[b].transform;
            //    trollsAlive = trollsAlive + 1;
            //    trollsLeftToSpawn = trollsLeftToSpawn - 1;
            //}
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

    IEnumerator WaveEnd()
    {
        yield return new WaitForSeconds(newWaveDelay);
        Debug.Log("Reinforcements (Voice)");
        StartCoroutine(UpdateWaveNumber(waveNumber));
    }

    IEnumerator PlayAmbientSound()
    {
        if (waveNumber == 0)
        {            
            int randomSound = Random.Range(0, ambientMusics.Length -1 );
            audioSource.clip = ambientMusics[randomSound];
            audioSource.Play();
        }

        if (waveNumber % 5 != 0 || waveNumber % 10 != 0)
        {
            int randomSound = Random.Range(0, ambientMusics.Length - 1);
            audioSource.clip = ambientMusics[randomSound];
            audioSource.Play();
        }

        yield return new WaitForSeconds(180f);

        StartCoroutine(PlayAmbientSound());
    }

    void ResetPoints()
    {
        if (player0PointsText != null)
        {
            player0Points = 0;
            player0PointsText.text = player0Points.ToString();
        }

        if (player1PointsText != null)
        {
            player1Points = 0;
            player1PointsText.text = player1Points.ToString();
        }

        if (player2PointsText != null)
        {
            player2Points = 0;
            player2PointsText.text = player2Points.ToString();
        }

        if (player3PointsText != null)
        {
            player3Points = 0;
            player3PointsText.text = player3Points.ToString();
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
        foreach (GameObject player in pManager.allPlayers)
        {
            if (player)
            {
                if (player.activeSelf)
                {
                    //Debug.Log("Player is Active");
                    player.GetComponent<PlayerProperties>().playerLivesText.text = playerLives.ToString();
                }
            }
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

    void GivePlayersThis()
    {
        foreach (GameObject player in pManager.allPlayers)
        {
            if (player.activeSelf)
            {
                player.GetComponent<PlayerProperties>().swarmMode = this;
            }
        }
    }
}
