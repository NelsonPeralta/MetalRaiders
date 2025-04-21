using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManagerInstance;
    public List<SpawnPoint> initialSpawnPoints = new List<SpawnPoint>();
    public List<SpawnPoint> genericSpawnPointsAlpha = new List<SpawnPoint>();
    public List<SpawnPoint> genericSpawnPointsBeta = new List<SpawnPoint>();

    public List<SpawnPoint> redSpawnPoints = new List<SpawnPoint>();
    public List<SpawnPoint> blueSpawnPoints = new List<SpawnPoint>();



    public OddballSpawnPoint oddballSpawnPoint;
    void Awake()
    {
        int c = 0;
        if (genericSpawnPointsAlpha.Count == 0)
        {
            foreach (SpawnPoint sp in GetComponentsInChildren<SpawnPoint>(false))
            {
                sp.name = $"Spawn point {c}";
                c++;

                if (sp.layer == SpawnPoint.Layer.Alpha && sp.team == GameManager.Team.None)
                    genericSpawnPointsAlpha.Add(sp);
                else if (sp.layer == SpawnPoint.Layer.Beta && sp.team == GameManager.Team.None)
                    genericSpawnPointsBeta.Add(sp);

                if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    if (sp.layer == SpawnPoint.Layer.Beta && sp.team == GameManager.Team.Red)
                        redSpawnPoints.Add(sp);
                    else if (sp.layer == SpawnPoint.Layer.Beta && sp.team == GameManager.Team.Blue)
                        blueSpawnPoints.Add(sp);
                }
            }
        }


        spawnManagerInstance = this;
    }

    private void Start()
    {
        oddballSpawnPoint = FindObjectOfType<OddballSpawnPoint>();
    }

    public Transform GetSpawnPointAtIndex(int i, GameManager.Team team)
    {
        if (team == GameManager.Team.None)
            return genericSpawnPointsAlpha[i].transform;
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
                return genericSpawnPointsAlpha[i].transform;
            }
        }

        return null;
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

    Transform GetCompletelyRandomSpawnpoint()
    {
        print($"GetCompletelyRandomSpawnpoint");
        return genericSpawnPointsAlpha[Random.Range(0, genericSpawnPointsAlpha.Count)].transform;
    }

    public (Transform, bool) GetRandomSafeSpawnPoint(GameManager.Team team) // return a position and if the spawn is random or not
    {
        print($"GetRandomSafeSpawnPoint {team} {blueSpawnPoints.Count}");

        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {
                foreach (SpawnPoint sp in genericSpawnPointsAlpha)
                    if (!sp.constested && !sp.reserved)
                        availableSpawnPoints.Add(sp);


                if (availableSpawnPoints.Count == 0)
                    foreach (SpawnPoint spb in genericSpawnPointsAlpha)
                        if (!spb.seen && !spb.reserved)
                            availableSpawnPoints.Add(spb);


                if (availableSpawnPoints.Count == 0)
                    foreach (SpawnPoint spb in genericSpawnPointsAlpha)
                        if (!spb.reserved)
                            availableSpawnPoints.Add(spb);

                if (availableSpawnPoints.Count == 0)
                    foreach (SpawnPoint spb in genericSpawnPointsAlpha)
                        if (!spb.seen)
                            availableSpawnPoints.Add(spb);


                if (availableSpawnPoints.Count == 0)
                    foreach (SpawnPoint sp in genericSpawnPointsBeta)
                        if (!sp.constested && !sp.reserved)
                            availableSpawnPoints.Add(sp);
            }
            else
            {
                if (team == GameManager.Team.Red)
                {
                    foreach (SpawnPoint sp in redSpawnPoints)
                        if (!sp.constested && !sp.reserved)
                            availableSpawnPoints.Add(sp);


                    if (availableSpawnPoints.Count == 0)
                        foreach (SpawnPoint spb in redSpawnPoints)
                            if (!spb.reserved && !spb.seen)
                                availableSpawnPoints.Add(spb);

                    if (availableSpawnPoints.Count == 0)
                        availableSpawnPoints = new List<SpawnPoint>(redSpawnPoints);
                }
                else
                {
                    foreach (SpawnPoint sp in blueSpawnPoints)
                        if (!sp.constested && !sp.reserved)
                            availableSpawnPoints.Add(sp);

                    if (availableSpawnPoints.Count == 0)
                        foreach (SpawnPoint spb in blueSpawnPoints)
                            if (!spb.reserved && !spb.seen)
                                availableSpawnPoints.Add(spb);

                    if (availableSpawnPoints.Count == 0)
                        availableSpawnPoints = new List<SpawnPoint>(blueSpawnPoints);
                }
            }
        }



        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            availableSpawnPoints.Clear();
            availableSpawnPoints.AddRange(genericSpawnPointsAlpha);
        }



        if (availableSpawnPoints.Count > 0)
        {
            int ran = Random.Range(0, availableSpawnPoints.Count);
            print($"Returning {availableSpawnPoints[ran].name} spawn");
            return (availableSpawnPoints[ran].transform, false);
        }

        return (GetCompletelyRandomSpawnpoint(), true);
    }

    public Transform GetSpawnPointAtPos(Vector3 p)
    {
        foreach (SpawnPoint sp in genericSpawnPointsAlpha)
        {
            if (sp.transform.position == p)
            {
                print($"Returning spawn point {sp.name}");
                return sp.transform;
            }
        }


        foreach (SpawnPoint sp in redSpawnPoints)
        {
            if (sp.transform.position == p)
            {
                print($"Returning spawn point {sp.name}");
                return sp.transform;
            }
        }

        foreach (SpawnPoint sp in blueSpawnPoints)
        {
            if (sp.transform.position == p)
            {
                print($"Returning spawn point {sp.name}");
                return sp.transform;
            }
        }

        return null;
    }

    public void ReserveSpawnPoint(Vector3 p)
    {
        foreach (SpawnPoint sp in genericSpawnPointsAlpha)
            if (sp.transform.position == p)
                sp.reserved = true;

        foreach (SpawnPoint sp in genericSpawnPointsBeta)
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
        print($"oneobjmode - RotateSpawns");

        List<SpawnPoint> _tempSplRed = new List<SpawnPoint>(redSpawnPoints);
        List<SpawnPoint> _tempSplBlue = new List<SpawnPoint>(blueSpawnPoints);

        redSpawnPoints.Clear(); blueSpawnPoints.Clear();

        redSpawnPoints = _tempSplBlue;
        blueSpawnPoints = _tempSplRed;
    }
}
