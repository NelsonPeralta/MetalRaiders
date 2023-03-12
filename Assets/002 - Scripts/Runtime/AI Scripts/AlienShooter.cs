using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienShooter : Actor
{
    [SerializeField] Fireball _fireBallPrefab;
    [SerializeField] AIGrenade _grenadePrefab;

    [SerializeField] AudioClip _hurtClip;


    float _meleeCooldown;
    float _throwFireballCooldown;
    float _throwGrenadeCooldown;

    bool isInRange;


    protected override void ChildOnEnable()
    {
        _flinchCooldown = 2.2f;
        hitPoints = _defaultHitpoints + (SwarmManager.instance.currentWave * 12 * FindObjectsOfType<Player>().Length);
    }

    public override void CooldownsUpdate()
    {
        if (_meleeCooldown > 0)
            _meleeCooldown -= Time.deltaTime;

        if (_throwFireballCooldown > 0)
            _throwFireballCooldown -= Time.deltaTime;

        if (_throwGrenadeCooldown > 0)
            _throwGrenadeCooldown -= Time.deltaTime;

        if (_flinchCooldown > 0 && hitPoints < _defaultHitpoints)
            _flinchCooldown -= Time.deltaTime;
    }

    protected override void ChildOnActorDamaged()
    {
        if (PhotonNetwork.IsMasterClient)
            if (_flinchCooldown <= 0)
            {
                AlienShooterFlinch();
            }
    }

    public override void AnalyzeNextAction()
    {
        if (target)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= closeRange)
            {
                nma.enabled = false;

                if (_meleeCooldown <= 0 && !isFlinching)
                {
                    AlienShooterMelee();
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
                    int ran = Random.Range(0, 4);

                    if (ran != 0)
                    {
                        if (_throwFireballCooldown <= 0 && !isTaunting && !isFlinching)
                        {
                            Debug.Log("Throw Fireball to Player");
                            AlienShooterShoot();
                        }
                    }
                    else
                    {
                        if (_throwGrenadeCooldown <= 0 && !isTaunting && !isFlinching)
                        {
                            Debug.Log("Throw Fireball to Player");
                            AlienShooterThrowGrenade();
                        }
                    }
                }
                else
                {
                    if (!isRunning && !isFlinching && !isTaunting)
                    {
                        Debug.Log("Chase Player");
                        AlienShooterRun();
                    }

                    if (isRunning && !isFlinching && !isTaunting)
                    {
                        nma.enabled = true;
                        nma.SetDestination(target.position);
                    }
                    else if (isFlinching || isTaunting)
                        nma.enabled = false;
                }
            }
            else if (distanceToTarget > longRange)
            {
                if (isInRange)
                    isInRange = false;

                if (!isRunning)
                {
                    //Debug.Log("Chase Player");
                    AlienShooterRun();
                }

                if (isRunning && !isFlinching && !isTaunting)
                {
                    nma.enabled = true;
                    nma.SetDestination(target.position);
                }
                else if (isFlinching || isTaunting)
                    nma.enabled = false;
            }


        }
        else // Stop Chasing
        {
            if (hitPoints > 0)
                if (!isIdling)
                    AlienShooterIdle();
            //nma.isStopped = true;
        }
    }





    public override void ChildPrepare()
    {
        isInRange = false;
    }




    [PunRPC]
    void AlienShooterMelee(bool caller = true)
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
    void AlienShooterShoot(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("AlienShooterShoot", RpcTarget.All, false);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Shoot");

            Vector3 dir = (target.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var proj = Instantiate(_fireBallPrefab.gameObject, losSpawn.transform.position
                , Quaternion.LookRotation(dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(proj.GetComponent<Collider>(), c.GetComponent<Collider>());
            proj.GetComponent<Fireball>().damage = 8;
            proj.GetComponent<Fireball>().force = 200;
            proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
            Destroy(proj, 5);
            _throwFireballCooldown = 0.4f;
        }
    }

    [PunRPC]
    void AlienShooterThrowGrenade(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("AlienShooterThrowGrenade", RpcTarget.All, false);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw Grenade");

            Vector3 dir = (target.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var potionBomb = Instantiate(_grenadePrefab.gameObject, losSpawn.transform.position, Quaternion.LookRotation(dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(potionBomb.GetComponent<Collider>(), c.GetComponent<Collider>());

            potionBomb.GetComponent<Rigidbody>().AddForce(losSpawn.transform.forward * 500);

            potionBomb.GetComponent<AIGrenade>().radius = 6;
            potionBomb.GetComponent<AIGrenade>().damage = 16;
            potionBomb.GetComponent<AIGrenade>().playerWhoThrewGrenade = gameObject;

            _throwGrenadeCooldown = 1.5f;
            _throwFireballCooldown = 1f;
        }
    }

    [PunRPC]
    void AlienShooterIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("AlienShooterIdle", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void AlienShooterRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("AlienShooterRun", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }

    [PunRPC]
    void AlienShooterFlinch(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("AlienShooterFlinch", RpcTarget.All, false);
        }
        else
        {
            try
            {
                GetComponent<AudioSource>().clip = _hurtClip;
                GetComponent<AudioSource>().Play();

                nma.enabled = false;
                _animator.Play("Flinch");
                _flinchCooldown = 2.2f;
            }
            catch { }
        }
    }
}
