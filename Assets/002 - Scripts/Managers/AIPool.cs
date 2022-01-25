using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class AIPool : MonoBehaviour
{
    public PhotonView PV;
    public static AIPool aIPoolInstance;
    public int amountToPool;

    [Header("AI Prefabs")]
    public GameObject skeletonPrefab;
    public GameObject zombiePrefab;
    public Transform watcherPrefab;

    [Header("AI Lists")]
    public List<Skeleton> skeletons = new List<Skeleton>();
    public List<Zombie> zombies = new List<Zombie>();
    public List<Watcher> watchers = new List<Watcher>();

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
                zombies.Add(obj.GetComponent<Zombie>());
                obj.transform.parent = gameObject.transform;

                obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AIs", watcherPrefab.name), Vector3.zero, Quaternion.identity);
                obj.SetActive(false);
                watchers.Add(obj.GetComponent<Watcher>());
                obj.transform.parent = gameObject.transform;
            }
    }
    public int GetRandomZombiePhotonId()
    {
        //foreach (ZombieScript obj in zombies)
        //    if (!obj.gameObject.activeSelf)
        //        return obj.PV.ViewID;
        return 0;
    }

    public int GetRandomWatcherPhotonId()
    {
        //foreach (Watcher obj in watchers)
        //    if (!obj.gameObject.activeSelf)
        //        return obj.PV.ViewID;
        return 0;
    }
    public Zombie GetPooledZombie(int PhotonId)
    {
        //foreach (ZombieScript obj in zombies)
        //    if (obj.PV.ViewID == PhotonId)
        //        return obj;
        return null;
    }

    public Watcher GetPooledWatcher(int photonViewId)
    {
        //foreach (Watcher obj in watchers)
        //    if (obj.PV.ViewID == photonViewId)
        //        return obj;
        return null;
    }
}
