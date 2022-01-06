using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public OnlineSwarmManager onlineSwarmManagerInstance;
    public ChildManager cManager;
    PlayerProperties pProperties;
    public GameObject packFX;
    public GameObject motionTrackerIcon;

    [Header("Respawn Settings")]
    public SphereCollider sCollider;
    public int spawnTime = 180;

    private void Start()
    {
        onlineSwarmManagerInstance = OnlineSwarmManager.onlineSwarmManagerInstance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            pProperties = other.gameObject.GetComponent<PlayerProperties>();

            if(pProperties.healthSlider.value < pProperties.maxHealth && pProperties.needsHealthPack)
            {
                pProperties.Health = pProperties.maxHealth;
                pProperties.healthSlider.value = pProperties.Health;
                pProperties.PlayHealthRechargeSound();
                onlineSwarmManagerInstance.RespawnHealthPack(transform.position, spawnTime);
                onlineSwarmManagerInstance.DisableHealthPack(transform.position);
            }
        }
    }
}
