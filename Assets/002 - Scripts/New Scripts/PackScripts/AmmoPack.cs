using System.Collections;
using UnityEngine;
using TMPro;

public class AmmoPack : MonoBehaviour
{
    [Header("Single")]
    public WeaponPool weaponPool;

    [Header("Ammo")]
    public string ammoType;
    public int defaultAmmo;
    public int ammoInThisPack;
    public float spawnTime;
    public bool canRespawn = false;
    public bool spawnsAtStart = false;
    public bool randomAmount;

    [Header("Respawn")]
    public bool respawnTriggered;
    public float respawnCountdown;

    [Header("Classes")]
    public GameObject model;
    public TextMeshPro ammoText;
    public OnlineAmmoPackSpawnPoint spawnPoint;

    [Header("Other Classes")]
    public PlayerInventory pInventory;

    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
    }

    public void ExecuteAction()
    {
        defaultAmmo = ammoInThisPack;

        disableModel();

        if (spawnsAtStart)
            if (randomAmount)
                enablePack(RandomAmmo());
            else if (!randomAmount)
                enablePack(defaultAmmo);
            else
                ;
        //else
        //    StartCoroutine(Respawn(spawnTime));

        //if (canRespawn)
        //    StartCoroutine(Respawn(spawnTime));
        if (!randomAmount)
            enablePack(defaultAmmo);
        else
            enablePack(RandomAmmo());
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ammo Pack Collided With: " + other);
        if (other.GetComponent<PlayerProperties>() && ammoInThisPack > 0 && model.activeSelf)
        {
            pInventory = other.GetComponent<PlayerProperties>().pInventory;
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

            if (ammoToRemoveFromThisPack > 0)
                aSource.Play();
            else
                return;

            if (ammoInThisPack <= ammoToRemoveFromThisPack)
                ammoToRemoveFromThisPack = ammoInThisPack;

            if (ammoType == "small")
                pInventory.smallAmmo += ammoInThisPack;
            else if (ammoType == "heavy")
                pInventory.heavyAmmo += ammoInThisPack;
            else if (ammoType == "power")
                pInventory.powerAmmo += ammoInThisPack;
            else if (ammoType == "grenade")
                pInventory.grenades += ammoInThisPack;

            ammoInThisPack -= ammoToRemoveFromThisPack;
            ammoText.text = ammoInThisPack.ToString();

            if (ammoInThisPack <= 0)
                disableModel();
        }
    }

    IEnumerator Respawn(float respawnTime)
    {
        Debug.Log("Starting Ammo Pack Respawn");
        yield return new WaitForSeconds(respawnTime);

        Debug.Log("INSIDE Ammo Pack Respawn");


        if (!randomAmount)
            enablePack(defaultAmmo);
        else
            enablePack(RandomAmmo());

        //if(canRespawn)
        //    StartCoroutine(Respawn(spawnTime));
    }


    void enablePack(int ammoCount)
    {
        for (int i = 0; i < weaponPool.allAmmoPacks.Count; i++)
            if (weaponPool.allAmmoPacks[i] == gameObject)
            {
                AmmoPack correspondingAmmoPackInPool = weaponPool.allAmmoPacks[i].GetComponent<AmmoPack>();
                correspondingAmmoPackInPool.model.SetActive(true);
                correspondingAmmoPackInPool.ammoInThisPack = ammoCount;
                correspondingAmmoPackInPool.ammoText.text = ammoInThisPack.ToString();
            }
    }

    void disableModel()
    {
        for(int i = 0; i < weaponPool.allAmmoPacks.Count; i++)
            if(weaponPool.allAmmoPacks[i] == gameObject)
            {
                AmmoPack correspondingAmmoPackInPool = weaponPool.allAmmoPacks[i].GetComponent<AmmoPack>();
                correspondingAmmoPackInPool.model.SetActive(false);
                correspondingAmmoPackInPool.ammoInThisPack = 0;
                correspondingAmmoPackInPool.ammoText.text = "";

                if(canRespawn)
                    StartCoroutine(Respawn(spawnTime));
            }
    }

    int RandomAmmo()
    {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.8f)));
        return ranAmmo;
    }
}
