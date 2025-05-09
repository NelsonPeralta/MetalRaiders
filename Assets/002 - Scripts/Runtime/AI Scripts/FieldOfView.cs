using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;

    public GameObject playerRef { get { return _actor.targetHitpoints.gameObject; } }

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;

    [SerializeField] GameObject _obstruction;
    [SerializeField] string _state;
    [SerializeField] List<Collider> _playersInRangeRadius = new List<Collider>();

    Actor _actor;
    RaycastHit hit;
    Ray ray;


    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(FOVRoutine()); // Stops when obj is disabled
    }

    private void Start()
    {
        _actor = GetComponent<Actor>();
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {



        _obstruction = null;
        if (_actor.hitPoints <= 0)
        {
            canSeePlayer = false;

            return;
        }

        try { radius = GetComponent<Actor>().longRange; } catch { }
        Transform or = GetComponent<Actor>().losSpawn;
        _playersInRangeRadius = Physics.OverlapSphere(transform.position, radius, targetMask).
            Where(item => item.transform.root.GetComponent<Player>() && item.transform.root.GetComponent<Player>().isAlive).ToList();

        if (_playersInRangeRadius.Count > 0)
        {
            if (_actor.isPatroling)
            {
                foreach (Collider col in _playersInRangeRadius)
                {
                    Transform target = col.transform.root;
                    Vector3 directionToTarget = (target.position - or.position).normalized;

                    if (Vector3.Angle(or.forward, directionToTarget) < angle / 2)
                    {
                        float distanceToTarget = Vector3.Distance(or.position, target.position);
                        hit = new RaycastHit();
                        ray = new Ray(or.position, directionToTarget);

                        if (!Physics.Raycast(ray, out hit, distanceToTarget, obstructionMask))
                        {
                            _actor.targetHitpoints = col.transform.root.GetComponent<HitPoints>();
                            _actor.targetTransform = target;
                        }
                        else
                        {

                        }
                    }

                }
            }
            else if (_actor.targetHitpoints)
                if (_playersInRangeRadius.ToList().Contains(_actor.targetHitpoints.GetComponent<Player>().playerCapsule.GetComponent<Collider>()))
                {
                    Transform target = GetComponent<Actor>().targetHitpoints.transform;
                    Vector3 directionToTarget = (target.position - or.position).normalized;

                    if (Vector3.Angle(or.forward, directionToTarget) < angle / 2)
                    {
                        float distanceToTarget = Vector3.Distance(or.position, target.position);
                        hit = new RaycastHit();
                        ray = new Ray(or.position, directionToTarget);

                        if (!Physics.Raycast(ray, out hit, distanceToTarget, obstructionMask))
                        {

                            canSeePlayer = true;
                            GetComponent<Actor>().targetTransform = target;
                            _state = "1";
                        }
                        else
                        {
                            // BUG: It may detect layer that should NOT be an obstruction

                            //Debug.Log(obstructionMask == (obstructionMask | (1 << hit.transform.gameObject.layer)));

                            // Check if obs layer is part of obs layer mask

                            if (!(obstructionMask == (obstructionMask | (1 << hit.transform.gameObject.layer)))) // Returns true if part of layer mask
                            {
                                _state = "1.5";

                                canSeePlayer = true;
                                GetComponent<Actor>().targetTransform = target;
                            }
                            else
                            {
                                _obstruction = hit.transform.gameObject;
                                _state = "2";

                                canSeePlayer = false;
                            }
                        }
                    }
                    else
                    {
                        _state = "3";

                        canSeePlayer = false;
                    }

                }
                else
                {
                    _state = "Actor can see a player, but it is not his main target";
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
        {

            _state = "5";
            canSeePlayer = false;
        }
    }
}