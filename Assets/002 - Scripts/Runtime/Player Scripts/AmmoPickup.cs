using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AmmoPickup : MonoBehaviour
{
    public PhotonView PV;
    public Player playerProperties;

    int _originalLayer;

    private void Start()
    {
        _originalLayer = gameObject.layer;

        playerProperties.OnPlayerDeath -= OnPLayerDeath;
        playerProperties.OnPlayerDeath += OnPLayerDeath;

        playerProperties.OnPlayerRespawned -= OnPlayerRespawn;
        playerProperties.OnPlayerRespawned += OnPlayerRespawn;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!playerProperties.PV.IsMine || !other.GetComponent<AmmoPack>())
            return;
        LootAmmo(other.transform.position, other.GetComponent<AmmoPack>());
    }

    void LootAmmo(Vector3 ammoPackPosition, AmmoPack ammoPackScript)
    {
        PlayerInventory pInventory = playerProperties.playerInventory;
        AudioSource aSource = playerProperties.GetComponent<AllPlayerScripts>().weaponPickUp.ammoPickupAudioSource;

        int ammoToRemoveFromThisPack = 0;

        if (ammoPackScript.ammoType == "grenade")
            ammoToRemoveFromThisPack = pInventory.maxGrenades - pInventory.grenades;

        if (ammoToRemoveFromThisPack > 0 && playerProperties.PV.IsMine)
            aSource.Play();
        else
            return;

        if (ammoPackScript.GetAmmo() <= ammoToRemoveFromThisPack)
            ammoToRemoveFromThisPack = ammoPackScript.GetAmmo();

        if (ammoPackScript.ammoType == "grenade")
        {
            pInventory.grenades += ammoToRemoveFromThisPack;
            playerProperties.GetComponent<KillFeedManager>().EnterNewFeed($"Picked up {ammoToRemoveFromThisPack} Grenades");
        }

        ammoPackScript.ammoText.text = ammoPackScript.GetAmmo().ToString();

        playerProperties.allPlayerScripts.playerInventory.UpdateAllExtraAmmoHuds();

        NetworkGameManager.instance.DisableAmmoPack(ammoPackScript.spawnPoint);
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

    void OnPLayerDeath(Player p)
    {
        gameObject.layer = 3;
    }

    void OnPlayerRespawn(Player p)
    {
        gameObject.layer = _originalLayer;
    }
}
