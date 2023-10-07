using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Breather : Actor
{
    [SerializeField] Fireball _fireBallPrefab;

    [SerializeField] AudioClip _hurtClip;

    float _meleeCooldown;
    float _throwFireballCooldown;

    bool isInRange;

    protected override void ChildOnEnable()
    {
        _hitPoints += (SwarmManager.instance.currentWave * 12 * FindObjectsOfType<Player>().Length);
    }

    protected override void ChildOnActorDamaged()
    {
        if (PhotonNetwork.IsMasterClient)
            if (_flinchCooldown <= 0)
            {
                BreatherFlinch();
            }
    }




    [PunRPC]
    void BreatherMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherAttack", RpcTarget.All, false);
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

            Vector3 dir = (targetTransform.position - new Vector3(0, 1.5f, 0)) - transform.position;
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

    [PunRPC]
    void BreatherFlinch(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherFlinch", RpcTarget.All, false);
        }
        else
        {
            try
            {
                GetComponent<AudioSource>().clip = _hurtClip;
                GetComponent<AudioSource>().Play();

                nma.enabled = false;
                _animator.Play("Flinch");
                _flinchCooldown = 1.6f;
            }
            catch { }
        }
    }

    public override void Idle(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void Run(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void Melee(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }
}
