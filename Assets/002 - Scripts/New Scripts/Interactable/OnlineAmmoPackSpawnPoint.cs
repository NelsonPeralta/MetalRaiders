using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineAmmoPackSpawnPoint : MonoBehaviour
{
    [Header("Singleton")]
    public WeaponPool weaponPool;
    public OnlineGameTime onlineGameTime;

    [Header("Info")]
    public string ammoType;
    public GameObject Placeholder;
    public AmmoPack ammoPack;
    public int timeToSpawn;



    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        onlineGameTime = OnlineGameTime.onlineGameTimeInstance;

        if (Placeholder)
            Placeholder.gameObject.SetActive(false);

        StartCoroutine(SpawnNewAmmoPackFromWeaponPool(0));
    }

    IEnumerator SpawnNewAmmoPackFromWeaponPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (weaponPool.allAmmoPacks.Count <= 0)
            StartCoroutine(SpawnNewAmmoPackFromWeaponPool(0.1f));
        else if (!ammoPack)
        {
            var newAmmoPack = weaponPool.GetAmmoPackFromList(ammoType).GetComponent<AmmoPack>();
            newAmmoPack.spawnPoint = this;
            newAmmoPack.transform.position = transform.position;
            newAmmoPack.transform.rotation = transform.rotation;
            newAmmoPack.gameObject.SetActive(true);
            ammoPack = newAmmoPack;
        }else
        {
            ammoPack.EnablePack();
        }
    }

    public void StartRespawn()
    {
        int timeWeaponWasGrabbed = onlineGameTime.totalTime;
        int newSpawnTime = timeToSpawn - (timeWeaponWasGrabbed % timeToSpawn);
        Debug.Log($"Time Ammo Pack grabbed: {onlineGameTime.totalTime}. New Spawn Time: {newSpawnTime}");
        StartCoroutine(SpawnNewAmmoPackFromWeaponPool(newSpawnTime));
    }
}
