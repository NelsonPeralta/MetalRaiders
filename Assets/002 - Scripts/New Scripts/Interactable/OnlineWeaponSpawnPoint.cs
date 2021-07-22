using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class OnlineWeaponSpawnPoint : MonoBehaviour
{
    public string weapon;
    public GameObject weaponPlaceHolder;
    public GameObject weaponSpawned;
    public float timeToSpawn;
    //public bool spawnAtStart;

    public WeaponPool weaponPool;
    public OnlineGameTime onlineGameTime;


    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        onlineGameTime = OnlineGameTime.onlineGameTimeInstance;

        if (weaponPlaceHolder)
            weaponPlaceHolder.gameObject.SetActive(false);

        //if (spawnAtStart)
            StartCoroutine( SpawnNewWeaponFromWeaponPool(0.1f));

        //StartCoroutine(RespawnWeapon());
    }

    public IEnumerator RespawnWeapon(int newTimeToSpawn = 0)
    {
        if(newTimeToSpawn == 0)
            yield return new WaitForSeconds(timeToSpawn);
        else
            yield return new WaitForSeconds(newTimeToSpawn);
        SpawnNewWeaponFromWeaponPool(0.1f);
        //StartCoroutine(RespawnWeapon());
    }

    IEnumerator SpawnNewWeaponFromWeaponPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!weaponSpawned)
        {
            Debug.Log("Spawning New Weapon");
            //var newWeap = Instantiate(weapon, gameObject.transform.position, gameObject.transform.rotation); //* Quaternion.Euler(180, 0, 180)
            var newWeap = weaponPool.GetWeaponFromList(weapon);
            newWeap.transform.position = transform.position;
            newWeap.transform.rotation = transform.rotation;
            newWeap.SetActive(true);
            newWeap.GetComponent<LootableWeapon>().onlineWeaponSpawnPoint = this;
            weaponSpawned = newWeap;
        }
    }

    public void EmptyWeaponCache()
    {
        weaponSpawned = null;
        Debug.Log($"Time weapon grabbed: {onlineGameTime.totalTime}");
        StartCoroutine(SpawnNewWeaponFromWeaponPool(timeToSpawn));
    }
}
