using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class AIPool : MonoBehaviour
{
    public PhotonView PV;
    public static AIPool aIPoolInstance;

    [Header("AIs")]
    public int amountToPool;
    public List<Skeleton> skeletons = new List<Skeleton>();
    public GameObject skeletonPrefab;
    public List<ZombieScript> zombies = new List<ZombieScript>();
    public GameObject zombiePrefab;

    private void Awake()
    {
        PV = gameObject.GetComponent<PhotonView>();
        if (PV.Owner.IsMasterClient)
        {
            aIPoolInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if(PhotonNetwork.IsMasterClient)
            for (int i = 0; i < amountToPool; i++)
            {
                GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", zombiePrefab.name), Vector3.zero, Quaternion.identity);
                obj.gameObject.SetActive(false);
                zombies.Add(obj.GetComponent<ZombieScript>());
                obj.transform.parent = gameObject.transform;
            }
    }
    public int GetRandomZombiePhotonId()
    {
        foreach (ZombieScript obj in zombies)
            if (!obj.gameObject.activeSelf)
                return obj.PV.ViewID;
        return 0;
    }
    public Skeleton GetPooledSkeleton()
    {
        foreach (Skeleton obj in skeletons)
            if (!obj.gameObject.activeSelf)
                return obj;
        return null;
    }

    public ZombieScript GetPooledZombie(int PhotonId)
    {
        foreach (ZombieScript obj in zombies)
            if (obj.PV.ViewID == PhotonId)
                return obj;
        return null;
    }
}
