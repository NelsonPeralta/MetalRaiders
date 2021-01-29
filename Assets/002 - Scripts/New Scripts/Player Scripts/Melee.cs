using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public PlayerController pController;
    public PlayerProperties pProperties;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip knifeSound;

    private void Update()
    {
        Collider[] colliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale);

        foreach (Collider hit in colliders)
        {
            if(hit.gameObject.GetComponent<AIHitbox>() != null)
            {
                var AIHitbox = hit.gameObject.GetComponent<AIHitbox>();

                if (pController.player.GetButtonDown("Melee") && !pController.isMeleeing)
                {
                    AIHitbox.UpdateAIHealthMelee(pProperties.meleeDamage, pProperties.gameObject);
                }
            }

            /*

            if (hit.gameObject.GetComponent<PlayerHitbox>() != null)
            {
                var playerHitbox = hit.gameObject.GetComponent<PlayerHitbox>();
                var playerInZone = playerHitbox.player.GetComponent<PlayerProperties>()

                if (pController.player.GetButtonDown("Melee"))
                {
                    playerHitbox.player.GetComponent<PlayerProperties>().BleedthroughDamage(pProperties.meleeDamage, false, pProperties.playerRewiredID);
                }
            }
            */
        }
    }

    public void PlayMeleeSound()
    {
        audioSource.clip = knifeSound;
        audioSource.Play();
    }
}
