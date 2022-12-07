using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overshield : MonoBehaviour
{
    [SerializeField] int _tts;
    private void Start()
    {
        try { NetworkGameManager.instance.overshield = this; } catch { }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() && !other.GetComponent<Player>().isDead && !other.GetComponent<Player>().isRespawning)
        {
            NetworkGameManager.instance.StartOverShieldRespawn(_tts);
            other.GetComponent<Player>().maxOvershieldPoints = 150;
            gameObject.SetActive(false);
        }
    }
}
