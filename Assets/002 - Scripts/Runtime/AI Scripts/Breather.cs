using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Breather : Actor
{
    [SerializeField] Fireball _fireBallPrefab;



    protected override void ChildOnEnable()
    {
        //_hitPoints += (SwarmManager.instance.currentWave * 12 * FindObjectsOfType<Player>().Length);
        _flinchCooldown = 0;
    }











    public override void Idle(bool callRPC = true)
    {
        BreatherIdle(callRPC);
    }

    public override void Run(bool callRPC = true)
    {
        BreatherRun(callRPC);
    }
    public override void Melee(bool callRPC = true)
    {
        BreatherMelee(callRPC);
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        BreatherShootFireBall(callRPC);
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        //RibbianThrow(callRPC);
    }











    [PunRPC]
    void BreatherMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("BreatherAttack", RpcTarget.All, false);
            targetTransform.GetComponent<Player>().Damage(22, false, pid);
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
    void BreatherShootFireBall(bool caller = true, Vector3? dir = null)
    {
        Vector3? _dir = dir;
        if (caller)
        {
            _dir = targetTransform.position - new Vector3(0, 1.5f, 0) - transform.position;
            GetComponent<PhotonView>().RPC("BreatherShootFireBall", RpcTarget.All, false, _dir);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw Fireball");

            //Vector3 dir = (targetTransform.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var proj = Instantiate(_fireBallPrefab.gameObject, losSpawn.transform.position
                , Quaternion.LookRotation((Vector3)_dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(proj.GetComponent<Collider>(), c.GetComponent<Collider>());
            proj.GetComponent<Fireball>().damage = 16;
            proj.GetComponent<Fireball>().force = 250;
            proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
            Destroy(proj, 5);
            _shootProjectileCooldown = 2.4f;
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

    protected override void ChildAwake()
    {
        SwarmManager.instance._breathersPool.Add(this);
    }
}
