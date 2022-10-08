using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManagerInstance;
    public List<SpawnPoint> genericSpawnPoints;
    void Awake()
    {
        int c = 0;
        if (genericSpawnPoints.Count == 0)
            foreach (SpawnPoint sp in GetComponentsInChildren<SpawnPoint>())
            {
                sp.name = $"Spawn point {c}";
                c++;

                genericSpawnPoints.Add(sp);
            }

        spawnManagerInstance = this;
    }

    Transform GetRandomSpawnpoint()
    {
        try { GameManager.GetMyPlayer().GetComponent<KillFeedManager>().EnterNewFeed($"Spwaning randomly"); } catch { }
        return genericSpawnPoints[Random.Range(0, genericSpawnPoints.Count)].transform;
    }

    public Transform GetRandomSafeSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPoints)
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                if (sp.players.Count == 0)
                    availableSpawnPoints.Add(sp);

        if (availableSpawnPoints.Count > 0)
        {
            int ran = Random.Range(0, availableSpawnPoints.Count);

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                if (availableSpawnPoints[ran].players.Count == 0)
                {
                    try { GameManager.GetMyPlayer().GetComponent<KillFeedManager>().EnterNewFeed($"Spawn point: {availableSpawnPoints[ran].name}({availableSpawnPoints[ran].transform.position})"); } catch { }
                    Debug.Log($"Spawn point: {availableSpawnPoints[ran].name}({availableSpawnPoints[ran].transform})");
                    return availableSpawnPoints[ran].transform;
                }
        }

        return GetRandomSpawnpoint();
    }
}
