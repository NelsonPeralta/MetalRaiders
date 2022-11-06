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
    public float damageCountdown;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            if (instantKillzone)
            {
                other.gameObject.GetComponent<Player>().Damage(999);
                return;
            }
            playersInRange.Add(other.GetComponent<Player>());
            if (!instantKillzone)
                other.GetComponent<ScreenEffects>().orangeScreen.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            other.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);
            RemovePlayerFromRange(other.GetComponent<Player>());
        }
    }

    IEnumerator DamagePlayersInRange_Coroutine()
    {
        yield return new WaitForSeconds(damageDelay);

        if (playersInRange.Count > 0)
            for (int i = 0; i < playersInRange.Count; i++)
            {
                playersInRange[i].Damage(damage, false, 99);
                if (instantKillzone)
                    RemovePlayerFromRange(playersInRange[i]);
            }
        StartCoroutine(DamagePlayersInRange_Coroutine());
    }
    void RemovePlayerFromRange(Player pp)
    {
        for (int i = 0; i < playersInRange.Count; i++)
        {
            if (pp == playersInRange[i])
                playersInRange.RemoveAt(i);
        }
    }
}
