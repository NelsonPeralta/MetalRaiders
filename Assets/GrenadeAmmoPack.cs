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
    int defaultAmmo;
    public int ammoInThisPack = 4;
    public float spawnTime = 180;
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

        if (spawnsAtStart)
            enablePack(defaultAmmo);

        if (canRespawn)
            StartCoroutine(Respawn(spawnTime));
    }
       
    private void OnTriggerEnter(Collider other)
    {
        if (sphereCollider.enabled && other.gameObject.tag == "player")
        {
            pInventory = other.gameObject.GetComponent<Player>().playerInventory;
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

    int RandomAmmo() {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, 3));
        return ranAmmo;
    }

    void enablePack(int ammoCount)
    {
        allChildren.SetActive(true);
        sphereCollider.enabled = true;
        ammoInThisPack = defaultAmmo;
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
