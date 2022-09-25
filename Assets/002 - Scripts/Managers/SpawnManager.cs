using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManagerInstance;
    public List<SpawnPoint> genericSpawnPoints;
    void Awake()
    {
        if (genericSpawnPoints.Count == 0)

            foreach (SpawnPoint sp in GetComponentsInChildren<SpawnPoint>())
                genericSpawnPoints.Add(sp);

        spawnManagerInstance = this;
    }

    Transform GetRandomSpawnpoint()
    {
        return genericSpawnPoints[Random.Range(0, genericSpawnPoints.Count)].transform;
    }

    public Transform GetRandomSafeSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPoints)
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                if (GameManager.instance.gameType == GameManager.GameType.Slayer)
                    if (sp.players.Count == 0)
                        availableSpawnPoints.Add(sp);

        if (availableSpawnPoints.Count > 0)
        {
            int ran = Random.Range(0, availableSpawnPoints.Count);

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                if (GameManager.instance.gameType == GameManager.GameType.Slayer)
                    if (availableSpawnPoints[ran].players.Count == 0)
                    {
                        Debug.Log($"Returning Safe Spawn Point: {availableSpawnPoints[ran].transform}");
                        return availableSpawnPoints[ran].transform;
                    }
        }

        return GetRandomSpawnpoint();
    }
}
