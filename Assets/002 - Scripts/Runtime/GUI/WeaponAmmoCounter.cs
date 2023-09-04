using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponAmmoCounter : MonoBehaviour
{
    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] LootableWeapon _lootableWeapon;
    [SerializeField] TMP_Text _tmp;

    float _c = 0;

    private void OnEnable()
    {
        _c = 0.2f;
    }
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
        if (_c > 0)
            _c -= Time.deltaTime;

        if (_c <= 0)
        {
            try
            {
                _tmp.text = _weaponProperties.currentAmmo.ToString();
            }
            catch
            {
                _tmp.text = _lootableWeapon.localAmmo.ToString();
            }
        }
        else
            _tmp.text = "";
    }
}
