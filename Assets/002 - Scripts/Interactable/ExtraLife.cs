using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraLife : MonoBehaviour
{
    public AudioSource audioSource;
    public int numberOfLives;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.GetComponent<PlayerProperties>();

            if (!player.isDead)
            {
                if (player.swarmMode != null)
                {
                    if (player.swarmMode.playerLives < 9)
                    {
                        player.swarmMode.playerLives = player.swarmMode.playerLives + numberOfLives;

                        if(player.swarmMode.playerLives > 9)
                        {
                            player.swarmMode.playerLives = 9;
                        }

                        player.swarmMode.UpdatePlayerLives();
                        audioSource.Play();
                        gameObject.layer = 23;
                        Destroy(gameObject, 1);
                    }
                }
            }
        }
    }
}
