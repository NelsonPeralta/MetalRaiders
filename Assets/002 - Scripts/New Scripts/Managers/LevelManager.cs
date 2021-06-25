using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject playerPrefab;
    public GameObject zombiePrefab;
    public ChildManager cManager;

    [Header("Game Type")]
    public bool Deathmatch = false;
    public bool Plague = false;

    [Header("Normal Zombie Spawns")]
    public GameObject[] redPlayers;
    public GameObject[] bluePlayers;
    public GameObject[] genericPlayersSpawns;
    public GameObject[] couchPlayersSpawns;    

    public GameObject[] normalsZombieSpawns;
    public GameObject[] normalZombies = new GameObject[36];

    public GameObject[] allPlayers;

    public int amountOfPlayers = 1;

    public int allPlayersCount;
    public int couchPlayersCount = 0;
    public int normalZombiesCount = 0;

    int Wave;
    int maxZombiesInPresentWave;

    int maxNormalZombieSpawnPoints = 0;
    int randomZombieSpawnChooser;

    bool hasFoundComponents = false;

    void Start()
    {
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        allPlayersCount = allPlayers.Length;

        cManager = GetComponent<ChildManager>();


        for (int i = 0; i < allPlayersCount; i++)
        {
            allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>().SetPlayerIDInInput();

            allPlayers[i].gameObject.GetComponent<Movement>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponent<Movement>().SetPlayerIDInInput();

            allPlayers[i].gameObject.GetComponent<PlayerController>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponent<PlayerController>().SetPlayerIDInInput();


            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().playerRewiredID = i;

            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().notMyFPSController = allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>();

            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().movement = allPlayers[i].gameObject.GetComponent<Movement>();

        }

        if (!hasFoundComponents)
        {
            FindSpawnPoints();
        }

        Debug.Log("Game Not Started Yet");
        GameStart();
        Debug.Log("Game Started");
    }

    void FindPlayers()
    {
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        allPlayersCount = allPlayers.Length;


        for(int i = 0; i < allPlayersCount; i++)
        {
            allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().playerRewiredID = i;

            allPlayers[i].gameObject.GetComponent<PlayerController>().playerRewiredID = i;

            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().notMyFPSController = allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>();

        }
    }

    void FindSpawnPoints()
    {
        int normalZombieCounter = 0;

        for(int i = 0; i < cManager.allChildren.Count; i++)
        {
            //Debug.Log(i);
        }

        foreach(GameObject child in cManager.allChildren)
        {
            if(child.gameObject.tag == "Spawn Point")
            {
                Debug.Log("Found Spawn Point");
                if(child.gameObject.GetComponent<SpawnPoint>().normalZombie)
                {
                    normalsZombieSpawns[normalZombieCounter] = child.gameObject;
                    normalZombieCounter = normalZombieCounter + 1;
                }
            }
        }
    }

    void GameStart()
    {
        if (Plague)
        {
            Debug.Log("Plague");
            StartCoroutine(SpawnZombies());
            WaveStart();
        }
    }

    IEnumerator SpawnZombies()
    {
        yield return new WaitForSeconds(2);

        for (int i = 0; i < normalsZombieSpawns.Length; i++)
        {
            if (normalsZombieSpawns[i] != null)
            {
                Debug.Log("Zombie is going to Spawn");                
                normalZombies[i] = Instantiate(zombiePrefab, normalsZombieSpawns[i].gameObject.transform.position, normalsZombieSpawns[i].gameObject.transform.rotation);
                normalZombies[i].gameObject.GetComponent<ZombieScript>().target = allPlayers[0].transform;
                Debug.Log("Zombie Spawned");
            }
            yield return new WaitForSeconds(2);
        }
    }

    private void FixedUpdate()
    {

    }

    void WaveStart()
    {
        
    }

    void CalculateMaxZombiesInWave()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator NormalZombieSpawnTimer()
    {
        yield return new WaitForSeconds(2);
    }

    void CalculateZombieCountForRound()
    {
        int maxAmountOfZombiesOnTheMap = 4;
    }

}
