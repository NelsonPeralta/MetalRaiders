using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponAmmoCounter : MonoBehaviour
{
    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] TMP_Text _tmp;

    float _c = 0;

    private void OnEnable()
    {
        _c = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (_c <= 0)
                _tmp.text = _weaponProperties.currentAmmo.ToString();
            else
                _tmp.text = "";
        }
        catch { }
    }
}
