using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHitbox : MonoBehaviour
{
    public AiAbstractClass aiAbstractClass;
    public GameObject aiGO;
    public float aiHealth;

    [Header("AI Scripts")]
    public ZombieScript zScript;
    public Skeleton skeleton;
    public Watcher watcher;
    public Troll troll;

    [Header("Boss AIs")]
    public Hellhound hellhound;
    public Wererat wererat;
    public BlackKnight blackKnight;
    public FlameTyrant flameTyrant;

    public string team;
    public bool AIisDead = false;

    public bool isHead = false;

    [Header("Other Hitboxes")]
    public Hitboxes otherHitboxes;

    private void Start()
    {
        if (zScript != null)
            UpdateAIHealthOnHitboxes(zScript.Health);

        if (skeleton != null)
            UpdateAIHealthOnHitboxes(skeleton.Health);

        if (watcher != null)
            UpdateAIHealthOnHitboxes(watcher.Health);

        if (troll != null)
            UpdateAIHealthOnHitboxes(troll.Health);

        ///////////////////////////////////////////// Bosses

        if (hellhound != null)
            UpdateAIHealthOnHitboxes(hellhound.Health);

        if (wererat != null)
            UpdateAIHealthOnHitboxes(wererat.Health);

        if (blackKnight != null)
            UpdateAIHealthOnHitboxes(blackKnight.Health);

        if (flameTyrant != null)
            UpdateAIHealthOnHitboxes(flameTyrant.Health);
    }

    public void DamageAI(bool damageFromPlayer, float damage, GameObject playerWhoShot)
    {
        if (zScript)
        {
            zScript.Health = zScript.Health - damage;
            UpdateAIHealthOnHitboxes(zScript.Health);

            if (zScript.isDead)
            {
                AIisDead = true;
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                zScript.TargetSwitch(playerWhoShot);
                zScript.TransferDamageToPoints(damageToInt);
            }
        }

        if (skeleton != null) ///Skeleton
        {
            skeleton.Health = skeleton.Health - damage;
            UpdateAIHealthOnHitboxes(skeleton.Health);

            if (skeleton.isDead)
            {
                AIisDead = true;
            }

            if (!skeleton.isGuarding)
            {
                if (!skeleton.shieldIsBroken)
                {
                    StartCoroutine(skeleton.Guard());
                }
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                skeleton.TargetSwitch(playerWhoShot);
                skeleton.TransferDamageToPoints(damageToInt);
            }
        }

        if (watcher != null) ///Watcher
        {
            watcher.Health = watcher.Health - damage;
            UpdateAIHealthOnHitboxes(watcher.Health);

            if (watcher.isDead)
            {
                AIisDead = true;
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                watcher.TargetSwitch(playerWhoShot);
                watcher.TransferDamageToPoints(damageToInt);
            }
        }

        if (troll != null) /// Hellhound
        {
            troll.Health = troll.Health - damage;
            UpdateAIHealthOnHitboxes(troll.Health);

            if (troll.isDead)
            {
                AIisDead = true;
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                troll.TargetSwitch(playerWhoShot);
                troll.TransferDamageToPoints(damageToInt);
            }
        }

        /////////////////////////////////////////////// Bosses

        if (hellhound != null) /// Hellhound
        {
            Debug.Log("In Update AI Health for Hellhound " + damage);
            hellhound.Health = hellhound.Health - damage;
            UpdateAIHealthOnHitboxes(hellhound.Health);

            if (hellhound.isDead)
            {
                AIisDead = true;
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                hellhound.TargetSwitch(playerWhoShot);
                hellhound.TransferDamageToPoints(damageToInt);
            }
        }

        if (wererat != null) /// Hellhound
        {
            wererat.Health = wererat.Health - damage;
            UpdateAIHealthOnHitboxes(wererat.Health);

            if (wererat.isDead)
            {
                AIisDead = true;
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                wererat.TargetSwitch(playerWhoShot);
                wererat.TransferDamageToPoints(damageToInt);
            }
        }

        if (blackKnight != null)
        {
            if (!blackKnight.shieldIsActive)
            {
                blackKnight.Health = blackKnight.Health - damage;
                UpdateAIHealthOnHitboxes(blackKnight.Health);

                if (blackKnight.isDead)
                {
                    AIisDead = true;
                }
                int damageToInt = Mathf.CeilToInt(damage);

                if (damageFromPlayer)
                {
                    blackKnight.TargetSwitch(playerWhoShot);
                    blackKnight.TransferDamageToPoints(damageToInt);
                }
            }
        }

        if (flameTyrant != null)
        {
            flameTyrant.Health = flameTyrant.Health - damage;
            UpdateAIHealthOnHitboxes(flameTyrant.Health);

            if (flameTyrant.isDead)
            {
                AIisDead = true;
            }
            int damageToInt = Mathf.CeilToInt(damage);

            if (damageFromPlayer)
            {
                flameTyrant.TargetSwitch(playerWhoShot);
                flameTyrant.TransferDamageToPoints(damageToInt);
            }
        }
    }

    public void UpdateAIHealthMelee(float meleeDamage, GameObject playerWhoShot)
    {
        if (zScript != null)
        {
            if (!zScript.hasBeenMeleedRecently)
            {
                zScript.Health = zScript.Health - meleeDamage;
                zScript.hasBeenMeleedRecently = true;
                StartCoroutine(zScript.MeleeReset());
                UpdateAIHealthOnHitboxes(zScript.Health);

                if (zScript.isDead)
                {
                    AIisDead = true;
                }
            }
        }

        if (skeleton != null)
        {
            if (!skeleton.hasBeenMeleedRecently)
            {
                skeleton.Health = skeleton.Health - meleeDamage;
                skeleton.hasBeenMeleedRecently = true;
                StartCoroutine(skeleton.MeleeReset());
                UpdateAIHealthOnHitboxes(skeleton.Health);

                if (skeleton.isDead)
                {
                    AIisDead = true;
                }

                skeleton.TargetSwitch(playerWhoShot);

                if (!skeleton.isGuarding)
                {
                    if (skeleton.simpleLOS.playerInLineOfSight && !skeleton.shieldIsBroken)
                    {
                        StartCoroutine(skeleton.Guard());
                    }
                }
            }
        }

        if (watcher != null)
        {
            if (!watcher.hasBeenMeleedRecently)
            {
                watcher.Health = watcher.Health - meleeDamage;
                watcher.hasBeenMeleedRecently = true;
                StartCoroutine(watcher.MeleeReset());
                UpdateAIHealthOnHitboxes(watcher.Health);

                if (watcher.isDead)
                {
                    AIisDead = true;
                }

                watcher.TargetSwitch(playerWhoShot);
            }
        }

        if (troll != null)
        {
            if (!troll.hasBeenMeleedRecently)
            {
                troll.Health = troll.Health - meleeDamage;
                troll.hasBeenMeleedRecently = true;
                StartCoroutine(troll.MeleeReset());
                UpdateAIHealthOnHitboxes(troll.Health);

                if (troll.isDead)
                {
                    AIisDead = true;
                }

                troll.TargetSwitch(playerWhoShot);
            }
        }

        ////////////////////////////////////////////////// Bosses

        if (hellhound != null)
        {
            if (!hellhound.hasBeenMeleedRecently)
            {
                hellhound.Health = hellhound.Health - meleeDamage;
                hellhound.hasBeenMeleedRecently = true;
                StartCoroutine(hellhound.MeleeReset());
                UpdateAIHealthOnHitboxes(hellhound.Health);

                if (hellhound.isDead)
                {
                    AIisDead = true;
                }

                hellhound.TargetSwitch(playerWhoShot);
            }
        }

        if (wererat != null)
        {
            if (!wererat.hasBeenMeleedRecently)
            {
                wererat.Health = wererat.Health - meleeDamage;
                wererat.hasBeenMeleedRecently = true;
                StartCoroutine(wererat.MeleeReset());
                UpdateAIHealthOnHitboxes(wererat.Health);

                if (wererat.isDead)
                {
                    AIisDead = true;
                }

                wererat.TargetSwitch(playerWhoShot);
            }
        }

        if (blackKnight != null)
        {
            if (!blackKnight.hasBeenMeleedRecently)
            {
                if (!blackKnight.shieldIsActive)
                {
                    blackKnight.Health = blackKnight.Health - meleeDamage;
                    blackKnight.hasBeenMeleedRecently = true;
                    StartCoroutine(blackKnight.MeleeReset());
                    UpdateAIHealthOnHitboxes(blackKnight.Health);

                    if (blackKnight.isDead)
                    {
                        AIisDead = true;
                    }

                    blackKnight.TargetSwitch(playerWhoShot);
                }
            }
        }

        if (flameTyrant != null)
        {
            if (!flameTyrant.hasBeenMeleedRecently)
            {
                flameTyrant.Health = flameTyrant.Health - meleeDamage;
                flameTyrant.hasBeenMeleedRecently = true;
                StartCoroutine(flameTyrant.MeleeReset());
                UpdateAIHealthOnHitboxes(flameTyrant.Health);

                if (flameTyrant.isDead)
                {
                    AIisDead = true;
                }

                flameTyrant.TargetSwitch(playerWhoShot);
            }
        }
    }

    void UpdateAIHealthOnHitboxes(float newHealth)
    {
        foreach (AIHitbox hitbox in otherHitboxes.AIHitboxes)
        {
            if (hitbox != null)
            {
                hitbox.aiHealth = newHealth;
            }
        }
    }
}
