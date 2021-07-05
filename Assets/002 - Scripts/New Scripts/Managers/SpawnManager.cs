using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public static SpawnManager Instance;

	public List<SpawnPoint> genericSpawnPoints;

	void Awake()
	{
        if(genericSpawnPoints.Count == 0)
            foreach (Transform child in transform)
                genericSpawnPoints.Add(child.GetComponent<SpawnPoint>());
        
		Instance = this;
	}

	public Transform GetGenericSpawnpoint()
	{
		return genericSpawnPoints[Random.Range(0, genericSpawnPoints.Count)].transform;
	}
}
