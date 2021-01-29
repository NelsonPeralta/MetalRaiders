using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public PlayerProperties player0;
    public PlayerProperties player1;
    public PlayerProperties player2;
    public PlayerProperties player3;

    private void Start()
    {
        //exitTriggerRadius = gameObject.GetComponent<SphereCollider>().radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.gameObject.GetComponent<PlayerProperties>();
            other.GetComponent<ScreenEffects>().orangeScreen.SetActive(true);

            if (player.playerRewiredID == 0)
            {
                player0 = player;
            }

            if (player.playerRewiredID == 1)
            {
                player1 = player;
            }

            if (player.playerRewiredID == 2)
            {
                player2 = player;
            }

            if (player.playerRewiredID == 3)
            {
                player3 = player;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            PlayerProperties player = other.gameObject.GetComponent<PlayerProperties>();
            other.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);

            if (player.playerRewiredID == 0)
            {
                player0 = null;
            }

            if (player.playerRewiredID == 1)
            {
                player1 = null;
            }

            if (player.playerRewiredID == 2)
            {
                player2 = null;
            }

            if (player.playerRewiredID == 3)
            {
                player3 = null;
            }
        }
    }

    private void Update()
    {
        if (player0 != null && !player0.isDead)
        {
            if (instantKillzone)
            {
                player0.BleedthroughDamage(999, false, 99);
                player0 = null;
            }            

            if(damageOverTime)
            {
                if(!playersReceivingDamage)
                {
                    damageCountdown = damageDelay;
                    playersReceivingDamage = true;
                }

                DamageOverTime(player0);
            }

            if (player0.Health <= 0)
                player0 = null;

            //checkPlayerDistance0();
        }

        if (player1 != null && !player1.isDead)
        {         
            if (damageOverTime)
            {
                if (!playersReceivingDamage)
                {
                    damageCountdown = damageDelay;
                    playersReceivingDamage = true;
                }

                DamageOverTime(player1);
            }

            if (instantKillzone)
            {
                player1.BleedthroughDamage(999, false, 99);
                player1 = null;
            }

            if (player1.Health <= 0)
                player1 = null;

            //checkPlayerDistance1();
        }

        if (player2 != null && !player2.isDead)
        {
            if (damageOverTime)
            {
                if (!playersReceivingDamage)
                {
                    damageCountdown = damageDelay;
                    playersReceivingDamage = true;
                }

                DamageOverTime(player2);
            }

            if (instantKillzone)
            {
                player2.BleedthroughDamage(999, false, 99);
                player2 = null;
            }

            if (player2.Health <= 0)
                player2 = null;

            //checkPlayerDistance2();
        }

        if (player3 != null && !player3.isDead)
        {
            if (damageOverTime)
            {
                if (!playersReceivingDamage)
                {
                    damageCountdown = damageDelay;
                    playersReceivingDamage = true;
                }

                DamageOverTime(player3);
            }

            if (instantKillzone)
            {
                player3.BleedthroughDamage(999, false, 99);
                player3 = null;
            }

            if (player3.Health <= 0)
                player3 = null;

            //checkPlayerDistance3();
        }
    }

    void DamageOverTime(PlayerProperties player)
    {
        if (playersReceivingDamage)
        {
            damageCountdown -= Time.deltaTime;

            if (damageCountdown <= 0)
            {
                if (player != null)
                {
                    if (!player.isDead)
                    {
                        if (player.hasShield) //If Player has Shields
                        {
                            if (player.Shield > 0)
                            {
                                player.SetShield(damage);
                            }
                            else
                            {
                                player.SetHealth(damage, false, 99);
                            }
                        }
                        else // If Player does not have Armor
                        {
                            player.SetHealth(damage, false, 99);
                        }
                    }
                }

                playersReceivingDamage = false;
                damageCountdown = damageDelay;
            }
        }
    }

    void checkPlayerDistance0()
    {
        if (player0 != null)
        {
            float playerDistance = Vector3.Distance(player0.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5f)
            {
                player0.InformerText.text = "";
                player0 = null;
            }
        }
    }

    void checkPlayerDistance1()
    {
        if (player1 != null)
        {
            float playerDistance = Vector3.Distance(player1.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5)
            {
                player1.InformerText.text = "";
                player1 = null;
            }
        }
    }

    void checkPlayerDistance2()
    {
        if (player2 != null)
        {
            float playerDistance = Vector3.Distance(player2.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5f)
            {
                player2.InformerText.text = "";
                player2 = null;
            }
        }
    }

    void checkPlayerDistance3()
    {
        if (player3 != null)
        {
            float playerDistance = Vector3.Distance(player3.gameObject.transform.position, gameObject.transform.position);

            if (playerDistance > exitTriggerRadius * 1.5f)
            {
                player3.InformerText.text = "";
                player3 = null;
            }
        }
    }
}
