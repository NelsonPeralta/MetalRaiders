using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Hurtzone : MonoBehaviour
{
    public bool instantKillzone;
    public AudioSource audioSource;

    [Header("Damage Over Time")]
    public int damage;

    [Header("Players in Range")]
    public List<PlayerCapsule> playersInRange = new List<PlayerCapsule>();

    float damageCountdown;

    private void Start()
    {
        if (instantKillzone)
            damage = 9999;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerCapsule>() && !playersInRange.Contains(other.GetComponent<PlayerCapsule>()))
        {
            Player player = other.GetComponent<PlayerCapsule>().player;

            if (instantKillzone)
            {
                if (player.lastPID > 0)
                    player.Damage((int)player.hitPoints, false, player.lastPID);
                else
                    player.Damage((int)player.hitPoints, false, player.photonId);
            }
            else
            {
                player.OnPlayerDeath -= OnPLayerDeath;
                player.OnPlayerDeath += OnPLayerDeath;
                playersInRange.Add(other.GetComponent<PlayerCapsule>());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerCapsule>())
            try { playersInRange.Remove(other.gameObject.GetComponent<PlayerCapsule>()); } catch { }
    }

    private void Update()
    {
        if (instantKillzone)
            return;

        if (damageCountdown > 0)
            damageCountdown -= Time.deltaTime;

        if (damageCountdown <= 0)
        {
            foreach (PlayerCapsule pcap in playersInRange)
            {
                if (pcap.player.isDead || pcap.player.isRespawning)
                    playersInRange.Remove(pcap);
            }

            for (int i = 0; i < playersInRange.Count; i++)
            {
                playersInRange[i].player.BasicDamage(damage);
            }

            damageCountdown = 0.22f;
        }
    }



    void OnPLayerDeath(Player p)
    {
        p.OnPlayerDeath -= OnPLayerDeath;

        if (playersInRange.Contains(p.playerCapsule)) playersInRange.Remove(p.playerCapsule);
    }
}
