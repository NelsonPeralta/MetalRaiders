﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SmallAmmoPack : MonoBehaviour
{
    [Header("Classes")]
    public PlayerInventory pInventory;
    public GameObject allChildren;
    public SphereCollider sphereCollider;
    public AudioSource aSource;
    public TextMeshPro ammoText;

    [Header("Ammo")]
    int defaultAmmo;
    public int ammoInThisPack = 48;
    public float spawnTime = 60;
    public bool canRespawn = false;
    public bool spawnsAtStart = false;
    public bool randomAmount;
    int ammoAllowedToRemoveFromThisPack;

    public void Start()
    {
        defaultAmmo = ammoInThisPack;
        disablePack();

        if (randomAmount && spawnsAtStart)
            enablePack(RandomAmmo());
        else if (!randomAmount && spawnsAtStart)
            enablePack(defaultAmmo);

        StartCoroutine(Respawn(spawnTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sphereCollider.enabled && other.gameObject.tag == "player")
        {
            pInventory = other.gameObject.GetComponent<Player>().playerInventory;
            aSource = other.gameObject.GetComponent<AllPlayerScripts>().weaponPickUp.ammoPickupAudioSource;

            //ammoAllowedToRemoveFromThisPack = pInventory.maxSmallAmmo - pInventory.smallAmmo;

            //if (pInventory.smallAmmo != pInventory.maxSmallAmmo)
            //    aSource.Play();


            //if (ammoInThisPack <= ammoAllowedToRemoveFromThisPack)
            //{
            //    pInventory.smallAmmo = pInventory.smallAmmo + ammoInThisPack;
            //    ammoInThisPack = 0;
            //}
            //else if (ammoInThisPack > ammoAllowedToRemoveFromThisPack)
            //{
            //    pInventory.smallAmmo = pInventory.smallAmmo + ammoAllowedToRemoveFromThisPack;
            //    ammoInThisPack = ammoInThisPack - ammoAllowedToRemoveFromThisPack;

            //    ammoText.text = ammoInThisPack.ToString();
            //}

            if (ammoInThisPack <= 0)
            {
                allChildren.SetActive(false);
                sphereCollider.enabled = false;

                if (!canRespawn)
                    Destroy(this.gameObject);
            }
        }
    }

    IEnumerator Respawn(float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);

        if (!randomAmount)
            enablePack(defaultAmmo);
        else
            enablePack(RandomAmmo());

        if (canRespawn)
            StartCoroutine(Respawn(respawnTime));
    }

    int RandomAmmo()
    {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, (48 * 0.7f)));
        return ranAmmo;
    }

    void enablePack(int ammoCount)
    {
        allChildren.SetActive(true);
        sphereCollider.enabled = true;
        ammoInThisPack = ammoCount;
        ammoText.text = ammoInThisPack.ToString();
    }

    void disablePack()
    {
        allChildren.SetActive(false);
        sphereCollider.enabled = false;
        ammoInThisPack = 0;
        ammoText.text = "";
    }
}
