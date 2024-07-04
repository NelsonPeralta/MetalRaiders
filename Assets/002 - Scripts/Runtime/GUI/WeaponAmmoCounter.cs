using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponAmmoCounter : MonoBehaviour
{
    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] LootableWeapon _lootableWeapon;
    [SerializeField] TMP_Text _tmp;


    private void Start()
    {
        try
        {
            _weaponProperties = GetComponent<WeaponProperties>();
            _lootableWeapon = GetComponent<LootableWeapon>();
        }
        catch
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_tmp != null)
        {
            _tmp.text = "";

            if (_weaponProperties)
                _tmp.text = _weaponProperties.loadedAmmo.ToString();
            else if (_lootableWeapon)
                _tmp.text = _lootableWeapon.localAmmo.ToString();
        }
    }
}
