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


            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb == null)
            {

                if (other.GetComponent<PlayerCapsule>())
                    if (other.transform.root.GetComponent<PhotonView>().IsMine && other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput <= 0)
                    {
                        other.transform.root.GetComponent<PlayerController>().DisableSprint();


                        rb = other.transform.root.GetComponent<Rigidbody>();
                        rb.GetComponent<PlayerMovement>().blockPlayerMoveInput = _blockMovementTime;

                        print($"ManCannon dir {transform.up * power}");
                        Debug.Log($"ManCannon BEFORE! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.useGravity = true;
                        rb.drag = 0;
                        Debug.Log($"ManCannon AFTER! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                        rb.AddForce(transform.up * power, ForceMode.Impulse);

                        GetComponent<AudioSource>().clip = onTriggerAudioClip;
                        GetComponent<AudioSource>().Play();
                    }
            }
            else
            {
                print($"ManCannon dir {transform.up * power}");
                //Debug.Log($"ManCannon LAUNCH! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                //Debug.Log($"ManCannon LAUNCH! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                rb.AddForce(transform.up * power, ForceMode.Impulse);

                GetComponent<AudioSource>().clip = onTriggerAudioClip;
                GetComponent<AudioSource>().Play();
            }
        }
    }
}
