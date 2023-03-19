using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Hurtzone : MonoBehaviour
{
    public AudioSource audioSource;
    float exitTriggerRadius;

    [Header("Damage Over Time")]
    public bool damageOverTime;
    public int damage;
    public float damageDelay;
    float damageCountdown;
    bool playersReceivingDamage;

    [Header("Other")]
    public bool instantKillzone;

    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();

    private void Start()
    {
        if (instantKillzone)
            damage = 9999;
        //StartCoroutine(DamagePlayersInRange_Coroutine());
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            if (instantKillzone)
            {
                Player player = other.GetComponent<Player>();
                //other.gameObject.GetComponent<Player>().BasicDamage(999);
                if (player.lastPID > 0)
                    player.Damage((int)player.hitPoints, false, player.lastPID);
                else
                    player.Damage((int)player.hitPoints, false, player.pid);
                return;
            }
            playersInRange.Add(other.GetComponent<Player>());
            //if (!instantKillzone)
            //    other.GetComponent<ScreenEffects>().orangeScreen.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<Player>())
        {
            //other.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);
            try { playersInRange.Remove(other.gameObject.GetComponent<Player>()); } catch { }
        }
    }

    private void Update()
    {
        if (instantKillzone)
            return;

        if (damageCountdown > 0)
            damageCountdown -= Time.deltaTime;

        if (damageCountdown <= 0)
        {
            foreach (Player player in playersInRange)
            {
                if (player.isDead || player.isRespawning)
                    playersInRange.Remove(player);
            }

            for (int i = 0; i < playersInRange.Count; i++)
            {
                playersInRange[i].BasicDamage(damage);
            }

            damageCountdown = 0.35f;
        }
    }
}
