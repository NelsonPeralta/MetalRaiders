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
        Log.Print(() =>"Alien Shooter ChildOnEnable " + hitPoints);
        //_hitPoints += (SwarmManager.instance.currentWave * 16 * FindObjectsOfType<Player>().Length);
        Log.Print(() =>"Alien Shooter ChildOnEnable " + hitPoints + (SwarmManager.instance.currentWave * 16 * FindObjectsOfType<Player>().Length));
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
            //Log.Print(() =>"UndeadIdle RPC");

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
            //Log.Print(() =>"UndeadRun RPC");

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
            Log.Print(() => $"Master client is trying to melee {targetTransform.name}");
            if (targetTransform.GetComponent<Player>())
                targetTransform.GetComponent<Player>().Damage(33, false, pid);
            else if (targetTransform.root.GetComponent<Player>())
                targetTransform.GetComponent<Player>().Damage(33, false, pid);
        }
        else
        {
            //Log.Print(() =>"Punch Player RPC");

            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();

            _animator.SetBool("Run", false);
            _animator.Play("Melee");
            _meleeCooldown = 1;

            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic) _meleeCooldown = 0.9f;
            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary) _meleeCooldown = 0.8f;
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
            //Log.Print(() =>"Punch Player RPC");
            _isCurrentlyShootingCooldown = 1;

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Shoot");



            ActorAddonsPool.instance.SpawnPooledFireball(losSpawn.transform.position, (Vector3)_dir, gameObject, 12, 42, 2, 50);
            _shootProjectileCooldown = 1.2f;

            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic) _shootProjectileCooldown = 1f;
            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary) _shootProjectileCooldown = 0.8f;
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
            //Log.Print(() =>"Punch Player RPC");
            SwarmManager.instance.globalActorGrenadeCooldown = 4;
            _isCurrentlyThrowingGrenadeCooldown = 1;
            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw");


            ActorAddonsPool.instance.SpawnPooledGrenade(losSpawn.transform.position, (Vector3)_dir, gameObject, actorHitboxes, 250, 7, 700);
            _throwExplosiveCooldown = 5f;

            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic) _throwExplosiveCooldown = 4f;
            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary) _throwExplosiveCooldown = 3f;
        }
    }

    protected override void ChildAwake()
    {
        SwarmManager.instance._ribbianPool.Add(this);
    }
}
