using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawnManager : MonoBehaviour
{
    public static AISpawnManager aISpawnManagerInstance;
    public List<SpawnPoint> genericSpawnPoints;
    void Awake()
    {
        if (genericSpawnPoints.Count == 0)
            foreach (Transform child in transform)
                genericSpawnPoints.Add(child.GetComponent<SpawnPoint>());

        aISpawnManagerInstance = this;
    }

    public Transform GetGenericSpawnpoint()
    {
        return genericSpawnPoints[Random.Range(0, genericSpawnPoints.Count)].transform;
    }
}
