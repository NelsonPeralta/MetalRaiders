using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GrenadeAmmoPack : MonoBehaviour
{
    [Header("Classes")]
    public PlayerInventory pInventory;
    public GameObject allChildren;
    public SphereCollider sphereCollider;
    public AudioSource aSource;
    public TextMeshPro ammoText;

    [Header("Ammo")]
    public int ammoInThisPack = 4;
    public float spawnTime = 180;
    public bool canRespawn = false;
    public bool spawnsAtStart = false;
    public bool randomAmount;
    int ammoAllowedToRemoveFromThisPack;

    public void Start()
    {
        if (randomAmount)
            RandomAmmo();
        ammoText.text = ammoInThisPack.ToString();
        if (!spawnsAtStart)
            StartCoroutine(Respawn(spawnTime));
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            pInventory = other.gameObject.GetComponent<PlayerProperties>().pInventory;
            aSource = other.gameObject.GetComponent<AllPlayerScripts>().weaponPickUp.ammoPickupAudioSource;

            ammoAllowedToRemoveFromThisPack = pInventory.maxGrenades - pInventory.grenades;

            if (pInventory.grenades != pInventory.maxGrenades)
                aSource.Play();


            if (ammoInThisPack <= ammoAllowedToRemoveFromThisPack)
            {
                pInventory.grenades = pInventory.grenades + ammoInThisPack;
                ammoInThisPack = 0;
            }
            else if(ammoInThisPack > ammoAllowedToRemoveFromThisPack)
            {
                pInventory.grenades = pInventory.grenades + ammoAllowedToRemoveFromThisPack;
                ammoInThisPack = ammoInThisPack - ammoAllowedToRemoveFromThisPack;

                if (ammoText != null)
                {
                    ammoText.text = ammoInThisPack.ToString();
                }
            }

            if (ammoInThisPack == 0)
            {
                if (canRespawn)
                    StartCoroutine(Respawn(spawnTime));
                else
                    Destroy(this.gameObject);
            }
        }
    }

    IEnumerator Respawn(float respawnTime)
    {
        allChildren.SetActive(false);
        sphereCollider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        allChildren.SetActive(true);
        sphereCollider.enabled = true;
        ammoInThisPack = 2;
        if (randomAmount)
            RandomAmmo();

        ammoText.text = ammoInThisPack.ToString();
    }

    void RandomAmmo() { ammoInThisPack = (int)Mathf.Floor(Random.Range(1, 3)); }
}
