using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallHealthPowerUp : MonoBehaviour
{
    Player pProperties;
    public int healthToGive;
    public AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>())
        {
            if (!other.gameObject.GetComponent<Player>().isDead)
            {
                pProperties = other.gameObject.GetComponent<Player>();

                //if (pProperties.healthSlider.value < pProperties.maxHitPoints && pProperties.needsHealthPack)
                //{
                //    pProperties.healthSlider.value = pProperties.healthSlider.value + healthToGive;
                //    pProperties.hitPoints = pProperties.healthSlider.value;

                //    if (pProperties.healthSlider.value > 100 || pProperties.hitPoints > 100)
                //    {
                //        pProperties.healthSlider.value = 100;
                //        pProperties.hitPoints = 100;                        
                //    }

                //    audioSource.Play();
                //    gameObject.layer = 23;
                //    Destroy(gameObject, 1);
                //}
            }
        }
    }
}
