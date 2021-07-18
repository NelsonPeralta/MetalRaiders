using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootableWeapon : MonoBehaviour
{
    public bool isWallGun;
    public int ammoInThisWeapon;
    public int extraAmmo;
    public bool isDualWieldable;

    public string weaponType;

    public bool smallAmmo;
    public bool heavyAmmo;
    public bool powerAmmo;

    private void Start()
    {
        //Debug.Log("Lootable Weapon Root: " + transform.parent);
        if (transform.parent)
        {
            string parentName = transform.parent.name;
            if (!parentName.Contains("WeaponPool"))
                Destroy(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void RandomAmmo() {
        ammoInThisWeapon = (int)Mathf.Ceil(Random.Range(0, ammoInThisWeapon));
        extraAmmo = (int)Mathf.Ceil(Random.Range(0, extraAmmo));
    }
}
