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
        Log.Print(() =>$"ManCannon {other.name} {other.transform.root.name}");
        if (other.gameObject.activeInHierarchy)
        {
            Log.Print(() =>$"ManCannon {other.name} active");

            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (!rb && other.GetComponent<OddballCollider>() != null) rb = other.GetComponent<OddballCollider>().rb;



            if (rb == null)
            {
                Log.Print(() =>$"ManCannon null");

                if (other.GetComponent<PlayerCapsule>() || other.GetComponent<Player>())
                    if (other.transform.root.GetComponent<PhotonView>().IsMine && ((/*!_invisible &&*/ other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput <= 0) || (/*_invisible &&*/ other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput > 0)))
                    {
                        //if (_invisible) PrintOnlyInEditor.Log(other.transform.root.GetComponent<PlayerMovement>().blockPlayerMoveInput);


                        other.transform.root.GetComponent<PlayerController>().DisableSprint();


                        rb = other.transform.root.GetComponent<Rigidbody>();
                        rb.GetComponent<PlayerMovement>().blockPlayerMoveInput = _blockMovementTime;
                        rb.GetComponent<PlayerMovement>().blockedMovementType = PlayerMovement.BlockedMovementType.ManCannon;

                        Log.Print(() => $"ManCannon dir {transform.up * power}");
                        Log.Print(() =>$"ManCannon BEFORE! {rb.linearDamping} {rb.angularDamping} ||| {rb.linearVelocity} {rb.angularVelocity}");
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.useGravity = true;
                        rb.linearDamping = 0;
                        Log.Print(() =>$"ManCannon AFTER! {rb.linearDamping} {rb.angularDamping} ||| {rb.linearVelocity} {rb.angularVelocity}");
                        rb.AddForce(transform.up * power, ForceMode.Impulse);

                        GetComponent<AudioSource>().clip = onTriggerAudioClip;
                        GetComponent<AudioSource>().Play();
                    }
            }
            else
            {

                //if (!_invisible)
                {
                    Log.Print(() => $"ManCannon dir {transform.up * power}");
                    //Log.Print(() =>$"ManCannon LAUNCH! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    //Log.Print(() =>$"ManCannon LAUNCH! {rb.drag} {rb.angularDrag} ||| {rb.velocity} {rb.angularVelocity}");
                    rb.AddForce(transform.up * power, ForceMode.Impulse);

                    GetComponent<AudioSource>().clip = onTriggerAudioClip;
                    GetComponent<AudioSource>().Play();
                }
            }
        }
        else
        {
            Log.Print(() =>$"ManCannon {other.name} inactive");
        }
    }
}
