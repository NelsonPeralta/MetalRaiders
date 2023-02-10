using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public GameObject model;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            Player p = other.GetComponent<Player>();

            p.hitPoints = p.maxHitPoints;
            SwarmManager.instance.DisableHealthPack_MasterCall(transform.position);
        }
    }
}
