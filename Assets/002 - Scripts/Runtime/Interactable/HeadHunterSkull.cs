using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadHunterSkull : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>())
        {
            if (!other.gameObject.GetComponent<Player>().isDead && !other.gameObject.GetComponent<Player>().isRespawning)
            {
                other.GetComponent<PlayerMultiplayerMatchStats>().score++;
                other.GetComponent<PlayerUI>().AddInformerText($"Skulls: {other.GetComponent<PlayerMultiplayerMatchStats>().score}");
               transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
