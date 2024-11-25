using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameObjectPool : MonoBehaviour
{
    public PhotonView PV;
    public int amountToPool;
    bool objectsSpawned = false;

    [Header("Base Objects")]
    public List<GameObject> bullets = new List<GameObject>();
    public GameObject bulletPrefab;
    public List<GameObject> shieldHits = new List<GameObject>();
    public GameObject shieldHitPrefab;
    public List<GameObject> bloodHits = new List<GameObject>();
    public GameObject bloodHitPrefab;
    public List<GameObject> genericHits = new List<GameObject>();
    public GameObject genericHitPrefab;
    public List<GameObject> weaponSmokeCollisions = new List<GameObject>();
    public GameObject weaponSmokeCollisionPrefab;


    public List<GameObject> bulletMetalImpactList = new List<GameObject>();
    [SerializeField] GameObject bulletMetalImpactPrefab;



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

            obj = Instantiate(shieldHitPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            shieldHits.Add(obj);
            obj.transform.parent = gameObject.transform;




            obj = Instantiate(bulletMetalImpactPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            bulletMetalImpactList.Add(obj);
            obj.transform.parent = gameObject.transform;

            obj = Instantiate(weaponSmokeCollisionPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            weaponSmokeCollisions.Add(obj);
            obj.transform.parent = gameObject.transform;
        }



        for (int i = 0; i < 100; i++)
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
                    return obj;
                }
        return null;
    }

    public GameObject SpawnPooledGenericHit(Vector3 pos, Vector3 norm)
    {
        foreach (GameObject obj in genericHits)
            if (!obj.activeSelf)
                if (!obj.activeSelf)
                {
                    obj.transform.position = pos;


                    obj.transform.rotation = Quaternion.LookRotation(norm);
                    obj.transform.position += obj.transform.forward / 10;


                    obj.SetActive(true);
                    return obj;
                }
        return null;
    }



    public GameObject SpawnPooledShieldHit()
    {
        foreach (GameObject obj in shieldHits)
            if (!obj.activeSelf)
            {
                return obj;
            }
        return null;
    }



    public GameObject SpawnBulletHole(Vector3 pos, Vector3 norm)
    {
        foreach (GameObject obj in bulletMetalImpactList)
            if (!obj.activeSelf)
            {
                obj.transform.position = pos;
                obj.transform.rotation = Quaternion.LookRotation(norm);
                obj.transform.position += obj.transform.forward / 1000;
                obj.SetActive(true);

                return obj;
            }
        return null;
    }
    public GameObject SpawnWeaponSmokeCollisionObject(Vector3 pos, AudioClip ac)
    {
        foreach (GameObject obj in weaponSmokeCollisions)
            if (!obj.activeInHierarchy)
            {
                obj.GetComponent<AudioSource>().clip = ac;

                obj.transform.position = pos;
                obj.SetActive(true);
                return obj;
            }
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
    }

    public IEnumerator DisableObjectAfterTime(GameObject obj, float time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
