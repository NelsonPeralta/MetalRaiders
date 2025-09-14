using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponAmmoCounter : MonoBehaviour
{
    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] LootableWeapon _lootableWeapon;
    [SerializeField] TMP_Text _tmp;


    private int _lastAmmo = -1;



    //    Why this is better:
    //No string allocations → uses SetText("{0}", ammo) instead of ToString().
    //Updates only when ammo changes → no unnecessary TMP updates every frame.
    //GC Alloc = 0 B and CPU time almost disappears (<0.01 ms typically).

    void Update()
    {
        if (_tmp == null) return;

        // Pick source of ammo
        int ammo = 0;
        if (_weaponProperties)
            ammo = _weaponProperties.loadedAmmo;
        else if (_lootableWeapon)
            ammo = _lootableWeapon.localAmmo;

        // Only update TMP text if ammo changed
        if (ammo != _lastAmmo)
        {
            _tmp.SetText("{0}", ammo); // avoids ToString() allocations
            _lastAmmo = ammo;
        }
    }
}
