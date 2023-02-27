using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Breather : Actor
{
    [SerializeField] Fireball _fireBallPrefab;

    float _meleeCooldown;
    float _throwFireballCooldown;

    bool isInRange;

    private void OnEnable()
    {
        hitPoints += FindObjectOfType<SwarmManager>().currentWave * 8;
    }

    public override void CooldownsUpdate()
    {
        if (_meleeCooldown > 0)
            _meleeCooldown -= Time.deltaTime;

        if (_throwFireballCooldown > 0)
            _throwFireballCooldown -= Time.deltaTime;
    }


    public override void AnalyzeNextAction()
    {
        if (!GetComponent<PhotonView>().IsMine)
            return;
        if (target)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= closeRange)
            {
                nma.enabled = false;

                if (_meleeCooldown <= 0)
                {
                    BreatherMelee();
                }
                else
                {

                }
            }
            else if (distanceToTarget > closeRange && distanceToTarget <= longRange)
            {
                if (distanceToTarget > closeRange && distanceToTarget <= midRange)
                {
                    if (!isInRange)
                        isInRange = true;
                }


                if (isInRange)
                {
                    if (_throwFireballCooldown <= 0)
                    {
                        Debug.Log("Throw Fireball to Player");
                        BreatherThrowFireBall();
                    }
                }
                else
                {
                    if (!isRunning)
                    {
                        Debug.Log("Chase Player");
                        BreatherRun();
                    }
                    nma.enabled = true;
                    nma.SetDestination(target.position);
                }
            }
            else if (distanceToTarget > longRange)
            {
                if (isInRange)
                    isInRange = false;

                if (!isRunning)
                {
                    //Debug.Log("Chase Player");
                    BreatherRun();
                }
                nma.enabled = true;
                nma.SetDestination(target.position);
            }


        }
        else // Stop Chasing
        {
            if (hitPoints > 0)
                if (!isIdling)
                    BreatherIdle();
            //nma.isStopped = true;
        }
    }





    public override void ChildPrepare()
    {
        isInRange = false;
    }





    [PunRPC]
    void BreatherMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherAttack", RpcTarget.All, false);
            target.GetComponent<Player>().Damage(4, false, pid);
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

    [PunRPC]
    void BreatherThrowFireBall(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherThrowFireBall", RpcTarget.All, false);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw Fireball");

            Vector3 dir = (target.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var proj = Instantiate(_fireBallPrefab.gameObject, losSpawn.transform.position
                , Quaternion.LookRotation(dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(proj.GetComponent<Collider>(), c.GetComponent<Collider>());
            proj.GetComponent<Fireball>().damage = 8;
            proj.GetComponent<Fireball>().force = 250;
            proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
            Destroy(proj, 5);
            _throwFireballCooldown = 2f;
        }
    }

    [PunRPC]
    void BreatherIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherIdle", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void BreatherRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherRun", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }
}
