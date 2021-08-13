using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMeleeTrigger : MonoBehaviour
{
    [Header("Basic AiS")]
    public ZombieScript zombie;
    public Skeleton skeleton;    
    public Troll troll;

    [Header("Boss AiS")]
    public Hellhound hellhound;
    public Wererat wererat;

    public GameObject player;
    public PlayerProperties pProperties;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            player = other.gameObject;
            pProperties = other.gameObject.GetComponent<PlayerProperties>();

            if (zombie != null)
            {
                zombie.IsInMeleeRange = true;
                Debug.Log("Zombie In Melee Range");
            }

            if (skeleton != null)
            {
                skeleton.IsInMeleeRange = true;
            }

            if (hellhound != null)
            {
                hellhound.IsInMeleeRange = true;
            }

            if (troll != null)
            {
                troll.IsInMeleeRange = true;
            }

            if (wererat != null)
            {
                wererat.IsInMeleeRange = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        player = other.gameObject;
        pProperties = other.gameObject.GetComponent<PlayerProperties>();
    }

    private void OnTriggerExit(Collider other)
    {
        player = null;
        pProperties = null;

        if (zombie != null)
        {
            zombie.IsInMeleeRange = false;
        }

        if (skeleton != null)
        {
            skeleton.IsInMeleeRange = false;
        }

        if (hellhound != null)
        {
            hellhound.IsInMeleeRange = false;
        }

        if (troll!= null)
        {
            troll.IsInMeleeRange = false;
        }

        if (wererat != null)
        {
            wererat.IsInMeleeRange = false;
        }
    }
}
