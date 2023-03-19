using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tyrant : Actor
{
    [SerializeField] Fireball _fireBallPrefab;
    [SerializeField] List<Transform> _minionSpawnPoints = new List<Transform>();

    float _meleeCooldown;
    float _throwFireballCooldown;
    float _summonlCooldown;

    bool isInRange;

    protected override void ChildOnEnable()
    {
        _flinchCooldown = 2.8f;
        hitPoints = _defaultHitpoints + (SwarmManager.instance.currentWave * 12 * FindObjectsOfType<Player>().Length);
    }

    public override void CooldownsUpdate()
    {
        if (_meleeCooldown > 0)
            _meleeCooldown -= Time.deltaTime;

        if (_throwFireballCooldown > 0)
            _throwFireballCooldown -= Time.deltaTime;

        if (_summonlCooldown > 0)
            _summonlCooldown -= Time.deltaTime;
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
                    FlameTyrantMelee();
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
                    int ran = Random.Range(0, 5);

                    if (ran != 0)
                    {
                        if (_throwFireballCooldown <= 0)
                        {
                            Debug.Log("Throw Fireball to Player");
                            FlameTyrantFireBall();
                        }
                    }
                    else
                    {
                        if (_summonlCooldown <= 0)
                        {
                            Debug.Log("Throw Fireball to Player");
                            FlameTyrantSummon();
                        }
                    }
                }
                else
                {
                    if (!isRunning)
                    {
                        Debug.Log("Chase Player");
                        FlameTyrantRun();
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
                    FlameTyrantRun();
                }
                nma.enabled = true;
                nma.SetDestination(target.position);
            }


        }
        else // Stop Chasing
        {
            if (hitPoints > 0)
                if (!isIdling)
                    FlameTyrantIdle();
            //nma.isStopped = true;
        }
    }





    public override void ChildPrepare()
    {
        isInRange = false;
    }





    [PunRPC]
    void FlameTyrantMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("FlameTyrantMelee", RpcTarget.AllViaServer, false);
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
    void FlameTyrantFireBall(bool caller = true)
    {
        if (caller)
        {
            Debug.Log("CALLER FlameTyrantFireBall");
            GetComponent<PhotonView>().RPC("FlameTyrantFireBall", RpcTarget.AllViaServer, false);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            Debug.Log("FlameTyrantFireBall");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw Fireball");

            Vector3 dir = (target.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var proj = Instantiate(_fireBallPrefab.gameObject, losSpawn.transform.position
                , Quaternion.LookRotation(dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(proj.GetComponent<Collider>(), c.GetComponent<Collider>());
            proj.GetComponent<Fireball>().damage = 14;
            proj.GetComponent<Fireball>().force = 150;
            proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
            Destroy(proj, 5);
            _throwFireballCooldown = 2f;
        }
    }

    [PunRPC]
    void FlameTyrantSummon(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("FlameTyrantSummon", RpcTarget.AllViaServer, false);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Summon");

            SwarmManager.instance.SpawnAi(SwarmManager.AiType.Helldog, _minionSpawnPoints[0]);
            SwarmManager.instance.SpawnAi(SwarmManager.AiType.Helldog, _minionSpawnPoints[1]);

            _summonlCooldown = 4f;
        }
    }

    [PunRPC]
    void FlameTyrantIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("FlameTyrantIdle", RpcTarget.AllViaServer, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void FlameTyrantRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("FlameTyrantRun", RpcTarget.AllViaServer, false);
        }
        else
        {
            Debug.Log("FlameTyrantRun RPC");
            _animator.SetBool("Run", true);
        }
    }
}
