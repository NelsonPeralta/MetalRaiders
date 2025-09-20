using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameObjectPool : MonoBehaviour
{
    public static GameObjectPool instance { get { return _instance; } set { _instance = value; } }




    bool objectsSpawned = false;

    [SerializeField] GameObject bulletPrefab, bluePlasmaRoundPrefab, redPlasmaRoundPrefab, greenPlasmaRoundPrefab, shardRoundPrefab;
    [SerializeField] GameObject shieldHitPrefab, bloodHitPrefab, genericHitPrefab, weaponSmokeCollisionPrefab;
    [SerializeField] GameObject bluePlasmaRoundPrefab_SS, greenPlasmaRoundPrefab_SS;
    [SerializeField] GameObject bulletMetalImpactPrefab, waterSmallImpactPrefab;

    public List<GameObject> bullets = new List<GameObject>();
    public List<GameObject> bluePlasmaRounds = new List<GameObject>();
    public List<GameObject> redPlasmaRounds = new List<GameObject>();
    public List<GameObject> greenPlasmaRounds = new List<GameObject>();
    public List<GameObject> shardRounds = new List<GameObject>();

    public List<GameObject> bluePlasmaRounds_SS = new List<GameObject>();
    public List<GameObject> greenPlasmaRounds_SS = new List<GameObject>();


    public List<GameObject> shieldHits = new List<GameObject>();
    public List<GameObject> bloodHits = new List<GameObject>();
    public List<GameObject> genericHits = new List<GameObject>();
    public List<GameObject> weaponSmokeCollisions = new List<GameObject>();


    public List<GameObject> bulletMetalImpactList = new List<GameObject>();
    public List<GameObject> waterSmallImpactList = new List<GameObject>();





    static GameObjectPool _instance;

    void Awake()
    {
        _instance = this;
    }



    private void Start()
    {
        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 20; i++)
        {
            GameObject obj = Instantiate(bulletPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            bullets.Add(obj);
            obj.transform.parent = gameObject.transform;


            obj = Instantiate(shardRoundPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            shardRounds.Add(obj);
            obj.transform.parent = gameObject.transform;
        }



        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 10; i++)
        {
            GameObject obj = null;

            if (GameManager.instance.nbLocalPlayersPreset == 1)
            {
                obj = Instantiate(bluePlasmaRoundPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                bluePlasmaRounds.Add(obj);
                obj.transform.parent = gameObject.transform;

                obj = Instantiate(redPlasmaRoundPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                redPlasmaRounds.Add(obj);
                obj.transform.parent = gameObject.transform;

                obj = Instantiate(greenPlasmaRoundPrefab, transform.position, transform.rotation);
                obj.SetActive(false);
                greenPlasmaRounds.Add(obj);
                obj.transform.parent = gameObject.transform;
            }
            else
            {
                obj = Instantiate(bluePlasmaRoundPrefab_SS, transform.position, transform.rotation);
                obj.SetActive(false);
                bluePlasmaRounds_SS.Add(obj);
                obj.transform.parent = gameObject.transform;


                obj = Instantiate(greenPlasmaRoundPrefab_SS, transform.position, transform.rotation);
                obj.SetActive(false);
                greenPlasmaRounds_SS.Add(obj);
                obj.transform.parent = gameObject.transform;
            }








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




            obj = Instantiate(weaponSmokeCollisionPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            weaponSmokeCollisions.Add(obj);
            obj.transform.parent = gameObject.transform;
        }



        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 20; i++)
        {
            GameObject obj = Instantiate(bulletMetalImpactPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            bulletMetalImpactList.Add(obj);
            obj.transform.parent = gameObject.transform;

            obj = Instantiate(waterSmallImpactPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            waterSmallImpactList.Add(obj);
            obj.transform.parent = gameObject.transform;
        }


        CurrentRoomManager.instance.AddSpawnedMappAddOn(null);
    }


    public enum BulletType { normal, blue_plasma_round, red_plasma_round, green_plasma_round, shard_round }
    public GameObject SpawnPooledBullet(BulletType bulletType)
    {
        if (bulletType == BulletType.normal)
            foreach (GameObject obj in bullets)
                if (!obj.activeSelf)
                    return obj;




        if (bulletType == BulletType.blue_plasma_round && GameManager.instance.nbLocalPlayersPreset == 1)
            foreach (GameObject obj in bluePlasmaRounds)
                if (!obj.activeSelf)
                    return obj;
        if (bulletType == BulletType.blue_plasma_round && GameManager.instance.nbLocalPlayersPreset > 1)
            foreach (GameObject obj in bluePlasmaRounds_SS)
                if (!obj.activeSelf)
                    return obj;



        if (bulletType == BulletType.red_plasma_round && GameManager.instance.nbLocalPlayersPreset == 1)
            foreach (GameObject obj in redPlasmaRounds)
                if (!obj.activeSelf)
                    return obj;




        if (bulletType == BulletType.green_plasma_round && GameManager.instance.nbLocalPlayersPreset == 1)
            foreach (GameObject obj in greenPlasmaRounds)
                if (!obj.activeSelf)
                    return obj;
        if (bulletType == BulletType.green_plasma_round && GameManager.instance.nbLocalPlayersPreset > 1)
            foreach (GameObject obj in greenPlasmaRounds_SS)
                if (!obj.activeSelf)
                    return obj;




        if (bulletType == BulletType.shard_round)
            foreach (GameObject obj in shardRounds)
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
        Debug.Log("SpawnBulletHole");
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

    public GameObject SpawnSmallWaterEffect(Vector3 pos, Vector3 norm)
    {
        foreach (GameObject obj in waterSmallImpactList)
            if (!obj.activeSelf)
            {
                obj.transform.position = pos;
                //obj.transform.rotation = Quaternion.LookRotation(norm);
                //obj.transform.position += obj.transform.forward / 1000;
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
        instance = null;
    }

    public IEnumerator DisableObjectAfterTime(GameObject obj, float time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
