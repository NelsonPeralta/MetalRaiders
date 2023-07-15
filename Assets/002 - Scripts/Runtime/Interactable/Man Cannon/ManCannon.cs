using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ManCannon : MonoBehaviour
{
    [SerializeField] int power;
    [SerializeField] bool _blockMovement;
    [SerializeField] AudioClip onTriggerAudioClip;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.activeSelf)
        {
            try
            {
                if (other.GetComponent<IMoveable>() != null)
                {
                    Debug.Log("Found IMoveable");
                    other.GetComponent<IMoveable>().Push(transform.up, power, PushSource.ManCannon, _blockMovement);

                    GetComponent<AudioSource>().clip = onTriggerAudioClip;
                    GetComponent<AudioSource>().Play();
                    return;
                }
            }
            catch { }


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
        }
    }
}
