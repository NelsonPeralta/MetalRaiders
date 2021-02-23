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

    public void RandomAmmo() {
        ammoInThisWeapon = (int)Mathf.Ceil(Random.Range(0, ammoInThisWeapon));
        extraAmmo = (int)Mathf.Ceil(Random.Range(0, extraAmmo));
    }
}
