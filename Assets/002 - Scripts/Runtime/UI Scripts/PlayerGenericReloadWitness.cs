using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGenericReloadWitness : MonoBehaviour
{
    public PlayerInventory pInventory;
    public PlayerController playerController;

    public GameObject witness;

    [SerializeField] bool isLeft;


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
                if (!pInventory.isDualWielding)
                {
                    witness.SetActive(pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic && playerController.isReloading);
                }
                else
                {
                    if (!isLeft)
                    {
                        witness.SetActive(playerController.isReloadingRight);
                    }
                    else
                    {
                        witness.SetActive(playerController.isReloadingLeft);
                    }
                }
            }
        }
    }
}
