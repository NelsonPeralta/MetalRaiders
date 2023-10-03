using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undead : Actor
{
    float _meleeCooldown;
    float _targetLostCooldown;


    protected override void ChildOnEnable()
    {
        _hitPoints += FindObjectOfType<SwarmManager>().currentWave * 8;
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
                    UndeadAttack();
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
                    UndeadRun();
                }
                nma.enabled = true;
                nma.SetDestination(targetTransform.position);
            }

            //float distanceToTarget = Vector3.Distance(transform.position, target.position);
            //if (distanceToTarget <= closeRange)
            //{
            //    if (_meleeCooldown <= 0)
            //    {
            //        Debug.Log("Punch Player");
            //        _animator.Play("Attack");
            //        nma.isStopped = true;
            //        nma.velocity = Vector3.zero;
            //        _meleeCooldown = 1;
            //    }
            //}
            //if (distanceToTarget > closeRange && distanceToTarget <= midRange)
            //{
            //    Debug.Log("Chase Player");
            //    _animator.Play("Run");
            //    nma.isStopped = false;
            //    nma.SetDestination(target.position);
            //}
            //if (distanceToTarget > midRange && distanceToTarget <= longRange)
            //{
            //    Debug.Log("Chase Player");
            //    _animator.Play("Run");
            //    nma.isStopped = false;
            //    nma.SetDestination(target.position);
            //}

        }
        else // Stop Chasing
        {
            if (hitPoints > 0)
                if (!isIdling)
                    UndeadIdle();
            //nma.isStopped = true;
        }
    }



    [PunRPC]
    void UndeadAttack(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("UndeadAttack", RpcTarget.All, false);
            targetTransform.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();

            _animator.SetBool("Run", false);
            _animator.Play("Melee");
            _meleeCooldown = 1;
        }
    }




    public override void ChildPrepare()
    {

    }






    [PunRPC]
    void UndeadIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("UndeadIdle", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void UndeadRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("UndeadRun", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }
}
