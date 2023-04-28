using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManagerInstance;
    public List<SpawnPoint> genericSpawnPoints = new List<SpawnPoint>();
    void Awake()
    {
        int c = 0;
        if (genericSpawnPoints.Count == 0)
            foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
            {
                sp.name = $"Spawn point {c}";
                c++;

                genericSpawnPoints.Add(sp);
            }

        spawnManagerInstance = this;
    }

    public Transform GetRandomComputerSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPoints)
            if (sp.spawnPointType == SpawnPoint.SpawnPointType.Computer)
                availableSpawnPoints.Add(sp);

        int ran = Random.Range(0, availableSpawnPoints.Count);

        return availableSpawnPoints[ran].transform;
    }

    Transform GetRandomSpawnpoint(int controllerId)
    {
        try { GameManager.GetLocalPlayer(controllerId).GetComponent<KillFeedManager>().EnterNewFeed($"Spawning randomly"); } catch { }
        return genericSpawnPoints[Random.Range(0, genericSpawnPoints.Count)].transform;
    }

    public Transform GetRandomSafeSpawnPoint(int controllerId = 0)
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPoints)
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                if (sp.players.Count == 0)
                    availableSpawnPoints.Add(sp);
            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                if (sp.spawnPointType == SpawnPoint.SpawnPointType.Player)
                    availableSpawnPoints.Add(sp);

        if (availableSpawnPoints.Count > 0)
        {
            int ran = Random.Range(0, availableSpawnPoints.Count);

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                if (availableSpawnPoints[ran].players.Count == 0)
                {
                    //try { GameManager.GetMyPlayer(controllerId).GetComponent<KillFeedManager>().EnterNewFeed($"Spawn point: {availableSpawnPoints[ran].name}({availableSpawnPoints[ran].transform.position})"); } catch { }
                    Debug.Log($"Spawn point: {availableSpawnPoints[ran].name}({availableSpawnPoints[ran].transform})");
                    return availableSpawnPoints[ran].transform;
                }
            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                return availableSpawnPoints[ran].transform;

        }

        return GetRandomSpawnpoint(controllerId);
    }
}
