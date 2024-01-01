using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class AlienShooter : Actor
{
    [SerializeField] Fireball _fireBallPrefab;
    [SerializeField] AIGrenade _grenadePrefab;

    protected override void ChildOnEnable()
    {
        Debug.Log("Alien Shooter ChildOnEnable " + hitPoints);
        //_hitPoints += (SwarmManager.instance.currentWave * 16 * FindObjectsOfType<Player>().Length);
        Debug.Log("Alien Shooter ChildOnEnable " + hitPoints + (SwarmManager.instance.currentWave * 16 * FindObjectsOfType<Player>().Length));
    }




















    public override void Idle(bool callRPC = true)
    {
        RibbianIdle(callRPC);
    }

    public override void Run(bool callRPC = true)
    {
        RibbianRun(callRPC);
    }
    public override void Melee(bool callRPC = true)
    {
        RibbianMelee(callRPC);
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        RibbianShoot(callRPC);
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        RibbianThrow(callRPC);
    }







    [PunRPC]
    void RibbianIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("RibbianIdle", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void RibbianRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("RibbianRun", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }


    [PunRPC]
    void RibbianMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("RibbianMelee", RpcTarget.All, false);
            targetTransform.GetComponent<Player>().Damage(33, false, pid);
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
    void RibbianShoot(bool caller = true, Vector3? dir = null)
    {
        Vector3? _dir = dir;

        if (caller)
        {
            try { _dir = targetTransform.position - new Vector3(0, 1.5f, 0) - transform.position; } catch { }
            GetComponent<PhotonView>().RPC("RibbianShoot", RpcTarget.All, false, _dir);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Shoot");

            //Vector3 dir = (targetTransform.position - new Vector3(0, 1.5f, 0)) - transform.position;
            Debug.Log(_dir);
            var proj = Instantiate(_fireBallPrefab.gameObject, losSpawn.transform.position
                , Quaternion.LookRotation((Vector3)_dir));


            try
            {
                if (SwarmManager.instance.editMode)
                    proj.GetComponent<Fireball>().damage = 1;
            }
            catch { }


            proj.GetComponent<Fireball>().sourceBiped = gameObject;
            _shootProjectileCooldown = 1.2f;
        }
    }

    [PunRPC]
    void RibbianThrow(bool caller = true, Vector3? dir = null)
    {
        Vector3? _dir = dir;
        if (caller)
        {
            try { _dir = targetTransform.position - new Vector3(0, 1.5f, 0) - transform.position; } catch { }
            GetComponent<PhotonView>().RPC("RibbianThrow", RpcTarget.All, false, _dir);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw");

            //Vector3 dir = (targetTransform.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var potionBomb = Instantiate(_grenadePrefab.gameObject, losSpawn.transform.position, Quaternion.LookRotation((Vector3)_dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(potionBomb.GetComponent<Collider>(), c.GetComponent<Collider>());

            potionBomb.GetComponent<Rigidbody>().AddForce(losSpawn.transform.forward * 600);

            //potionBomb.GetComponent<AIGrenade>().radius = 6;
            //potionBomb.GetComponent<AIGrenade>().damage = 25;
            try
            {
                if (SwarmManager.instance.editMode)
                    potionBomb.GetComponent<AIGrenade>().damage = 1;
            }
            catch { }
            potionBomb.GetComponent<AIGrenade>().playerWhoThrewGrenade = gameObject;

            _throwExplosiveCooldown = 3.1f;
        }
    }

    protected override void ChildAwake()
    {
        SwarmManager.instance._ribbianPool.Add(this);
    }
}
