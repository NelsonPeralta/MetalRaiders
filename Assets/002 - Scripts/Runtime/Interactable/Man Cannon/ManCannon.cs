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
        Debug.Log($"ManCannon {other.name} {other.transform.root.name}");
        if (other.gameObject.activeInHierarchy)
        {
            Debug.Log($"ManCannon {other.name} active");

            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (!rb && other.GetComponent<OddballCollider>() != null) rb = other.GetComponent<OddballCollider>().rb;



            if (rb == null)
            {
                Debug.Log($"ManCannon null");

                if (other.GetComponent<PlayerCapsule>() || other.GetComponent<Player>())
                    if (other.transform.root.GetComponent<PhotonView>().IsMine && ((/*!_invisible &&*/ other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput <= 0) || (/*_invisible &&*/ other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput > 0)))
                    {
                        //if (_invisible) print(other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput);


                        other.transform.root.GetComponent<PlayerController>().DisableSprint();


                        rb = other.transform.root.GetComponent<Rigidbody>();
                        rb.GetComponent<PlayerMovement>().blockPlayerMoveInput = _blockMovementTime;
                        rb.GetComponent<PlayerMovement>().blockedMovementType = PlayerMovement.BlockedMovementType.ManCannon;

                        print($"ManCannon dir {transform.up * power}");
                        Debug.Log($"ManCannon BEFORE! {rb.linearDamping} {rb.angularDamping} ||| {rb.linearVelocity} {rb.angularVelocity}");
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.useGravity = true;
                        rb.linearDamping = 0;
                        Debug.Log($"ManCannon AFTER! {rb.linearDamping} {rb.angularDamping} ||| {rb.linearVelocity} {rb.angularVelocity}");
                        rb.AddForce(transform.up * power, ForceMode.Impulse);

                        GetComponent<AudioSource>().clip = onTriggerAudioClip;
                        GetComponent<AudioSource>().Play();
                    }
            }
            else
            {

                //if (!_invisible)
                {
                    print($"ManCannon dir {transform.up * power}");
                    //Debug.Log($"ManCannon LAUNCH! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    //Debug.Log($"ManCannon LAUNCH! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                    rb.AddForce(transform.up * power, ForceMode.Impulse);

                    GetComponent<AudioSource>().clip = onTriggerAudioClip;
                    GetComponent<AudioSource>().Play();
                }
            }
        }
        else
        {
            Debug.Log($"ManCannon {other.name} inactive");
        }
    }
}
