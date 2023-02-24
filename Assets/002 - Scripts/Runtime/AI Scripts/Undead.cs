using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undead : Actor
{
    float _meleeCooldown;
    float _targetLostCooldown;



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
        if (target)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= closeRange)
            {
                if (_meleeCooldown <= 0)
                {
                    Debug.Log("Punch Player");
                    _animator.Play("Attack");
                    nma.enabled = false;
                    _meleeCooldown = 1;
                }
            }
            else if (distanceToTarget > closeRange)
            {
                Debug.Log("Chase Player");
                _animator.Play("Run");
                nma.enabled = true;
                nma.SetDestination(target.position);
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
            _animator.Play("Idle");
            nma.enabled = false;
            //nma.isStopped = true;
        }
    }
}
