using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoSeller : MonoBehaviour
{
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip sellingNoise;
    float exitTriggerRadius;

    [Header("Seller Info")]
    public string ammoType;
    public int cost;

    [Header("Players in Range")]
    public Player player0;
    public Player player1;
    public Player player2;
    public Player player3;

    private void Start()
    {
        exitTriggerRadius = gameObject.GetComponent<SphereCollider>().radius;
        audioSource.clip = sellingNoise;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            Player player = other.gameObject.GetComponent<Player>();

            if (player.playerRewiredID == 0)
            {
                player0 = player;

                if (player0.gameObject.GetComponent<PlayerSwarmMatchStats>().GetPoints() >= cost && player0.gameObject.GetComponent<PlayerSwarmMatchStats>().GetPoints() > 0)
                {
                    player0.GetComponent<PlayerUI>().weaponInformerText.text = "Hold E to Refill " + ammoType.ToString() + " Ammo for: " + cost.ToString() + " Points";
                }
                else
                {
                    player0.GetComponent<PlayerUI>().weaponInformerText.text = "Not enough Points (" + cost.ToString() + ")";
                }
            }
        }
    }

    private void Update()
    {
        if (player0 != null)
        {
            if (player0.GetComponent<PlayerController>().rewiredPlayer.GetButtonShortPressDown("Interact"))
            {
                if (player0.gameObject.GetComponent<PlayerSwarmMatchStats>() != null)
                {
                    if (player0.gameObject.GetComponent<PlayerSwarmMatchStats>().GetPoints() >= cost && player0.gameObject.GetComponent<PlayerSwarmMatchStats>().GetPoints() > 0)
                    {
                        if (ammoType == "Small")
                        {
                            //if (player0.playerInventory.smallAmmo < player0.playerInventory.maxSmallAmmo)
                            //{
                            //    sellAmmo("Small", player0.gameObject.GetComponent<PlayerSwarmMatchStats>(), player0.playerInventory);
                            //}
                        }

                        if (ammoType == "Heavy")
                        {

                            //if (player0.playerInventory.heavyAmmo < player0.playerInventory.maxHeavyAmmo)
                            //{
                            //    sellAmmo("Heavy", player0.gameObject.GetComponent<PlayerSwarmMatchStats>(), player0.playerInventory);
                            //}
                        }

                        if (ammoType == "Power")
                        {
                            //if (player0.playerInventory.powerAmmo < player0.playerInventory.maxPowerAmmo)
                            //{
                            //    sellAmmo("Power", player0.gameObject.GetComponent<PlayerSwarmMatchStats>(), player0.playerInventory);
                            //}
                        }
                    }
                }
            }

            checkPlayerDistance0();
        }
    }

    void sellAmmo(string ammoType, PlayerSwarmMatchStats pPoints, PlayerInventory pInventory)
    {
            pPoints.RemovePoints(cost);
        //if (ammoType == "Small")
        //        pInventory.smallAmmo = pInventory.maxSmallAmmo;

        //if (ammoType == "Heavy")
        //    pInventory.heavyAmmo = pInventory.maxHeavyAmmo;

        //if (ammoType == "Power")
        //    pInventory.powerAmmo = pInventory.maxPowerAmmo;

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
                player0.GetComponent<PlayerUI>().weaponInformerText.text = "";
                player0 = null;
            }
        }
    }

}
