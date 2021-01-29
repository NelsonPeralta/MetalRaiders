using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallHealthPowerUp : MonoBehaviour
{
    PlayerProperties pProperties;
    public int healthToGive;
    public AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>())
        {
            if (!other.gameObject.GetComponent<PlayerProperties>().isDead)
            {
                pProperties = other.gameObject.GetComponent<PlayerProperties>();

                if (pProperties.healthSlider.value < pProperties.maxHealth && pProperties.needsHealthPack)
                {
                    pProperties.healthSlider.value = pProperties.healthSlider.value + healthToGive;
                    pProperties.Health = pProperties.healthSlider.value;

                    if (pProperties.healthSlider.value > 100 || pProperties.Health > 100)
                    {
                        pProperties.healthSlider.value = 100;
                        pProperties.Health = 100;                        
                    }

                    audioSource.Play();
                    gameObject.layer = 23;
                    Destroy(gameObject, 1);
                }
            }
        }
    }
}
