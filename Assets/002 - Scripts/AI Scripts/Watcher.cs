using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class Watcher : AiAbstractClass
{
    [Header("Combat")]
    public int projectileDamage;
    public int projectileSpeed;
    public int meteorDamage;
    public int meteorSpeed;
    public int meteorRadius;

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


    public enum WatcherActions { Defend, Fireball, Meteor, Seek, Idle }
    WatcherActions _watcherAction;

    public WatcherActions watcherAction
    {
        get { return _watcherAction; }
        set
        {
            if(_watcherAction != value)
            {
                _watcherAction = value;
                InvokeOnActionChanged();
            }
        }
    }
    private void Start()
    {
        shieldModel.SetActive(false);
    }

    public override void OnEnable()
    {
        watcherAction  = WatcherActions.Seek;
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        PlayerRange newPlayerRange = aiAbstractClass.playerRange;
        PlayerRange previousPlayerRange = aiAbstractClass.previousPlayerRange;
        WatcherActions previousAction = watcherAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            if (newPlayerRange == PlayerRange.Medium && (previousPlayerRange == PlayerRange.Close || previousPlayerRange == PlayerRange.Long))
            {
                if (ran == 0)
                    previousAction = WatcherActions.Meteor;
                else
                    previousAction = WatcherActions.Fireball;
            }
            else if (newPlayerRange == PlayerRange.Out)
                previousAction = WatcherActions.Seek;
        }
        else
        {
            previousAction = WatcherActions.Seek;
        }

        if (newPlayerRange == PlayerRange.Close)
            previousAction = WatcherActions.Defend;
        else if (newPlayerRange == PlayerRange.Out)
            previousAction = WatcherActions.Seek;

        ChangeAction(previousAction.ToString());
    }

    public override void DoAction()
    {
        int ran = Random.Range(0, 3);
        WatcherActions previousKnightAction = watcherAction;

        if (playerRange == PlayerRange.Medium || playerRange == PlayerRange.Long)
        {
            if (ran == 0)
                previousKnightAction = WatcherActions.Meteor;
            else
                previousKnightAction = WatcherActions.Fireball;
        }

        if (!isDead && target)
        {

            if (previousKnightAction != WatcherActions.Defend && previousKnightAction != WatcherActions.Idle)
            {
                animator.SetBool("Defend", false);
                shieldModel.SetActive(false);
            }

            if (previousKnightAction != WatcherActions.Seek)
            {
                seek = false;
            }


            if (previousKnightAction == WatcherActions.Defend)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
                {
                    animator.SetBool("Defend", true);
                    shieldModel.SetActive(true);
                }
            }
            else if (previousKnightAction == WatcherActions.Fireball)
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
            else if (previousKnightAction == WatcherActions.Meteor)
            {
                if (canDoAction)
                {
                    animator.Play("Summon");

                    var pSurro = target.GetComponent<Player>().pSurroundings;
                    var meteo = Instantiate(meteor, pSurro.top.transform.position + new Vector3(0, 10, 0), pSurro.top.transform.rotation);
                    meteo.GetComponent<Fireball>().radius = meteorRadius;
                    meteo.GetComponent<Fireball>().damage = meteorDamage;
                    meteo.GetComponent<Fireball>().force = meteorSpeed;
                    meteo.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                    meteo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    meteo.transform.Rotate(180, 0, 0);

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousKnightAction == WatcherActions.Seek)
            {
                seek = true;
            }
        }else if(!isDead && !target)
        {
            watcherAction = WatcherActions.Idle;
            animator.SetBool("Idle", true);
        }
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

        Player pp = GameManager.instance.GetPlayerWithPhotonViewId(playerWhoShotPDI);
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

    [PunRPC]
    public override void ChangeAction_RPC(string actionString)
    {
        watcherAction = (WatcherActions)System.Enum.Parse(typeof(WatcherActions), actionString);
    }
}
