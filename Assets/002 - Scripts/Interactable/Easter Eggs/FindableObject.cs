using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindableObject : MonoBehaviour
{
    public EasterEggFindThreeObjects find3Objects;

    [Header("Seller Info")]
    public bool isActivated;

    [Header("Players in Range")]
    public PlayerProperties player0;
    public PlayerProperties player1;
    public PlayerProperties player2;
    public PlayerProperties player3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.gameObject.GetComponent<PlayerProperties>();

            if (player.playerRewiredID == 0)
            {
                player0 = player;

                if (!isActivated)
                {
                    player0.InformerText.text = "Interact";
                }
            }

            if (player.playerRewiredID == 1)
            {
                player1 = player;

                if (!isActivated)
                {
                    player1.InformerText.text = "Interact";
                }
            }

            if (player.playerRewiredID == 2)
            {
                player2 = player;

                if (!isActivated)
                {
                    player2.InformerText.text = "Interact";
                }
            }

            if (player.playerRewiredID == 3)
            {
                player3 = player;

                if (!isActivated)
                {
                    player3.InformerText.text = "Interact";
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.gameObject.GetComponent<PlayerProperties>();

            if (player.playerRewiredID == 0 && player0)
            {
                player0.InformerText.text = "";
                player0 = null;
            }

            if (player.playerRewiredID == 1 && player1)
            {
                player1.InformerText.text = "";
                player1 = null;
            }

            if (player.playerRewiredID == 2 && player2)
            {
                player2.InformerText.text = "";
                player2 = null;
            }

            if (player.playerRewiredID == 3 && player3)
            {
                player3.InformerText.text = "";
                player3 = null;
            }
        }
    }

    private void Update()
    {
        if (find3Objects != null)
        {
            if (player0 != null)
            {
                if (player0.pController.player.GetButtonDown("Interact") || player0.pController.player.GetButtonDown("Reload"))
                {
                    if (!isActivated)
                    {
                        find3Objects.updateActivatedObjects(this);
                        player0.InformerText.text = "";
                        player0 = null;
                    }
                }
            }

            if (player1 != null)
            {
                if (player1.pController.player.GetButtonDown("Interact") || player1.pController.player.GetButtonDown("Reload"))
                {
                    if (!isActivated)
                    {
                        find3Objects.updateActivatedObjects(this);
                        player1.InformerText.text = "";
                        player1 = null;
                    }
                }
            }

            if (player2 != null)
            {
                if (player2.pController.player.GetButtonDown("Interact") || player2.pController.player.GetButtonDown("Reload"))
                {
                    if (!isActivated)
                    {
                        find3Objects.updateActivatedObjects(this);
                        player2.InformerText.text = "";
                        player2 = null;
                    }
                }
            }

            if (player3 != null)
            {
                if (player3.pController.player.GetButtonDown("Interact") || player3.pController.player.GetButtonDown("Reload"))
                {
                    if (!isActivated)
                    {
                        find3Objects.updateActivatedObjects(this);
                        player3.InformerText.text = "";
                        player3 = null;
                    }
                }
            }
        }
    }
}
