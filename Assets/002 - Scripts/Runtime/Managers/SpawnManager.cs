using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManagerInstance;
    public List<SpawnPoint> initialSpawnPoints = new List<SpawnPoint>();
    public List<SpawnPoint> genericSpawnPointsAlpha = new List<SpawnPoint>();
    public List<SpawnPoint> genericSpawnPointsBeta = new List<SpawnPoint>();

    public OddballSpawnPoint oddballSpawnPoint;
    void Awake()
    {
        int c = 0;
        if (genericSpawnPointsAlpha.Count == 0)
        {
            foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
            {
                sp.name = $"Spawn point {c}";
                c++;

                if (sp.layer == SpawnPoint.Layer.Alpha)
                    genericSpawnPointsAlpha.Add(sp);
                else if (sp.layer == SpawnPoint.Layer.Beta)
                    genericSpawnPointsBeta.Add(sp);
            }
        }


        spawnManagerInstance = this;
    }

    private void Start()
    {
        oddballSpawnPoint = FindObjectOfType<OddballSpawnPoint>();
    }

    public Transform GetRandomComputerSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPointsAlpha)
            if (sp.spawnPointType == SpawnPoint.SpawnPointType.Computer)
                availableSpawnPoints.Add(sp);

        int ran = Random.Range(0, availableSpawnPoints.Count);

        return availableSpawnPoints[ran].transform;
    }

    Transform GetRandomSpawnpoint(int controllerId)
    {
        if (CurrentRoomManager.instance.gameStarted)
            try { GameManager.GetLocalPlayer(controllerId).GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>Spawning randomly"); } catch { }
        return genericSpawnPointsAlpha[Random.Range(0, genericSpawnPointsAlpha.Count)].transform;
    }

    public Transform GetRandomSafeSpawnPoint(int controllerId = 0)
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPointsAlpha)
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                if (!sp.occupied)
                    availableSpawnPoints.Add(sp);


                if (availableSpawnPoints.Count == 0)
                {
                    foreach (SpawnPoint spb in genericSpawnPointsBeta)
                    {
                        if (!spb.occupied)
                            availableSpawnPoints.Add(sp);
                    }
                }


                if (availableSpawnPoints.Count == 0)
                {
                    foreach (SpawnPoint spb in genericSpawnPointsAlpha)
                    {
                        if (!spb.seen)
                            availableSpawnPoints.Add(sp);
                    }
                }

            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                if (sp.spawnPointType == SpawnPoint.SpawnPointType.Player)
                    availableSpawnPoints.Add(sp);





        if (availableSpawnPoints.Count > 0)
        {
            int ran = Random.Range(0, availableSpawnPoints.Count);

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                if (!availableSpawnPoints[ran].occupied)
                {
                    //try { GameManager.GetMyPlayer(controllerId).GetComponent<KillFeedManager>().EnterNewFeed($"Spawn point: {availableSpawnPoints[ran].name}({availableSpawnPoints[ran].transform.position})"); } catch { }
                    return availableSpawnPoints[ran].transform;
                }
            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                return availableSpawnPoints[ran].transform;

        }

        return GetRandomSpawnpoint(controllerId);
    }
}
