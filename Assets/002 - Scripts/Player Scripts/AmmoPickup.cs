using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AmmoPickup : MonoBehaviour
{
    public PhotonView PV;
    public PlayerProperties playerProperties;
    private void OnTriggerEnter(Collider other)
    {
        if (!playerProperties.PV.IsMine || !other.GetComponent<AmmoPack>())
            return;
        LootAmmo(other.transform.position, other.GetComponent<AmmoPack>());
    }

    void LootAmmo(Vector3 ammoPackPosition, AmmoPack ammoPackScript)
    {
        PlayerInventory pInventory = playerProperties.pInventory;
        AudioSource aSource = playerProperties.GetComponent<AllPlayerScripts>().weaponPickUp.ammoPickupAudioSource;

        int ammoToRemoveFromThisPack = 0;
        if (ammoPackScript.ammoType == "small")
            ammoToRemoveFromThisPack = pInventory.maxSmallAmmo - pInventory.smallAmmo;
        else if (ammoPackScript.ammoType == "heavy")
            ammoToRemoveFromThisPack = pInventory.maxHeavyAmmo - pInventory.heavyAmmo;
        else if (ammoPackScript.ammoType == "power")
            ammoToRemoveFromThisPack = pInventory.maxPowerAmmo - pInventory.powerAmmo;
        else if (ammoPackScript.ammoType == "grenade")
            ammoToRemoveFromThisPack = pInventory.maxGrenades - pInventory.grenades;

        if (ammoToRemoveFromThisPack > 0 && playerProperties.PV.IsMine)
            aSource.Play();
        else
            return;

        if (ammoPackScript.GetAmmo() <= ammoToRemoveFromThisPack)
            ammoToRemoveFromThisPack = ammoPackScript.GetAmmo();

        if (ammoPackScript.ammoType == "small")
            pInventory.smallAmmo += ammoToRemoveFromThisPack;
        else if (ammoPackScript.ammoType == "heavy")
            pInventory.heavyAmmo += ammoToRemoveFromThisPack;
        else if (ammoPackScript.ammoType == "power")
            pInventory.powerAmmo += ammoToRemoveFromThisPack;
        else if (ammoPackScript.ammoType == "grenade")
            pInventory.grenades += ammoToRemoveFromThisPack;

        //ammoPackScript.GetAmmo() -= ammoToRemoveFromThisPack;
        ammoPackScript.ammoText.text = ammoPackScript.GetAmmo().ToString();

        playerProperties.allPlayerScripts.playerInventory.UpdateAllExtraAmmoHuds();

        if (ammoPackScript.onlineAmmoPackSpawnPoint)
            PV.RPC("DisableAmmoPack", RpcTarget.All, ammoPackPosition);
        else
            PV.RPC("DestroyAmmoPack", RpcTarget.All, ammoPackPosition);
    }

    [PunRPC]
    void DisableAmmoPack(Vector3 ammoPackPosition)
    {
        for (int i = 0; i < playerProperties.weaponPool.allAmmoPackSpawnPoints.Count; i++)
            if (playerProperties.weaponPool.allAmmoPackSpawnPoints[i].transform.position == ammoPackPosition)
            {
                Debug.Log("Disabling ammo pack");
                playerProperties.weaponPool.allAmmoPackSpawnPoints[i].ammoPack.gameObject.SetActive(false);
                playerProperties.weaponPool.allAmmoPackSpawnPoints[i].StartRespawn();
            }
    }

    [PunRPC]
    void DestroyAmmoPack(Vector3 ammoPackPosition)
    {
        try
        {
            foreach (AmmoPack ap in FindObjectsOfType<AmmoPack>())
            {
                Debug.Log("Destroying ammo pack");
                if (ap.transform.position == ammoPackPosition)
                    Destroy(ap.gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
}