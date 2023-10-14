using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public GameObject model;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() && other.GetComponent<Player>().hitPoints <= 0.7f * other.GetComponent<Player>().maxHitPoints)
        {
            Player p = other.GetComponent<Player>();

            p.hitPoints = p.maxHitPoints;
            p.playerShield.ShowShieldRechargeEffect();
            p.playerShield.PlayShieldStartSound(p);
            SwarmManager.instance.DisableHealthPack_MasterCall(transform.position);
        }
    }
}
