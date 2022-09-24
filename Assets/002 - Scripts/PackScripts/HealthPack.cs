using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    Player pProperties;
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
            pProperties = other.gameObject.GetComponent<Player>();

            pProperties.hitPoints = pProperties.maxHitPoints;
            SwarmManager.instance.DisableHealthPack_MasterCall(transform.position);
        }
    }
}
