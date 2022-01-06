using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnPointType { Player, Computer}
    public SpawnPointType spawnPointType;

    public int spawnNumber;

    [Header("Type of Spawn")]
    public bool player;
    public bool normalZombie;

    public bool hasRecentlySpawnedSomething = false;
}
