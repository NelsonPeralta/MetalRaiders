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
    public bool spawnAtStart;

    public WeaponPool weaponPool;


    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;

        if (weaponPlaceHolder)
            weaponPlaceHolder.gameObject.SetActive(false);

        if (spawnAtStart)
            SpawnNewWeaponFromWeaponPool();

        StartCoroutine(RespawnWeapon());
    }

    IEnumerator RespawnWeapon(int newTimeToSpawn = 0)
    {
        if(newTimeToSpawn == 0)
            yield return new WaitForSeconds(timeToSpawn);
        else
            yield return new WaitForSeconds(newTimeToSpawn);
        SpawnNewWeaponFromWeaponPool();
        StartCoroutine(RespawnWeapon());
    }

    void SpawnNewWeaponFromWeaponPool()
    {

        if (weaponPool.allWeapons.Count <= 0)
            StartCoroutine(RespawnWeapon(1));
        else if (!weaponSpawned || !weaponSpawned.activeSelf)
        {
            //var newWeap = Instantiate(weapon, gameObject.transform.position, gameObject.transform.rotation); //* Quaternion.Euler(180, 0, 180)
            var newWeap = weaponPool.GetWeaponFromList(weapon);
            newWeap.transform.position = transform.position;
            newWeap.transform.rotation = transform.rotation;
            newWeap.SetActive(true);
            weaponSpawned = newWeap;
        }
    }
}
