using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undead : Actor
{
    public override void AnalyzeNextAction()
    {
        if (fieldOfView.canSeePlayer) // Chase Player
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= closeRange)
            {
                Debug.Log("Punch Player");
                _animator.Play("Attack");
                nma.isStopped = true;
            }
            if (distanceToTarget > closeRange && distanceToTarget <= midRange)
            {
                Debug.Log("Shoot Player");
                _animator.Play("Idle");
                nma.isStopped = true;
            }
            if (distanceToTarget > midRange && distanceToTarget <= longRange)
            {
                Debug.Log("Chase Player");
                _animator.Play("Run");
                nma.isStopped = false;
                nma.SetDestination(target.position);
            }

        }
        else // Stop Chasing
        {
            _animator.Play("Idle");
            nma.isStopped = true;
        }
    }
}
