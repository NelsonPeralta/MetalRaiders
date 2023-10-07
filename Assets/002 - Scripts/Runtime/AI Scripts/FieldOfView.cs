using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;

    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;

    [SerializeField] GameObject _firstCollision;

    private void Start()
    {
        playerRef = GameManager.GetRootPlayer().gameObject;
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            try
            {
                FieldOfViewCheck();
            }
            catch { }
        }
    }

    private void FieldOfViewCheck()
    {
        try { radius = GetComponent<Actor>().longRange; } catch { }
        Transform or = GetComponent<Actor>().losSpawn;
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length > 0)
        {
            //try
            //{
            //    RaycastHit hit = new RaycastHit();
            //    Ray ray = new Ray(transform.position, transform.forward);
            //    if (Physics.Raycast(ray, out hit, 20, obstructionMask))
            //        _firstCollision = hit.transform.gameObject;
            //}
            //catch { }

            if (rangeChecks.ToList().Contains(GetComponent<Actor>().targetPlayer.gameObject.GetComponent<Collider>()))
            {
                Transform target = GetComponent<Actor>().targetPlayer.transform;
                Vector3 directionToTarget = (target.position - or.position).normalized;

                if (Vector3.Angle(or.forward, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector3.Distance(or.position, target.position);

                    if (!Physics.Raycast(or.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        canSeePlayer = true;
                        GetComponent<Actor>().targetTransform = target;
                    }
                    else
                    {
                        try
                        {
                            RaycastHit hit = new RaycastHit();
                            Ray ray = new Ray(or.position, directionToTarget);
                            if (Physics.Raycast(ray, out hit, distanceToTarget, obstructionMask))
                                _firstCollision = hit.transform.gameObject;
                        }
                        catch { }

                        canSeePlayer = false;
                    }
                }
                else
                    canSeePlayer = false;

            }
            else
            {
                Debug.Log("Cant see at all");
                canSeePlayer = false;
            }



            // Original Code
            {
                //Transform target = rangeChecks[0].transform;
                //Vector3 directionToTarget = (target.position - transform.position).normalized;

                //if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
                //{
                //    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                //    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                //        canSeePlayer = true;
                //    else
                //        canSeePlayer = false;
                //}
                //else
                //    canSeePlayer = false;
            }
        }
        else if (canSeePlayer)
            canSeePlayer = false;
    }
}