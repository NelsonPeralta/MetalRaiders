using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class WeaponSpawnPoint : MonoBehaviour
{
    public GameObject weapon;
    public GameObject weaponPlaceHolder;
    public GameObject weaponSpawned;
    public float timeToSpawn;
    public bool spawnAtStart;

    private void Start()
    {
        if (weaponPlaceHolder)
            weaponPlaceHolder.gameObject.SetActive(false);

        if (spawnAtStart)
            SpawnNewWeapon();

        StartCoroutine(RespawnWeapon());
    }

    IEnumerator RespawnWeapon()
    {
        yield return new WaitForSeconds(timeToSpawn);
        SpawnNewWeapon();
        StartCoroutine(RespawnWeapon());
    }

    void SpawnNewWeapon()
    {
        if (!weaponSpawned)
        {
            //var newWeap = Instantiate(weapon, gameObject.transform.position, gameObject.transform.rotation); //* Quaternion.Euler(180, 0, 180)

            var newWeap = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "WeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
            newWeap.name = newWeap.name.Replace("(Clone)", "");
            weaponSpawned = newWeap;
        }
    }
}
