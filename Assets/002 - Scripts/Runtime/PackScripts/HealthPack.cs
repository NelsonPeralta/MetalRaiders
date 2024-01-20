using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public GameObject model;




    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Health Pack {other.name}");
        if (other.GetComponent<PlayerCapsule>() && other.GetComponent<PlayerCapsule>().transform.root.GetComponent<Player>().hitPoints <= 0.7f * other.GetComponent<PlayerCapsule>().transform.root.GetComponent<Player>().maxHitPoints)
        {
            Player p = other.GetComponent<PlayerCapsule>().transform.root.GetComponent<Player>();

            p.hitPoints = p.maxHitPoints;
            p.playerShield.ShowShieldRechargeEffect();
            p.playerShield.PlayShieldStartSound(p);
            SwarmManager.instance.DisableHealthPack_MasterCall(transform.position);


            bool _achievementUnlocked = false;
            Steamworks.SteamUserStats.GetAchievement("MEDIC", out _achievementUnlocked);

            if (!_achievementUnlocked)
            {
                Debug.Log($"Unlocked Achivement MEDIC");
                AchievementManager.UnlockAchievement("MEDIC");
            }
        }
    }
}
