using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupArmor : MonoBehaviour
{
    public SwarmMode swarmMode;
    public AudioSource audioSource;
    public AudioClip sellingNoise;
    public GameObject armorGO;

    [Header("Seller Info")]
    public int cost = -1;
    public bool destroyWhenSold;
    public GameObject weapon;
    public string armorName;

    [Header("Players in Range")]
    public PlayerProperties player0;
    public PlayerProperties player1;
    public PlayerProperties player2;
    public PlayerProperties player3;

    private void Start()
    {
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

                if (armorGO.activeSelf)
                {
                    if (player0.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost)
                    {
                        player0.InformerText.text = "Buy POWER AMMOR for: " + cost.ToString() + " Points";
                    }
                    else
                    {
                        player0.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                    }
                    if(player0.hasShield)
                    {
                        player0.InformerText.text = "You already have an Armor dumbass...";
                    }
                }
            }

            if (player.playerRewiredID == 1)
            {
                player1 = player;

                if (armorGO.activeSelf)
                {
                    if (player1.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost)
                    {
                        player1.InformerText.text = "Buy POWER AMMOR for: " + cost.ToString() + " Points";
                    }
                    else
                    {
                        player1.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                    }
                    if (player1.hasShield)
                    {
                        player1.InformerText.text = "You already have an Armor dumbass...";
                    }
                }
            }

            if (player.playerRewiredID == 2)
            {
                player2 = player;

                if (armorGO.activeSelf)
                {
                    if (player2.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost)
                    {
                        player2.InformerText.text = "Buy POWER AMMOR for: " + cost.ToString() + " Points";
                    }
                    else
                    {
                        player2.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                    }
                    if (player2.hasShield)
                    {
                        player2.InformerText.text = "You already have an Armor dumbass...";
                    }
                }
            }

            if (player.playerRewiredID == 3)
            {
                player3 = player;

                if (armorGO.activeSelf)
                {
                    if (player3.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost)
                    {
                        player3.InformerText.text = "Buy POWER AMMOR for: " + cost.ToString() + " Points";
                    }
                    else
                    {
                        player3.InformerText.text = "Not enough Points (" + cost.ToString() + ")";
                    }
                    if (player3.hasShield)
                    {
                        player3.InformerText.text = "You already have an Armor dumbass...";
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.gameObject.GetComponent<PlayerProperties>();

            if (player.playerRewiredID == 0)
            {
                player0.InformerText.text = "";
                player0 = null;
            }

            if (player.playerRewiredID == 1)
            {
                player1.InformerText.text = "";
                player1 = null;
            }

            if (player.playerRewiredID == 2)
            {
                player2.InformerText.text = "";
                player2 = null;
            }

            if (player.playerRewiredID == 3)
            {
                player3.InformerText.text = "";
                player3 = null;
            }
        }
    }

    private void Update()
    {
        if (player0 != null)
        {
            if (player0.pController.player.GetButtonDown("Interact") ||
                player0.pController.player.GetButtonShortPressDown("Reload"))
            {
                if (player0.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player0.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost /*&& player0.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0*/)
                    {
                        sellPowerArmor(player0.gameObject.GetComponent<PlayerPoints>(), player0);
                    }
                }
            }
        }

        if (player1 != null)
        {
            if (player1.pController.player.GetButtonDown("Interact") ||
                player1.pController.player.GetButtonShortPressDown("Reload"))
            {
                if (player1.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player1.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player1.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        sellPowerArmor(player1.gameObject.GetComponent<PlayerPoints>(), player1);
                    }
                }
            }
        }

        if (player2 != null)
        {
            if (player2.pController.player.GetButtonDown("Interact") ||
                player2.pController.player.GetButtonShortPressDown("Reload"))
            {
                if (player2.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player2.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player2.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        sellPowerArmor(player2.gameObject.GetComponent<PlayerPoints>(), player2);
                    }
                }
            }
        }

        if (player3 != null)
        {
            if (player3.pController.player.GetButtonDown("Interact") ||
                player3.pController.player.GetButtonShortPressDown("Reload"))
            {
                if (player3.gameObject.GetComponent<PlayerPoints>() != null)
                {
                    if (player3.gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && player3.gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
                    {
                        sellPowerArmor(player3.gameObject.GetComponent<PlayerPoints>(), player3);
                    }
                }
            }
        }
    }

    void sellPowerArmor(PlayerPoints pPoints, PlayerProperties pProperties)
    {
        if (!pProperties.hasShield)
        {
            pPoints.swarmPoints = pPoints.swarmPoints - cost;
            pPoints.swarmPointsText.text = pPoints.swarmPoints.ToString();

            pProperties.hasShield = true;
            pProperties.hasMotionTracker = true;

            if (weapon)
            {
                for (int i = 0; i < pProperties.pInventory.allWeaponsInInventory.Length; i++)
                {
                    if (pProperties.pInventory.allWeaponsInInventory[i] != null)
                    {
                        if (weapon.name == pProperties.pInventory.allWeaponsInInventory[i].gameObject.name)
                        {
                            if (!pProperties.pInventory.weaponsEquiped[1])
                            {
                                if (weapon.name != pProperties.pInventory.weaponsEquiped[0].name)
                                {
                                    pProperties.wPickup.weaponCollidingWithInInventory = pProperties.pInventory.allWeaponsInInventory[i].gameObject;
                                    pProperties.wPickup.PickupSecWeap();
                                }
                            }
                            else
                            {
                                if (pProperties.pInventory.activeWeapIs == 0)
                                {
                                    if (weapon.name != pProperties.pInventory.weaponsEquiped[0].gameObject.name)
                                    {
                                        // TO DO: equip player minigun

                                        pProperties.wPickup.weaponCollidingWithInInventory = pProperties.pInventory.allWeaponsInInventory[i].gameObject;
                                        //pProperties.wPickup.ReplaceWeapon(weapon.GetComponent<LootableWeapon>()); // Cant send script using RPC
                                    }
                                }
                                else
                                {
                                    if (weapon.name != pProperties.pInventory.weaponsEquiped[1].gameObject.name)
                                    {
                                        // TO DO: equip player minigun

                                        pProperties.wPickup.weaponCollidingWithInInventory = pProperties.pInventory.allWeaponsInInventory[i].gameObject;
                                        //pProperties.wPickup.ReplaceWeapon(weapon.GetComponent<LootableWeapon>()); // Cant send script using RPC
                                    }
                                }
                            }
                        }
                    }
                }
            }
            pProperties.gameObject.GetComponent<PlayerSkins>().EnableArmor(armorName, pProperties.gameObject);

            audioSource.Play();
            if (destroyWhenSold)
            {
                Destroy(gameObject, 1);
            }
        }
    }
}
