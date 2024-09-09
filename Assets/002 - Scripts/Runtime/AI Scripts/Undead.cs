using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undead : Actor
{
    float _targetLostCooldown, _checkForBarricadeCooldown;

    RaycastHit _hit;


    protected override void ChildOnEnable()
    {
        _hitPoints = _defaultHitpoints + FindObjectOfType<SwarmManager>().currentWave * 8;
        _checkForBarricadeCooldown = 0.25f;
    }



    protected override void ChildUpdate()
    {
        if (_checkForBarricadeCooldown > 0)
        {
            _checkForBarricadeCooldown -= Time.deltaTime;

            if (_checkForBarricadeCooldown < 0)
            {
                if (Physics.Raycast(losSpawn.transform.position, losSpawn.transform.forward, out _hit, 1, GameManager.instance.hideLayerMask))
                {
                    if (_hit.transform.GetComponent<ZombieBarricade>() && _hit.transform.GetComponent<ZombieBarricade>().hitpoints > 0)
                    {
                        print($"undead sees barricade {_meleeCooldown} {_hardIdleTime} {isMeleeing}");
                        UndeadIdle(hardIdle: true);
                        if (_meleeCooldown <= 0)
                        {
                            if (PhotonNetwork.IsMasterClient)
                                UndeadDamageBarricade(GameManager.instance.hazards.IndexOf(_hit.transform.GetComponent<Hazard>()));
                        }
                    }
                }

                _checkForBarricadeCooldown = 0.1f;
            }
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


    [PunRPC]
    void UndeadDamageBarricade(int hazardInd, bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("UndeadDamageBarricade", RpcTarget.All, hazardInd, false);
            GameManager.instance.hazards[hazardInd].GetComponent<ZombieBarricade>().Damage();
        }
        else
        {
            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();

            _animator.Play("Melee");
            _meleeCooldown = 2;
        }
    }








    [PunRPC]
    void UndeadIdle(bool caller = true, bool hardIdle = false)
    {
        if (caller)
        {
            print("calling undeadidle");
            GetComponent<PhotonView>().RPC("UndeadIdle", RpcTarget.All, false, hardIdle);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");
            print("undeadidle");

            if(hardIdle)_hardIdleTime = 3;

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
            Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }

    public override void Idle(bool callRPC = true)
    {
        if (!isMeleeing)
            UndeadIdle(callRPC);
    }

    public override void Run(bool callRPC = true)
    {
        UndeadRun(callRPC);
    }

    public override void Melee(bool callRPC = true)
    {
        UndeadAttack(callRPC);
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        UndeadRun(callRPC);
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        UndeadRun(callRPC);
    }

    protected override void ChildAwake()
    {
        SwarmManager.instance.zombieList.Add(this);
    }
}
