using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGenericReloadWitness : MonoBehaviour
{
    public PlayerInventory pInventory;
    public PlayerController playerController;

    public GameObject witness;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (pInventory && playerController)
        {
            if (pInventory.activeWeapon)
            {
                witness.SetActive(pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic && playerController.isReloading);
            }
        }
    }
}
