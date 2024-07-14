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
            foreach (SpawnPoint sp in GetComponentsInChildren<SpawnPoint>(false))
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

    public Transform GetSpawnPointAtIndex(int i)
    {
        return genericSpawnPointsAlpha[i].transform;
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

    Transform GetRandomSpawnpoint()
    {
        return genericSpawnPointsAlpha[Random.Range(0, genericSpawnPointsAlpha.Count)].transform;
    }

    public (Transform, bool) GetRandomSafeSpawnPoint() // return a position and if the spawn is random or not
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
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



        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            availableSpawnPoints.AddRange(genericSpawnPointsAlpha);



        if (availableSpawnPoints.Count > 0)
        {
            int ran = Random.Range(0, availableSpawnPoints.Count);
            print($"Returning {availableSpawnPoints[ran].name} spawn");
            return (availableSpawnPoints[ran].transform, false);
        }

        return (GetRandomSpawnpoint(), true);
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
    }
}
