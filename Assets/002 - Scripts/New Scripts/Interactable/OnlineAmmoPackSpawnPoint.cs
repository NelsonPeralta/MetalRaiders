using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineAmmoPackSpawnPoint : MonoBehaviour
{
    [Header("Singleton")]
    public WeaponPool weaponPool;

    [Header("Info")]
    public string ammoType;
    public GameObject Placeholder;
    public GameObject ammoPack;
    public float timeToSpawn;



    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;

        if (Placeholder)
            Placeholder.gameObject.SetActive(false);

        StartCoroutine(SpawnNewAmmoPackFromWeaponPool());
    }

    IEnumerator SpawnNewAmmoPackFromWeaponPool(int delay = 0)
    {
        yield return new WaitForSeconds(delay);
        if (weaponPool.allAmmoPacks.Count <= 0)
            StartCoroutine(SpawnNewAmmoPackFromWeaponPool(1));
        else if (!ammoPack || !ammoPack.activeSelf)
        {
            var newAmmoPack = weaponPool.GetAmmoPackFromList(ammoType);
            newAmmoPack.GetComponent<AmmoPack>().spawnPoint = this;
            newAmmoPack.transform.position = transform.position;
            newAmmoPack.transform.rotation = transform.rotation;
            newAmmoPack.SetActive(true);
            ammoPack = newAmmoPack;
        }
    }
}
