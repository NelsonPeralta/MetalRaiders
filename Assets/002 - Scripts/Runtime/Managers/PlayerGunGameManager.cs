using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunGameManager : MonoBehaviour
{
    public int index
    {
        get { return _index; }
        set
        {
            if (value < 0 || value >= gunIndex.Count)
            {
                return;
            }

            _index = value;

            WeaponProperties _preActiveWeapon = playerInventory.activeWeapon;

            playerInventory.activeWeapon = _gunIndex[_index];
            playerInventory.activeWeapon.currentAmmo = playerInventory.activeWeapon.ammoCapacity;
            playerInventory.activeWeapon.spareAmmo = playerInventory.activeWeapon.maxAmmo;



            //playerInventory.holsteredWeapon = playerInventory.weaponCodeNameDict["nailgun"];
            //playerInventory.holsteredWeapon.currentAmmo = playerInventory.activeWeapon.ammoCapacity;
            //playerInventory.holsteredWeapon.spareAmmo = playerInventory.activeWeapon.maxAmmo;

            _preActiveWeapon.gameObject.SetActive(false);
            //playerInventory.holsteredWeapon = 
        }
    }

    public List<WeaponProperties> gunIndex { get { return _gunIndex; } }

    [SerializeField] List<WeaponProperties> _gunIndex = new List<WeaponProperties>();
    [SerializeField] PlayerInventory playerInventory;

    int _index;
}
