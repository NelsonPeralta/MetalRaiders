using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    public ChildManager cManager;
    public PlayerInventory pInventory;

    int ammoAllowedToRemoveFromThisPack;

    public AudioSource aSource;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            cManager = GetComponent<ChildManager>();

            pInventory = other.gameObject.GetComponent<ChildManager>().FindChildWithTag("Player Inventory").GetComponent<PlayerInventory>();

            if(pInventory.smallAmmo < pInventory.maxSmallAmmo || pInventory.heavyAmmo < pInventory.maxHeavyAmmo || pInventory.powerAmmo < pInventory.maxPowerAmmo)
            {
                StartCoroutine(GiveAmmo());
            }
        }
    }

    IEnumerator GiveAmmo()
    {
        pInventory.smallAmmo = pInventory.maxSmallAmmo;
        pInventory.heavyAmmo = pInventory.maxHeavyAmmo;
        pInventory.powerAmmo = pInventory.maxPowerAmmo;

        cManager.FindChildWithTagScript("Pack FX").SetActive(false);
        cManager.FindChildWithTagScript("Motion Tracker Icon").SetActive(false);

        yield return new WaitForSeconds(5);

        Destroy(this.gameObject);
    }
}
