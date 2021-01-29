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

                if (player0.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player0.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                {
                    player0.InformerText.text = "Buy " + ammoType.ToString() + " Ammo for: " + cost.ToString() + " Points";
                }
                else
                {
                    player0.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                }
            }

            if (player.playerRewiredID == 1)
            {
                player1 = player;

                if (player1.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player1.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                {
                    player1.InformerText.text = "Buy " + ammoType.ToString() + " Ammo for: " + cost.ToString() + " Points";
                }
                else
                {
                    player1.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                }
            }

            if (player.playerRewiredID == 2)
            {
                player2 = player;

                if (player2.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player2.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                {
                    player2.InformerText.text = "Buy " + ammoType.ToString() + " Ammo for: " + cost.ToString() + " Points";
                }
                else
                {
                    player2.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                }
            }

            if (player.playerRewiredID == 3)
            {
                player3 = player;

                if (player3.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player3.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                {
                    player3.InformerText.text = "Buy " + ammoType.ToString() + " Ammo for: " + cost.ToString() + " Points";
                }
                else
                {
                    player3.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
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
                if (player0.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player0.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player0.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        if (ammoType == "Small")
                        {
                            if (player0.pInventory.smallAmmo < player0.pInventory.maxSmallAmmo)
                            {
                                sellAmmo("Small", player0.gameObject.GetComponent<PlayerPoints>(), player0.pInventory);
                            }
                        }

                        if (ammoType == "Heavy")
                        {

                            if (player0.pInventory.heavyAmmo < player0.pInventory.maxHeavyAmmo)
                            {
                                sellAmmo("Heavy", player0.gameObject.GetComponent<PlayerPoints>(), player0.pInventory);
                            }
                        }

                        if (ammoType == "Power")
                        {
                            if (player0.pInventory.powerAmmo < player0.pInventory.maxPowerAmmo)
                            {
                                sellAmmo("Power", player0.gameObject.GetComponent<PlayerPoints>(), player0.pInventory);
                            }
                        }
                    }
                }
            }

            checkPlayerDistance0();
        }

        if (player1 != null)
        {
            if (player1.pController.player.GetButtonShortPressDown("Reload") || player1.pController.player.GetButtonDown("Interact"))
            {
                if (player1.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player1.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player1.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        if (ammoType == "Small")
                        {
                            if (player1.pInventory.smallAmmo < player1.pInventory.maxSmallAmmo)
                            {
                                sellAmmo("Small", player1.gameObject.GetComponent<PlayerPoints>(), player1.pInventory);
                            }
                        }

                        if (ammoType == "Heavy")
                        {

                            if (player1.pInventory.heavyAmmo < player1.pInventory.maxHeavyAmmo)
                            {
                                sellAmmo("Heavy", player1.gameObject.GetComponent<PlayerPoints>(), player1.pInventory);
                            }
                        }

                        if (ammoType == "Power")
                        {
                            if (player1.pInventory.powerAmmo < player1.pInventory.maxPowerAmmo)
                            {
                                sellAmmo("Power", player1.gameObject.GetComponent<PlayerPoints>(), player1.pInventory);
                            }
                        }
                    }
                }
            }

            checkPlayerDistance1();
        }

        if (player2 != null)
        {
            if (player2.pController.player.GetButtonShortPressDown("Reload") || player2.pController.player.GetButtonDown("Interact"))
            {
                if (player2.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player2.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player2.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        if (ammoType == "Small")
                        {
                            if (player2.pInventory.smallAmmo < player2.pInventory.maxSmallAmmo)
                            {
                                sellAmmo("Small", player2.gameObject.GetComponent<PlayerPoints>(), player2.pInventory);
                            }
                        }

                        if (ammoType == "Heavy")
                        {

                            if (player2.pInventory.heavyAmmo < player2.pInventory.maxHeavyAmmo)
                            {
                                sellAmmo("Heavy", player2.gameObject.GetComponent<PlayerPoints>(), player2.pInventory);
                            }
                        }

                        if (ammoType == "Power")
                        {
                            if (player2.pInventory.powerAmmo < player2.pInventory.maxPowerAmmo)
                            {
                                sellAmmo("Power", player2.gameObject.GetComponent<PlayerPoints>(), player2.pInventory);
                            }
                        }
                    }
                }
            }

            checkPlayerDistance2();
        }

        if (player3 != null)
        {
            if (player3.pController.player.GetButtonShortPressDown("Reload") || player3.pController.player.GetButtonDown("Interact"))
            {
                if (player3.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player3.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player3.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        if (ammoType == "Small")
                        {
                            if (player3.pInventory.smallAmmo < player3.pInventory.maxSmallAmmo)
                            {
                                sellAmmo("Small", player3.gameObject.GetComponent<PlayerPoints>(), player3.pInventory);
                            }
                        }

                        if (ammoType == "Heavy")
                        {

                            if (player3.pInventory.heavyAmmo < player3.pInventory.maxHeavyAmmo)
                            {
                                sellAmmo("Heavy", player3.gameObject.GetComponent<PlayerPoints>(), player3.pInventory);
                            }
                        }

                        if (ammoType == "Power")
                        {
                            if (player3.pInventory.powerAmmo < player3.pInventory.maxPowerAmmo)
                            {
                                sellAmmo("Power", player3.gameObject.GetComponent<PlayerPoints>(), player3.pInventory);
                            }
                        }
                    }
                }
            }

            checkPlayerDistance3();
        }
    }

    void sellAmmo(string ammoType, PlayerPoints pPoints, PlayerInventory pInventory)
    {
        if (ammoType == "Small")
        {
            pPoints.swarmPoints = pPoints.swarmPoints - cost;
            pPoints.swarmPointsText.text = pPoints.swarmPoints.ToString();

            int ammoMissing = pInventory.maxSmallAmmo - pInventory.smallAmmo;

            if (ammoMissing <= 48)
            {
                pInventory.smallAmmo = pInventory.smallAmmo + ammoMissing;
            }
            else
            {
                pInventory.smallAmmo = pInventory.smallAmmo + 48;
            }
        }

        if (ammoType == "Heavy")
        {
            pPoints.swarmPoints = pPoints.swarmPoints - cost;
            pPoints.swarmPointsText.text = pPoints.swarmPoints.ToString();

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
            pPoints.swarmPoints = pPoints.swarmPoints - cost;
            pPoints.swarmPointsText.text = pPoints.swarmPoints.ToString();

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

    void checkPlayerDistance1()
    {
        if (player1 != null)
        {
            float playerDistance = Vector3.Distance(player1.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5)
            {
                player1.InformerText.text = "";
                player1 = null;
            }
        }
    }

    void checkPlayerDistance2()
    {
        if (player2 != null)
        {
            float playerDistance = Vector3.Distance(player2.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5f)
            {
                player2.InformerText.text = "";
                player2 = null;
            }
        }
    }

    void checkPlayerDistance3()
    {
        if (player3 != null)
        {
            float playerDistance = Vector3.Distance(player3.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5f)
            {
                player3.InformerText.text = "";
                player3 = null;
            }
        }
    }
}
