using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoSeller : MonoBehaviour
{
    public SwarmMode swarmMode;
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip sellingNoise;
    float exitTriggerRadius;

    [Header("Seller Info")]
    public string ammoType;
    public int cost;

    [Header("Players in Range")]
    public PlayerProperties player0;
    public PlayerProperties player1;
    public PlayerProperties player2;
    public PlayerProperties player3;

    private void Start()
    {
        exitTriggerRadius = gameObject.GetComponent<SphereCollider>().radius;
        audioSource.clip = sellingNoise;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.gameObject.GetComponent<PlayerProperties>();

            if (player.playerRewiredID == 0)
            {
                player0 = player;

                if (player0.gameObject.GetComponent<OnlinePlayerSwarmScript>().GetPoints() >= cost && player0.gameObject.GetComponent<OnlinePlayerSwarmScript>().GetPoints() > 0)
                {
                    player0.InformerText.text = "Hold R to Buy " + ammoType.ToString() + " Ammo for: " + cost.ToString() + " Points";
                }
                else
                {
                    player0.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                }
            }
        }
    }

    private void Update()
    {
        if (player0 != null)
        {
            if (player0.pController.player.GetButtonShortPressDown("Reload") || player0.pController.player.GetButtonDown("Interact"))
            {
                if (player0.gameObject.GetComponent<OnlinePlayerSwarmScript>() != null)
                {
                    if (player0.gameObject.GetComponent<OnlinePlayerSwarmScript>().GetPoints() >= cost && player0.gameObject.GetComponent<OnlinePlayerSwarmScript>().GetPoints() > 0)
                    {
                        if (ammoType == "Small")
                        {
                            if (player0.pInventory.smallAmmo < player0.pInventory.maxSmallAmmo)
                            {
                                sellAmmo("Small", player0.gameObject.GetComponent<OnlinePlayerSwarmScript>(), player0.pInventory);
                            }
                        }

                        if (ammoType == "Heavy")
                        {

                            if (player0.pInventory.heavyAmmo < player0.pInventory.maxHeavyAmmo)
                            {
                                sellAmmo("Heavy", player0.gameObject.GetComponent<OnlinePlayerSwarmScript>(), player0.pInventory);
                            }
                        }

                        if (ammoType == "Power")
                        {
                            if (player0.pInventory.powerAmmo < player0.pInventory.maxPowerAmmo)
                            {
                                sellAmmo("Power", player0.gameObject.GetComponent<OnlinePlayerSwarmScript>(), player0.pInventory);
                            }
                        }
                    }
                }
            }

            checkPlayerDistance0();
        }
    }

    void sellAmmo(string ammoType, OnlinePlayerSwarmScript pPoints, PlayerInventory pInventory)
    {
            pPoints.RemovePoints(cost);
        if (ammoType == "Small")
        {

            int ammoMissing = pInventory.maxSmallAmmo - pInventory.smallAmmo;

            if (ammoMissing <= 24)
            {
                pInventory.smallAmmo = pInventory.smallAmmo + ammoMissing;
            }
            else
            {
                pInventory.smallAmmo += 24;
            }
        }

        if (ammoType == "Heavy")
        {

            int ammoMissing = pInventory.maxHeavyAmmo - pInventory.heavyAmmo;

            if (ammoMissing <= 30)
            {
                pInventory.heavyAmmo = pInventory.heavyAmmo + ammoMissing;
            }
            else
            {
                pInventory.heavyAmmo = pInventory.heavyAmmo + 30;
            }
        }

        if (ammoType == "Power")
        {

            int ammoMissing = pInventory.maxPowerAmmo - pInventory.powerAmmo;

            if (ammoMissing <= 1)
            {
                pInventory.powerAmmo = pInventory.powerAmmo + ammoMissing;
            }
            else
            {
                pInventory.powerAmmo = pInventory.powerAmmo + 4;
            }
        }

        pInventory.UpdateAllExtraAmmoHuds();
        anim.SetTrigger("Jump");
        audioSource.Play();
    }

    void checkPlayerDistance0()
    {
        if (player0 != null)
        {
            float playerDistance = Vector3.Distance(player0.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5f)
            {
                player0.InformerText.text = "";
                player0 = null;
            }
        }
    }

}
