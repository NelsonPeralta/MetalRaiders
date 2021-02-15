using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxAmmo : MonoBehaviour
{
    public SwarmMode swarmMode;
    public AudioSource audioSource;
    bool ammoGiven;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.GetComponent<PlayerProperties>();

            if (!player.isDead && !ammoGiven)
            {
                if (player.swarmMode)
                    if (player.swarmMode.pManager)
                    {
                        foreach (GameObject p in player.swarmMode.pManager.allPlayers)
                        {
                            PlayerInventory playerInventory = p.GetComponent<AllPlayerScripts>().playerInventory;
                            playerInventory.smallAmmo = playerInventory.maxSmallAmmo;
                            playerInventory.heavyAmmo = playerInventory.maxHeavyAmmo;
                            playerInventory.powerAmmo = playerInventory.maxPowerAmmo;
                            playerInventory.grenades = playerInventory.maxGrenades;

                            playerInventory.UpdateAllExtraAmmoHuds();
                        }

                    }
                    else { }
                else
                {
                    PlayerInventory playerInventory = player.GetComponent<AllPlayerScripts>().playerInventory;
                    playerInventory.smallAmmo = playerInventory.maxSmallAmmo;
                    playerInventory.heavyAmmo = playerInventory.maxHeavyAmmo;
                    playerInventory.powerAmmo = playerInventory.maxPowerAmmo;
                    playerInventory.grenades = playerInventory.maxGrenades;
                    playerInventory.UpdateAllExtraAmmoHuds();
                }
                audioSource.Play();
                ammoGiven = true;
                Destroy(gameObject, 1);
            }
        }
    }
}
