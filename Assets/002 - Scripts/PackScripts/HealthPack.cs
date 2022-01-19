using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    PlayerProperties pProperties;
    public GameObject packFX;
    public GameObject motionTrackerIcon;

    [Header("Respawn Settings")]
    public SphereCollider sCollider;
    int spawnTime = 180;

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            pProperties = other.gameObject.GetComponent<PlayerProperties>();

            if(pProperties.healthSlider.value < pProperties.maxHealth && pProperties.needsHealthPack)
            {
                pProperties.health = pProperties.maxHealth;
                pProperties.healthSlider.value = pProperties.health;
                pProperties.PlayHealthRechargeSound();
                SwarmManager.instance.DisableHealthPack(transform.position);
            }
        }
    }
}
