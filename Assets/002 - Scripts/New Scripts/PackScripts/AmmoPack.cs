using System.Collections;
using UnityEngine;
using TMPro;

public class AmmoPack : MonoBehaviour
{
    [Header("Single")]
    public WeaponPool weaponPool;
    public OnlineGameTime onlineGameTime;

    [Header("Ammo")]
    public string ammoType;
    int defaultAmmo;
    public int ammoInThisPack;

    [Header("Classes")]
    public TextMeshPro ammoText;
    public OnlineAmmoPackSpawnPoint spawnPoint;

    [Header("Other Classes")]
    public PlayerProperties playerProperties;

    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        onlineGameTime = OnlineGameTime.onlineGameTimeInstance;
        defaultAmmo = ammoInThisPack;
        UpdateAmmoText();
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ammo Pack Collided With: " + other);
        if (other.GetComponent<PlayerProperties>() && ammoInThisPack > 0)
        {
            playerProperties = other.GetComponent<PlayerProperties>();
            PlayerInventory pInventory = other.GetComponent<PlayerProperties>().pInventory;
            AudioSource aSource = other.GetComponent<AllPlayerScripts>().weaponPickUp.ammoPickupAudioSource;

            int ammoToRemoveFromThisPack = 0;
            if (ammoType == "small")
                ammoToRemoveFromThisPack = pInventory.maxSmallAmmo - pInventory.smallAmmo;
            else if (ammoType == "heavy")
                ammoToRemoveFromThisPack = pInventory.maxHeavyAmmo - pInventory.heavyAmmo;
            else if (ammoType == "power")
                ammoToRemoveFromThisPack = pInventory.maxPowerAmmo - pInventory.powerAmmo;
            else if (ammoType == "grenade")
                ammoToRemoveFromThisPack = pInventory.maxGrenades - pInventory.grenades;

            if (ammoToRemoveFromThisPack > 0 && playerProperties.PV.IsMine)
                aSource.Play();
            else
                return;

            if (ammoInThisPack <= ammoToRemoveFromThisPack)
                ammoToRemoveFromThisPack = ammoInThisPack;

            if (ammoType == "small")
                pInventory.smallAmmo += ammoToRemoveFromThisPack;
            else if (ammoType == "heavy")
                pInventory.heavyAmmo += ammoToRemoveFromThisPack;
            else if (ammoType == "power")
                pInventory.powerAmmo += ammoToRemoveFromThisPack;
            else if (ammoType == "grenade")
                pInventory.grenades += ammoToRemoveFromThisPack;

            ammoInThisPack -= ammoToRemoveFromThisPack;
            ammoText.text = ammoInThisPack.ToString();

                DisableAmmoPack();
        }
    }

    public void EnablePack()
    {
        for (int i = 0; i < weaponPool.allAmmoPacks.Count; i++)
            if (weaponPool.allAmmoPacks[i] == gameObject)
            {

                AmmoPack correspondingAmmoPackInPool = weaponPool.allAmmoPacks[i].GetComponent<AmmoPack>();
                correspondingAmmoPackInPool.ammoInThisPack = defaultAmmo;
                correspondingAmmoPackInPool.UpdateAmmoText();
                correspondingAmmoPackInPool.gameObject.SetActive(true);
            }
    }

    void DisableAmmoPack()
    {
        for(int i = 0; i < weaponPool.allAmmoPacks.Count; i++)
            if(weaponPool.allAmmoPacks[i] == gameObject)
            {
                AmmoPack correspondingAmmoPackInPool = weaponPool.allAmmoPacks[i].GetComponent<AmmoPack>();
                //if (spawnPoint)
                //    spawnPoint.StartRespawn();
                //correspondingAmmoPackInPool.gameObject.SetActive(false);
                playerProperties.allPlayerScripts.weaponPickUp.DisableAmmoPackWithRPC(i);
            }
    }

    int RandomAmmo()
    {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.8f)));
        return ranAmmo;
    }

    void UpdateAmmoText()
    {
        ammoText.text = ammoInThisPack.ToString();
    }
}
