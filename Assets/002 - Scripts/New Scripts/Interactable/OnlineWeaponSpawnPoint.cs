using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class OnlineWeaponSpawnPoint : MonoBehaviour
{
    public string weapon;
    public GameObject weaponPlaceHolder;
    public LootableWeapon weaponSpawned;
    public int timeToSpawn;
    //public bool spawnAtStart;

    public WeaponPool weaponPool;
    public OnlineGameTime onlineGameTime;


    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        onlineGameTime = OnlineGameTime.onlineGameTimeInstance;

        if (weaponPlaceHolder)
            weaponPlaceHolder.gameObject.SetActive(false);

        StartCoroutine(SpawnNewWeaponFromWeaponPool(0.1f));
    }

    IEnumerator SpawnNewWeaponFromWeaponPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (weaponPool.allWeapons.Count <= 0)
            StartCoroutine(SpawnNewWeaponFromWeaponPool(0.1f));
        if (!weaponSpawned)
        {
            //Debug.Log("Spawning New Weapon");
            var newWeap = weaponPool.GetWeaponFromList(weapon).GetComponent<LootableWeapon>();
            newWeap.transform.position = transform.position;
            newWeap.transform.rotation = transform.rotation;
            newWeap.gameObject.SetActive(true);
            newWeap.onlineWeaponSpawnPoint = this;
            weaponSpawned = newWeap;
        }
        else
            weaponSpawned.EnableWeapon();
    }

    public void StartRespawn()
    {
        int timeWeaponWasGrabbed = onlineGameTime.totalTime;
        int newSpawnTime = timeToSpawn - (timeWeaponWasGrabbed % timeToSpawn);
        Debug.Log($"Time weapon grabbed: {onlineGameTime.totalTime}. New Spawn Time: {newSpawnTime}");
        StartCoroutine(SpawnNewWeaponFromWeaponPool(newSpawnTime));
    }
}
