using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        oddballSpawnPoint = FindObjectOfType<OddballSpawnPoint>();
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

        int ran = Random.Range(0, availableSpawnPoints.Count);

        return availableSpawnPoints[ran].transform;
    }

    Transform GetCompletelyRandomSpawnpoint()
    {
        Log.Print($"GetCompletelyRandomSpawnpoint");
        return genericSpawnPoints[Random.Range(0, genericSpawnPoints.Count)].transform;
    }

    public (Transform, bool) GetRandomSafeSpawnPoint(GameManager.Team team) // return a position and if the spawn is random or not
    {
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
            int ran = Random.Range(0, availableSpawnPoints.Count);
            Log.Print($"Returning {availableSpawnPoints[ran].name} spawn (danger level {availableSpawnPoints[ran].blockingLevel})");
            return (availableSpawnPoints[ran].transform, false);
        }

        return (GetCompletelyRandomSpawnpoint(), true);
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
