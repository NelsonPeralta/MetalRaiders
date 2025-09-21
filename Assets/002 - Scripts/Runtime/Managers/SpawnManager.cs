using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManagerInstance;
    public List<SpawnPoint> initialSpawnPoints = new List<SpawnPoint>();
    public List<SpawnPoint> genericSpawnPoints = new List<SpawnPoint>();

    public List<SpawnPoint> redSpawnPoints = new List<SpawnPoint>();
    public List<SpawnPoint> blueSpawnPoints = new List<SpawnPoint>();

    public OddballSpawnPoint oddballSpawnPoint;


    List<SpawnPoint> _tempListOfSpawns = new List<SpawnPoint>();

    int _tempDangerLevel;


    void Awake()
    {
        int c = 0;
        if (genericSpawnPoints.Count == 0)
        {
            foreach (SpawnPoint sp in GetComponentsInChildren<SpawnPoint>(false))
            {
                sp.name = $"Spawn point {c}";
                c++;

                if (sp.team == GameManager.Team.None)
                    genericSpawnPoints.Add(sp);

                if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    if (sp.team == GameManager.Team.Red)
                        redSpawnPoints.Add(sp);
                    else if (sp.team == GameManager.Team.Blue)
                        blueSpawnPoints.Add(sp);
                }
            }
        }


        spawnManagerInstance = this;
    }

    private void OnDestroy()
    {
        spawnManagerInstance = null;
    }

    private void Start()
    {
        _udpateSpawnPointsData = 0.1f;
        oddballSpawnPoint = FindObjectOfType<OddballSpawnPoint>();
    }



    // Class-level reusable list



    float _udpateSpawnPointsData;
    private void Update()
    {
        if (!CurrentRoomManager.instance.gameStarted || CurrentRoomManager.instance.gameOver)
            return;

        if (_udpateSpawnPointsData > 0)
        {
            _udpateSpawnPointsData -= Time.deltaTime;

            if (_udpateSpawnPointsData <= 0f)
            {
                List<SpawnPoint> spawnPoints = null;

                if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                {
                    spawnPoints = spawnManagerInstance.genericSpawnPoints;
                }
                else
                {
                    _tempListOfSpawns.Clear();
                    _tempListOfSpawns.AddRange(spawnManagerInstance.redSpawnPoints);
                    _tempListOfSpawns.AddRange(spawnManagerInstance.blueSpawnPoints);
                    spawnPoints = _tempListOfSpawns;
                }





                var players = GameManager.instance.GetAllPhotonPlayers();

                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    for (int j = 0; j < players.Count; j++)
                    {
                        var spawnPos = spawnPoints[i].transform.position;
                        var player = players[j];
                        var playerPos = player.transform.position;

                        if (player.isDead) continue;

                        float sqrDist = (spawnPos - playerPos).sqrMagnitude;

                        if (sqrDist > 42f * 42f)
                            continue;

                        int level;
                        if (sqrDist > 35f * 35f)
                            level = GetBlockingLevel(player, spawnPos, 13, 11, 13);
                        else if (sqrDist > 30f * 30f)
                            level = GetBlockingLevel(player, spawnPos, 14, 12, 14);
                        else if (sqrDist > 25f * 25f)
                            level = GetBlockingLevel(player, spawnPos, 15, 13, 15);
                        else if (sqrDist > 20f * 20f)
                            level = GetBlockingLevel(player, spawnPos, 16, 14, 16);
                        else if (sqrDist > 15f * 15f)
                            level = GetBlockingLevel(player, spawnPos, 17, 15, 17);
                        else if (sqrDist > 10f * 10f)
                            level = GetBlockingLevel(player, spawnPos, 18, 16, 18);
                        else
                            level = GetBlockingLevel(player, spawnPos, 19, 17, 19);

                        if (level == 0) continue;

                        int id = (i + 1) + ((j + 1) * 1000);
                        //Debug.Log($"GetBlockingLevel {spawnPoints[i].name} {Mathf.Abs(spawnPos.y - playerPos.y)}");
                        spawnPoints[i].AddBlockingLevelEntry(id, level, 0.5f);
                    }
                }

                _udpateSpawnPointsData = 0.1f;
            }
        }
    }

    private int GetBlockingLevel(Player player, Vector3 spawnPos, int front, int side, int back)
    {
        Vector3 playerPos = player.transform.position;
        Vector3 toSpawn = (spawnPos - playerPos).normalized;
        float dot = Vector3.Dot(player.transform.forward, toSpawn);

        float yDiff = Mathf.Abs(spawnPos.y - playerPos.y);
        int verticalFact = yDiff < 2f ? 0 : Mathf.Clamp((int)Mathf.Floor(yDiff) - 2, 0, 99);
        //Debug.Log($"GetBlockingLevel {verticalFact}");

        if (dot > 0.866f) return Mathf.Clamp(front - verticalFact, 0, 99);   // Front ±30°
        else if (dot > 0f) return Mathf.Clamp(side - verticalFact, 0, 99);   // Sides
        else return Mathf.Clamp(back - verticalFact, 0, 99);                 // Back
    }

    private int GetBlockingLevel(Transform player, Vector3 spawnPos, int front, int side, int back)
    {
        Vector3 playerPos = player.transform.position;
        Vector3 toSpawn = (spawnPos - playerPos).normalized;
        float dot = Vector3.Dot(player.transform.forward, toSpawn);

        float yDiff = Mathf.Abs(spawnPos.y - playerPos.y);
        int verticalFact = yDiff < 2f ? 0 : Mathf.Clamp((int)Mathf.Floor(yDiff) - 2, 0, 99);
        //Debug.Log($"GetBlockingLevel {verticalFact}");

        if (dot > 0.866f) return Mathf.Clamp(front - verticalFact, 0, 99);   // Front ±30°
        else if (dot > 0f) return Mathf.Clamp(side - verticalFact, 0, 99);   // Sides
        else return Mathf.Clamp(back - verticalFact, 0, 99);                 // Back
    }


    public Transform GetSpawnPointAtIndex(int i, GameManager.Team team)
    {
        if (team == GameManager.Team.None)
            return genericSpawnPoints[i].transform;
        else
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
            {
                if (team == GameManager.Team.Red)
                {
                    return redSpawnPoints[i].transform;
                }
                else
                {
                    return blueSpawnPoints[i].transform;
                }
            }
            else
            {
                return genericSpawnPoints[i].transform;
            }
        }

        return null;
    }

    public Transform GetRandomComputerSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint sp in genericSpawnPoints)
            if (sp.spawnPointType == SpawnPoint.SpawnPointType.Computer)
                availableSpawnPoints.Add(sp);

        int ran = UnityEngine.Random.Range(0, availableSpawnPoints.Count);

        return availableSpawnPoints[ran].transform;
    }

    Transform GetCompletelyRandomSpawnpoint()
    {
        Log.Print($"GetCompletelyRandomSpawnpoint");
        return genericSpawnPoints[UnityEngine.Random.Range(0, genericSpawnPoints.Count)].transform;
    }

    public (Transform, bool) GetRandomSafeSpawnPoint(GameManager.Team team) // return a position and if the spawn is random or not
    {
        (Transform, bool) finalRes = (GetCompletelyRandomSpawnpoint(), true);
        Log.Print($"GetRandomSafeSpawnPoint {team} {blueSpawnPoints.Count}");

        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {

                // first check: non contested and non reserved
                _tempListOfSpawns = new List<SpawnPoint>(genericSpawnPoints);
                _tempListOfSpawns = _tempListOfSpawns.Where(x => !x.constested && !x.reserved).ToList();

                if (_tempListOfSpawns.Count > 0)
                {
                    for (int i = 0; i < _tempListOfSpawns.Count; i++)
                    {
                        Log.Print($"SpawnManager F0 {i}/{_tempListOfSpawns.Count} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                    }

                    _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                    _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                    _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                    availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                    for (int i = 0; i < _tempListOfSpawns.Count; i++)
                    {
                        Log.Print($"SpawnManager F1 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                    }
                }
                else
                {
                    // second check: non reserved

                    _tempListOfSpawns = new List<SpawnPoint>(genericSpawnPoints);
                    _tempListOfSpawns = _tempListOfSpawns.Where(x => !x.reserved).ToList();

                    if (_tempListOfSpawns.Count > 0)
                    {
                        _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                        _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                        _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                        availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                        for (int i = 0; i < _tempListOfSpawns.Count; i++)
                        {
                            Log.Print($"SpawnManager F2 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                        }
                    }
                    else
                    {
                        // third check: lowest danger level

                        _tempListOfSpawns = new List<SpawnPoint>(genericSpawnPoints);

                        if (_tempListOfSpawns.Count > 0)
                        {
                            _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                            _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                            _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                            availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                            for (int i = 0; i < _tempListOfSpawns.Count; i++)
                            {
                                Log.Print($"SpawnManager F3 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                            }
                        }
                    }
                }
            }
            else
            {
                if (team == GameManager.Team.Red)
                {
                    // first check: non contested and non reserved
                    _tempListOfSpawns = new List<SpawnPoint>(redSpawnPoints);
                    _tempListOfSpawns = _tempListOfSpawns.Where(x => !x.constested && !x.reserved).ToList();

                    if (_tempListOfSpawns.Count > 0)
                    {
                        _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                        _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                        _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                        availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                        for (int i = 0; i < _tempListOfSpawns.Count; i++)
                        {
                            Log.Print($"SpawnManager F1 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                        }
                    }
                    else
                    {
                        // second check: non reserved

                        _tempListOfSpawns = new List<SpawnPoint>(redSpawnPoints);
                        _tempListOfSpawns = _tempListOfSpawns.Where(x => !x.reserved).ToList();

                        if (_tempListOfSpawns.Count > 0)
                        {
                            _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                            _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                            _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                            availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                            for (int i = 0; i < _tempListOfSpawns.Count; i++)
                            {
                                Log.Print($"SpawnManager F2 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                            }
                        }
                        else
                        {
                            // third check: lowest danger level

                            _tempListOfSpawns = new List<SpawnPoint>(redSpawnPoints);

                            if (_tempListOfSpawns.Count > 0)
                            {
                                _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                                _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                                _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                                availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                                for (int i = 0; i < _tempListOfSpawns.Count; i++)
                                {
                                    Log.Print($"SpawnManager F3 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                                }
                            }
                        }
                    }

                    if (availableSpawnPoints.Count == 0)
                        availableSpawnPoints = new List<SpawnPoint>(redSpawnPoints);
                }
                else
                {
                    // first check: non contested and non reserved
                    _tempListOfSpawns = new List<SpawnPoint>(blueSpawnPoints);
                    _tempListOfSpawns = _tempListOfSpawns.Where(x => !x.constested && !x.reserved).ToList();

                    if (_tempListOfSpawns.Count > 0)
                    {
                        _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                        _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                        _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                        availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                        for (int i = 0; i < _tempListOfSpawns.Count; i++)
                        {
                            Log.Print($"SpawnManager F1 {i} {_tempListOfSpawns[i].blockingLevel}");
                        }
                    }
                    else
                    {
                        // second check: non reserved

                        _tempListOfSpawns = new List<SpawnPoint>(blueSpawnPoints);
                        _tempListOfSpawns = _tempListOfSpawns.Where(x => !x.reserved).ToList();

                        if (_tempListOfSpawns.Count > 0)
                        {
                            _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                            _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                            _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                            availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                            for (int i = 0; i < _tempListOfSpawns.Count; i++)
                            {
                                Log.Print($"SpawnManager F2 {i} {_tempListOfSpawns[i].blockingLevel}");
                            }
                        }
                        else
                        {
                            // third check: lowest danger level

                            _tempListOfSpawns = new List<SpawnPoint>(blueSpawnPoints);

                            if (_tempListOfSpawns.Count > 0)
                            {
                                _tempListOfSpawns.Sort((hit1, hit2) => hit1.blockingLevel.CompareTo(hit2.blockingLevel)); // this should sort by danger level. Smallest index = lowest
                                _tempDangerLevel = _tempListOfSpawns[0].blockingLevel;
                                _tempListOfSpawns = _tempListOfSpawns.Where(x => x.blockingLevel == _tempDangerLevel).ToList();
                                availableSpawnPoints = new List<SpawnPoint>(_tempListOfSpawns);

                                for (int i = 0; i < _tempListOfSpawns.Count; i++)
                                {
                                    Log.Print($"SpawnManager F3 {i} {_tempListOfSpawns[i].name} {_tempListOfSpawns[i].blockingLevel}");
                                }
                            }
                        }
                    }

                    if (availableSpawnPoints.Count == 0)
                        availableSpawnPoints = new List<SpawnPoint>(blueSpawnPoints);
                }
            }
        }



        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            availableSpawnPoints.Clear();
            availableSpawnPoints.AddRange(genericSpawnPoints);
        }



        if (availableSpawnPoints.Count > 0)
        {
            int ran = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            Log.Print($"Returning {availableSpawnPoints[ran].name} spawn (danger level {availableSpawnPoints[ran].blockingLevel})");

            finalRes = (availableSpawnPoints[ran].transform, false);
        }




        int sphereColliderRange = 12;
        Collider[] hitColliders = Physics.OverlapSphere(finalRes.Item1.position, sphereColliderRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Spawn Point"))
            {
                if (hitCollider.gameObject.GetComponent<SpawnPoint>())
                {
                    Debug.Log($"SpawnManager virtual contestion {hitCollider.name}");

                    float sqrDist = (finalRes.Item1.position - hitCollider.transform.position).sqrMagnitude;
                    int level;
                    if (sqrDist > 8f * 8f)
                        level = GetBlockingLevel(finalRes.Item1, hitCollider.transform.position, 10, 10, 10);
                    else if (sqrDist > 5f * 5f)
                        level = GetBlockingLevel(finalRes.Item1, hitCollider.transform.position, 15, 15, 15);
                    else
                        level = GetBlockingLevel(finalRes.Item1, hitCollider.transform.position, 20, 20, 20);
                    if (level == 0) continue;
                    hitCollider.gameObject.GetComponent<SpawnPoint>().AddBlockingLevelEntry(UnityEngine.Random.Range(-100, -200), level, 2);
                }
            }
        }

        return finalRes;
    }

    public Transform GetSpawnPointAtPos(Vector3 p)
    {
        foreach (SpawnPoint sp in genericSpawnPoints)
        {
            if (sp.transform.position == p)
            {
                Log.Print($"Returning spawn point {sp.name}");
                return sp.transform;
            }
        }


        foreach (SpawnPoint sp in redSpawnPoints)
        {
            if (sp.transform.position == p)
            {
                Log.Print($"Returning spawn point {sp.name}");
                return sp.transform;
            }
        }

        foreach (SpawnPoint sp in blueSpawnPoints)
        {
            if (sp.transform.position == p)
            {
                Log.Print($"Returning spawn point {sp.name}");
                return sp.transform;
            }
        }

        return null;
    }

    public void ReserveSpawnPoint(Vector3 p)
    {
        foreach (SpawnPoint sp in genericSpawnPoints)
            if (sp.transform.position == p)
                sp.reserved = true;

        foreach (SpawnPoint sp in redSpawnPoints)
            if (sp.transform.position == p)
                sp.reserved = true;

        foreach (SpawnPoint sp in blueSpawnPoints)
            if (sp.transform.position == p)
                sp.reserved = true;
    }

    public void RotateSpawns()
    {
        Log.Print($"oneobjmode - RotateSpawns");

        List<SpawnPoint> _tempSplRed = new List<SpawnPoint>(redSpawnPoints);
        List<SpawnPoint> _tempSplBlue = new List<SpawnPoint>(blueSpawnPoints);

        redSpawnPoints.Clear(); blueSpawnPoints.Clear();

        redSpawnPoints = _tempSplBlue;
        blueSpawnPoints = _tempSplRed;
    }
}
