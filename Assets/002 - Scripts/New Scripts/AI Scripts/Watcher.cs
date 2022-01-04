using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class Watcher : AiAbstractClass
{
    public Animator animator;
    public AudioSource aSource;
    public SwarmMode swarmMode;
    public MyPlayerManager pManager;

    [Header("Combat")]
    public int projectileDamage;
    public int projectileSpeed;
    public int meteorDamage;
    public int meteorSpeed;
    public GameObject projectileSpawnPoint;
    public GameObject motionTrackerDot;

    [Header("Prefabs")]
    public GameObject projectile;
    public GameObject meteor;
    public GameObject wall;
    public GameObject deathSmoke;

    [Header("Sounds")]
    public AudioClip summonWall;

    [Header("Shield")]
    public GameObject shieldModel;
    public SphereCollider shieldCollider;


    public enum WatcherActions { Defend, Fireball, Meteor, Seek }
    WatcherActions _watcherAction;

    public WatcherActions watcherAction
    {
        get { return _watcherAction; }
        set
        {
            if(_watcherAction != value)
            {
                _watcherAction = value;
                Debug.Log($"Watcher action change: {_watcherAction}");
                InvokeOnActionChanged();
            }
        }
    }
    private void Start()
    {
        shieldModel.SetActive(false);
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        WatcherActions previousAction = watcherAction;

        if (targetInLineOfSight)
        {
            if (aiAbstractClass.playerRange == PlayerRange.Medium)
                previousAction = WatcherActions.Fireball;
            else if (aiAbstractClass.playerRange == PlayerRange.Long)
                previousAction = WatcherActions.Meteor;
            else if (aiAbstractClass.playerRange == PlayerRange.Out)
                previousAction = WatcherActions.Seek;
        }
        else
        {
            previousAction = WatcherActions.Seek;
        }

        if (aiAbstractClass.playerRange == PlayerRange.Close)
            previousAction = WatcherActions.Defend;
        else if (aiAbstractClass.playerRange == PlayerRange.Out)
            previousAction = WatcherActions.Seek;

        watcherAction = previousAction;
    }

    public override void DoAction()
    {
        if (isDead)
            return;

        if (watcherAction != WatcherActions.Defend)
        {
            animator.SetBool("Defend", false);
            shieldModel.SetActive(false);
        }

        if(watcherAction != WatcherActions.Seek)
        {
            seek = false;
        }


        if (watcherAction == WatcherActions.Defend)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
            {
                animator.SetBool("Defend", true);
                shieldModel.SetActive(true);
            }
        }
        else if (watcherAction == WatcherActions.Fireball)
        {
            if (canDoAction)
            {
                animator.Play("Projectile");

                var proj = Instantiate(projectile, projectileSpawnPoint.transform.position
                    , projectileSpawnPoint.transform.rotation);
                proj.GetComponent<Fireball>().damage = projectileDamage;
                proj.GetComponent<Fireball>().force = projectileSpeed;
                proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                Destroy(proj, 5);

                nextActionCooldown = defaultNextActionCooldown;
            }
        }
        else if (watcherAction == WatcherActions.Meteor)
        {
            if (canDoAction)
            {
                animator.Play("Summon");

                var pSurro = target.GetComponent<PlayerProperties>().pSurroundings;
                var meteo = Instantiate(meteor, pSurro.top.transform.position + new Vector3(0, 10, 0), pSurro.top.transform.rotation);
                meteo.GetComponent<Fireball>().radius = 3;
                meteo.GetComponent<Fireball>().damage = meteorDamage;
                meteo.GetComponent<Fireball>().force = meteorSpeed;
                meteo.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                meteo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                meteo.transform.Rotate(180, 0, 0);

                nextActionCooldown = defaultNextActionCooldown;
            }
        }
        else if (watcherAction == WatcherActions.Seek)
        {
            seek = true;
        }
        Debug.Log($"Do Watcher action: {watcherAction}");
    }

    public override void ChildUpdate()
    {
        if (!target)
            return;

        Vector3 targetPostition = new Vector3(target.position.x,
                                        this.transform.position.y,
                                        target.position.z);
        this.transform.LookAt(targetPostition);
    }

    public override void Damage(int damage, int playerWhoShotPDI)
    {
        if (isDead)
            return;
        GetComponent<PhotonView>().RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI);
    }

    [PunRPC]
    public override void Damage_RPC(int damage, int playerWhoShotPDI)
    {
        if (isDead)
            return;

        PlayerProperties pp = GameManager.instance.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(damage);

        health -= damage;
        if (isDead)
        {
            pp.GetComponent<OnlinePlayerSwarmScript>().kills++;
            pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(defaultHealth);
        }
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        if (!targetInLineOfSight)
            watcherAction = WatcherActions.Seek;
        else
        {
            Debug.Log($"Target in line of sight. Player range: {playerRange}");
            if (playerRange == PlayerRange.Medium)
                watcherAction = WatcherActions.Fireball;
            else if (playerRange == PlayerRange.Long)
                watcherAction = WatcherActions.Meteor;
        }
    }
}
