using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameObjectPool : MonoBehaviour
{
    public PhotonView PV;
    public static GameObjectPool gameObjectPoolInstance;
    public int amountToPool;
    public bool objectsSpawned = false;

    [Header("Bullets")]
    public List<GameObject> bullets = new List<GameObject>();
    public GameObject bulletPrefab;

    [Header("Blood Hit")]
    public List<GameObject> bloodHits = new List<GameObject>();
    public GameObject bloodHitPrefab;

    [Header("Generic Hit")]
    public List<GameObject> genericHits = new List<GameObject>();
    public GameObject genericHitPrefab;

    [Header("Player Ragdoll")]
    public List<GameObject> ragdolls = new List<GameObject>();
    public GameObject ragdollPrefab;

    [Header("Testing Object")]
    public List<GameObject> testingObjects = new List<GameObject>();
    public GameObject testingObjectPrefab;

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
        if (!GameObjectPool.gameObjectPoolInstance.objectsSpawned)
            for (int i = 0; i < amountToPool; i++)
            {
                GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
                //GameObject obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlinePlayerBullet"), Vector3.zero, Quaternion.identity);
                obj.SetActive(false);
                bullets.Add(obj);
                obj.transform.parent = gameObject.transform;

                obj = Instantiate(bloodHitPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                bloodHits.Add(obj);
                obj.transform.parent = gameObject.transform;

                obj = Instantiate(genericHitPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                genericHits.Add(obj);
                obj.transform.parent = gameObject.transform;

                obj = Instantiate(ragdollPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                ragdolls.Add(obj);
                obj.transform.parent = gameObject.transform;

                obj = Instantiate(testingObjectPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                testingObjects.Add(obj);
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

    public GameObject SpawnPooledBloodHit()
    {
        foreach (GameObject obj in bloodHits)
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

    public GameObject SpawnPooledTestingObject()
    {
        foreach (GameObject obj in testingObjects)
            if (!obj.activeSelf)
                return obj;
        return null;
    }

    private void OnDestroy()
    {
        foreach (GameObject go in bullets)
            Destroy(go);

        foreach (GameObject go in bloodHits)
            Destroy(go);

        foreach (GameObject go in genericHits)
            Destroy(go);

        foreach (GameObject go in ragdolls)
            Destroy(go);

        foreach (GameObject go in testingObjects)
            Destroy(go);

        gameObjectPoolInstance = null;
    }
}
