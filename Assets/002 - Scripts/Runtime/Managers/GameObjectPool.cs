using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;

public class GameObjectPool : MonoBehaviour
{
    public int amountToPool;
    bool objectsSpawned = false;

    public GameObject bulletPrefab;
    public GameObject bloodHitPrefab;
    public GameObject genericHitPrefab;
    public GameObject weaponSmokeCollisionPrefab;
    [SerializeField] GameObject bulletMetalImpactPrefab;
    public GameObject fragGrenadePrefab;
    public GameObject stickyGrenadePrefab;
    public GameObject explosionPrefab;

    public List<GameObject> bullets = new List<GameObject>();
    public List<GameObject> bloodHits = new List<GameObject>();
    public List<GameObject> genericHits = new List<GameObject>();
    public List<GameObject> weaponSmokeCollisions = new List<GameObject>();
    public List<GameObject> bulletMetalImpactList = new List<GameObject>();
    public List<GameObject> fragGrenades = new List<GameObject>();
    public List<GameObject> stickyGrenades = new List<GameObject>();
    public List<GameObject> explosions = new List<GameObject>();





    public static GameObjectPool instance { get { return _instance; } }


    static GameObjectPool _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (GameObjectPool.instance.objectsSpawned)
            return;
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




            obj = Instantiate(bulletMetalImpactPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            bulletMetalImpactList.Add(obj);
            obj.transform.parent = gameObject.transform;

            obj = Instantiate(weaponSmokeCollisionPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            weaponSmokeCollisions.Add(obj);
            obj.transform.parent = gameObject.transform;




            obj = Instantiate(fragGrenadePrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            fragGrenades.Add(obj);
            obj.transform.parent = gameObject.transform;

            obj = Instantiate(explosionPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            explosions.Add(obj);
            obj.transform.parent = gameObject.transform;
        }



        for (int i = 0; i < amountToPool * 5; i++)
        {
            GameObject obj = Instantiate(bulletMetalImpactPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            bulletMetalImpactList.Add(obj);
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
                if (!obj.activeSelf)
                {
                    StartCoroutine(DisableObjectAfterTime(obj));
                    return obj;
                }
        return null;
    }

    public GameObject SpawnPooledGenericHit()
    {
        foreach (GameObject obj in genericHits)
            if (!obj.activeSelf)
                if (!obj.activeSelf)
                {
                    StartCoroutine(DisableObjectAfterTime(obj));
                    return obj;
                }
        return null;
    }



    public GameObject SpawnBulletMetalImpactObject()
    {
        foreach (GameObject obj in bulletMetalImpactList)
            if (!obj.activeSelf)
                if (!obj.activeSelf)
                {
                    StartCoroutine(DisableObjectAfterTime(obj, 10));
                    return obj;
                }
        return null;
    }
    public GameObject SpawnWeaponSmokeCollisionObject(Vector3 pos)
    {
        foreach (GameObject obj in weaponSmokeCollisions)
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = pos;
                obj.SetActive(true);
                StartCoroutine(DisableObjectAfterTime(obj, 10));
                return obj;
            }
        return null;
    }
    public GameObject SpawnFragGrenade()
    {
        foreach (GameObject obj in fragGrenades)
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        return null;
    }

    public GameObject SpawnStickyGrenade()
    {
        foreach (GameObject obj in stickyGrenades)
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        return null;
    }

    public GameObject SpawnExplosion()
    {
        foreach (GameObject obj in explosions)
            if (!obj.activeInHierarchy)
            {
                StartCoroutine(DisableObjectAfterTime(obj, 5));
                return obj;
            }
        return null;
    }


    IEnumerator DisableObjectAfterTime(GameObject obj, int time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
