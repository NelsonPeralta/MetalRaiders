using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helldog : Actor
{
    float _meleeCooldown;
    float _targetLostCooldown;

    protected override void ChildOnEnable()
    {
        _hitPoints += FindObjectOfType<SwarmManager>().currentWave * 4;
    }

    public override void CooldownsUpdate()
    {
        if (_meleeCooldown > 0)
        {
            _meleeCooldown -= Time.deltaTime;
        }
    }


    public override void AnalyzeNextAction()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        //if (fieldOfView.canSeePlayer) // Chase Player
        if (targetTransform)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            if (distanceToTarget <= closeRange)
            {
                //Debug.Log("Punch Player");
                nma.enabled = false;

                if (_meleeCooldown <= 0)
                {
                    HelldogAttack();
                }
                else
                {
                    //if (!isIdling)
                    //    CloseRange_RPC(false);
                }
            }
            else if (distanceToTarget > closeRange)
            {
                if (!isRunning)
                {
                    //Debug.Log("Chase Player");
                    HelldogRun();
                }
                nma.enabled = true;
                nma.SetDestination(targetTransform.position);
            }

        }
        else // Stop Chasing
        {
            if (hitPoints > 0)
                if (!isIdling)
                    HelldogIdle();
            //nma.isStopped = true;
        }
    }



    [PunRPC]
    void HelldogAttack(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HelldogAttack", RpcTarget.All, false);
            targetTransform.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();

            _animator.SetBool("Run", false);
            _animator.Play("Bite");
            _meleeCooldown = 1;
        }
    }




    public override void ChildPrepare()
    {

    }






    [PunRPC]
    void HelldogIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HelldogIdle", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void HelldogRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HelldogRun", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }
}
