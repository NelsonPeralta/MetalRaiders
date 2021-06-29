using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameObjectPool : MonoBehaviour
{
    public static GameObjectPool gameObjectPoolInstance;
    public int amountToPool;

    [Header("Bullets")]
    public List<GameObject> bullets = new List<GameObject>();
    public GameObject bulletPrefab;

    [Header("Bullets")]
    public List<GameObject> genericHits = new List<GameObject>();
    public GameObject genericHitPrefab;

    [Header("Player Ragdoll")]
    public List<GameObject> ragdolls = new List<GameObject>();
    public GameObject ragdollPrefab;

    private void Awake()
    {
        if (gameObjectPoolInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        gameObjectPoolInstance = this;
    }

    private void Start()
    {
        for(int i = 0; i < amountToPool; i++)
        {
            //GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
            GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlinePlayerBullet"), Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            bullets.Add(obj);
            obj.transform.parent = gameObject.transform;

            //obj = Instantiate(genericHitPrefab, transform.position, transform.rotation);
            obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineGenericHit"), Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            genericHits.Add(obj);
            obj.transform.parent = gameObject.transform;

            obj = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            ragdolls.Add(obj);
            obj.transform.parent = gameObject.transform;
        }
    }

    public GameObject SpawnPooledBullet()
    {
        foreach (GameObject obj in bullets)
            if (!obj.activeSelf)
                return obj;
        return null;
    }

    public GameObject SpawnPooledGenericHit()
    {
        foreach (GameObject obj in genericHits)
            if (!obj.activeSelf)
                return obj;
        return null;
    }

    public GameObject SpawnPooledPlayerRagdoll()
    {
        foreach (GameObject obj in ragdolls)
            if (!obj.activeSelf)
                return obj;
        return null;
    }
}
