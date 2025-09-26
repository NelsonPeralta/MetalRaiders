using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tyrant : Actor
{
    [SerializeField] Fireball _fireBallPrefab;
    [SerializeField] List<Transform> _minionSpawnPoints = new List<Transform>();

    protected override void ChildOnEnable()
    {
        //_hitPoints += FindObjectOfType<SwarmManager>().currentWave * 24;
    }














    public override void Idle(bool callRPC = true)
    {
        FlameTyrantIdle(callRPC);
    }

    public override void Run(bool callRPC = true)
    {
        FlameTyrantRun(callRPC);
    }
    public override void Melee(bool callRPC = true)
    {
        FlameTyrantMelee(callRPC);
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        FlameTyrantFireBall(callRPC);
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        FlameTyrantSummon(callRPC);
    }













    [PunRPC]
    void FlameTyrantMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("FlameTyrantMelee", RpcTarget.AllViaServer, false);
            targetTransform.GetComponent<Player>().Damage(62, false, pid);
        }
        else
        {
            Log.Print(() =>"Punch Player RPC");

            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();

            _animator.SetBool("Run", false);
            _animator.Play("Melee");
            _meleeCooldown = 1.5f;
        }
    }

    [PunRPC]
    void FlameTyrantFireBall(bool caller = true, Vector3? dir = null)
    {
        Vector3? _dir = dir;
        if (caller)
        {
            Log.Print(() =>"CALLER FlameTyrantFireBall");
            _dir = targetTransform.position - new Vector3(0, 1.5f, 0) - transform.position;
            GetComponent<PhotonView>().RPC("FlameTyrantFireBall", RpcTarget.AllViaServer, false, _dir);
            //target.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            Log.Print(() =>"FlameTyrantFireBall");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Throw Fireball");

            //Vector3 dir = (targetTransform.position - new Vector3(0, 1.5f, 0)) - transform.position;
            var proj = Instantiate(_fireBallPrefab.gameObject, losSpawn.transform.position
                , Quaternion.LookRotation((Vector3)_dir));
            foreach (ActorHitbox c in actorHitboxes)
                Physics.IgnoreCollision(proj.GetComponent<Collider>(), c.GetComponent<Collider>());
            proj.GetComponent<Fireball>().damage = 32;
            proj.GetComponent<Fireball>().sourceBiped = gameObject;
            Destroy(proj, 5);
            _shootProjectileCooldown = 2f;
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
            //Log.Print(() =>"Punch Player RPC");

            _animator.SetBool("Run", false);
            nma.enabled = false;
            _animator.Play("Summon");

            SwarmManager.instance.SpawnAi(SwarmManager.AiType.Helldog, _minionSpawnPoints[0]);
            SwarmManager.instance.SpawnAi(SwarmManager.AiType.Helldog, _minionSpawnPoints[1]);
            _throwExplosiveCooldown = 3;
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
            //Log.Print(() =>"UndeadIdle RPC");

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
            Log.Print(() =>"FlameTyrantRun RPC");
            _animator.SetBool("Run", true);
        }
    }

    protected override void ChildAwake()
    {
        SwarmManager.instance.tyrantPool.Add(this);
    }
}
