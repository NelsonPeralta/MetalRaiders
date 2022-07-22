using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootableWeapon : MonoBehaviour
{
    Vector3 _spawnPointPosition;
    public string weaponName;
    public bool isWallGun;
    public int ammoInThisWeapon;
    public int extraAmmo;
    public bool isDualWieldable;

    int defaultAmmo;
    int defaultExtraAmmo;

    public string weaponType;

    public bool smallAmmo;
    public bool heavyAmmo;
    public bool powerAmmo;


    public OnlineWeaponSpawnPoint onlineWeaponSpawnPoint;

    public Vector3 spawnPointPosition
    {
        get { return _spawnPointPosition; }
        set { _spawnPointPosition = value; }
    }

    private void Start()
    {
        defaultAmmo = ammoInThisWeapon;
        defaultExtraAmmo = extraAmmo;
        //Debug.Log("Lootable Weapon Root: " + transform.parent);
        if (transform.parent)
        {
            string parentName = transform.parent.name;
            if (!parentName.Contains("WeaponPool"))
                Destroy(gameObject);
        }
        else
            Destroy(gameObject);

        spawnPointPosition = new Vector3((float)System.Math.Round(transform.position.x, 1), (float)System.Math.Round(transform.position.y, 1), (float)System.Math.Round(transform.position.z, 1));
    }

    public void ResetAmmo()
    {
        ammoInThisWeapon = defaultAmmo;
        extraAmmo = defaultExtraAmmo;
    }

    public void RandomAmmo() {
        ammoInThisWeapon = (int)Mathf.Ceil(Random.Range(0, ammoInThisWeapon));
        extraAmmo = (int)Mathf.Ceil(Random.Range(0, extraAmmo));

        defaultAmmo = (int)Mathf.Ceil(Random.Range(0, defaultAmmo)); ;
        extraAmmo = (int)Mathf.Ceil(Random.Range(0, extraAmmo));
    }

    public void DisableWeapon()
    {
        //onlineWeaponSpawnPoint.StartCoroutine(onlineWeaponSpawnPoint.RespawnWeapon());
        onlineWeaponSpawnPoint.StartRespawn();
        gameObject.SetActive(false);
    }

    public void EnableWeapon()
    {
        gameObject.SetActive(true);
        ResetAmmo();
    }
}
