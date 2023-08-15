using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineAmmoPackSpawnPoint : MonoBehaviour
{
    [Header("Info")]
    public bool randomAmmo;
    public string ammoType;
    public GameObject Placeholder;
    public NetworkGrenadeSpawnPoint ammoPack;
    public int timeToSpawn;



    private void Start()
    {
        if (Placeholder)
            Placeholder.gameObject.SetActive(false);

        StartCoroutine(SpawnNewAmmoPackFromWeaponPool(0));
    }

    IEnumerator SpawnNewAmmoPackFromWeaponPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (FindObjectOfType<WeaponPool>().allAmmoPacks.Count <= 0)
            StartCoroutine(SpawnNewAmmoPackFromWeaponPool(0.1f));
        else if (!ammoPack)
        {
            var newAmmoPack = FindObjectOfType<WeaponPool>().GetAmmoPackFromList(ammoType).GetComponent<NetworkGrenadeSpawnPoint>();
            newAmmoPack.onlineAmmoPackSpawnPoint = this;
            newAmmoPack.transform.position = transform.position;
            newAmmoPack.transform.rotation = transform.rotation;
            newAmmoPack.gameObject.SetActive(true);
            ammoPack = newAmmoPack;
            if(randomAmmo)
                ammoPack.SetRandomAmmoAsDefault();
        }
        //else
        //    ammoPack.EnablePack();
    }

    public void StartRespawn()
    {
        //Debug.Log($"Time Ammo Pack grabbed: {FindObjectOfType<OnlineGameTime>().totalTime}. New Spawn Time: {newSpawnTime}");

        int timeWeaponWasGrabbed = FindObjectOfType<GameTime>().totalTime;
        int newSpawnTime = timeToSpawn - (timeWeaponWasGrabbed % timeToSpawn);
        StartCoroutine(SpawnNewAmmoPackFromWeaponPool(newSpawnTime));
    }
}
