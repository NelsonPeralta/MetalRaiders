using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ManCannon : MonoBehaviour
{
    [SerializeField] int power;
    [SerializeField] AudioClip onTriggerAudioClip;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.activeSelf)
        {
            try
            {
                if (!other.GetComponent<PhotonView>().IsMine)
                    return;
                Rigidbody rb = other.GetComponent<Rigidbody>();
                rb.AddForce(transform.up * power);

                GetComponent<AudioSource>().clip = onTriggerAudioClip;
                GetComponent<AudioSource>().Play();
            }
            catch { }

            try
            {
                if (other.GetComponent<Player>().isDead || other.GetComponent<Player>().isRespawning || !other.GetComponent<Player>().isMine)
                    return;

                CharacterController cc = other.GetComponent<CharacterController>();
                cc.GetComponent<PlayerImpactReceiver>().AddImpact(transform.up, power);

                GetComponent<AudioSource>().clip = onTriggerAudioClip;
                GetComponent<AudioSource>().Play();
            }
            catch { }
        }
    }
}