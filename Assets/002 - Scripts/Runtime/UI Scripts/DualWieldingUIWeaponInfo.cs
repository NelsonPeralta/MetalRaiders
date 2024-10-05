using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DualWieldingUIWeaponInfo : MonoBehaviour
{
    [SerializeField] PlayerInventory _playerInventory;
    [SerializeField] Image _image;
    [SerializeField] TMP_Text _loadedAmmo, _spareAmmo;



    // Update is called once per frame
    void Update()
    {
        if (_playerInventory.thirdWeapon)
        {
            _image.sprite = _playerInventory.thirdWeapon.weaponIcon;
            _loadedAmmo.text = _playerInventory.thirdWeapon.loadedAmmo.ToString();
            _spareAmmo.text = _playerInventory.thirdWeapon.spareAmmo.ToString();
        }
    }
}
