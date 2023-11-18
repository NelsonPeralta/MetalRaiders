using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ManCannon : MonoBehaviour
{
    [SerializeField] int power;
    [SerializeField] float _blockMovementTime;
    [SerializeField] AudioClip onTriggerAudioClip;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ManCannon {other}");
        if (other.gameObject.activeInHierarchy)
        {
            //try
            //{
            //    if (other.GetComponent<IMoveable>() != null)
            //    {
            //        Debug.Log("Found IMoveable");
            //        other.GetComponent<IMoveable>().Push(transform.up, power, PushSource.ManCannon, _blockMovement);

            //        GetComponent<AudioSource>().clip = onTriggerAudioClip;
            //        GetComponent<AudioSource>().Play();
            //        return;
            //    }
            //}
            //catch { }


            try
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();

                if (rb == null)
                    if (other.GetComponent<PlayerCapsule>())
                        if (other.transform.root.GetComponent<PhotonView>().IsMine)
                        {
                            rb = other.transform.root.GetComponent<Rigidbody>();
                            rb.GetComponent<PlayerMovement>().blockPlayerMoveInput = _blockMovementTime;
                        }

                Debug.Log($"ManCannon LAUNCH!");
                rb.velocity = Vector3.zero;
                rb.AddForce(transform.up * power, ForceMode.Impulse);

                GetComponent<AudioSource>().clip = onTriggerAudioClip;
                GetComponent<AudioSource>().Play();
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }
    }
}
