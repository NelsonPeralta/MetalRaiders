using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class AIPool : MonoBehaviour
{
    [Header("AIs")]
    public int amountToPool;
    public List<Skeleton> skeletons = new List<Skeleton>();
    public GameObject skeletonPrefab;
    public List<ZombieScript> zombies = new List<ZombieScript>();
    public GameObject zombiePrefab;
    private void Start()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", zombiePrefab.name), Vector3.zero, Quaternion.identity);
            //GameObject obj = Instantiate(zombiePrefab, transform.position, transform.rotation);
            obj.gameObject.SetActive(false);
            zombies.Add(obj.GetComponent<ZombieScript>());
            obj.transform.parent = gameObject.transform;

            //GameObject obj = Instantiate(skeletonPrefab, transform.position, transform.rotation);
            //obj.gameObject.SetActive(false);
            //skeletons.Add(obj.GetComponent<Skeleton>());
            //obj.transform.parent = gameObject.transform;
        }
    }
    public Skeleton GetPooledSkeleton()
    {
        foreach (Skeleton obj in skeletons)
            if (!obj.gameObject.activeSelf)
                return obj;
        return null;
    }

    public ZombieScript GetPooledZombie()
    {
        foreach (ZombieScript obj in zombies)
            if (!obj.gameObject.activeSelf)
                return obj;
        return null;
    }
}
