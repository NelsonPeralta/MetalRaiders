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
    [SerializeField] WatcherActions _watcherAction;

    public WatcherActions watcherAction
    {
        get { return _watcherAction; }
        set
        {
            if (_watcherAction != value)
            {
                _watcherAction = value;
                InvokeOnActionChanged();
            }
        }
    }
    public override void OnEnable()
    {
        watcherAction = WatcherActions.Seek;
        seek = true;
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        Debug.Log("OnPlayerRangeChange_Delegate ");
        PlayerRange newPlayerRange = aiAbstractClass.playerRange;
        PlayerRange previousPlayerRange = aiAbstractClass.previousPlayerRange;
        WatcherActions previousAction = watcherAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            Debug.Log("OnPlayerRangeChange_Delegate targetInLineOfSight");

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
            Debug.Log("OnPlayerRangeChange_Delegate ELSE targetInLineOfSight");

            previousAction = WatcherActions.Seek;
        }

        if (newPlayerRange == PlayerRange.Close)
            previousAction = WatcherActions.Defend;
        else if (newPlayerRange == PlayerRange.Out)
            previousAction = WatcherActions.Seek;

        Debug.Log($"OnPlayerRangeChange_Delegate ELSE {previousAction}");


        ChangeAction(previousAction.ToString());
    }

    public override void DoAction()
    {
        int ran = Random.Range(0, 3);
        WatcherActions previousWatcherAction = watcherAction;

        if (playerRange == PlayerRange.Medium || playerRange == PlayerRange.Long)
        {
            if (ran == 0)
                previousWatcherAction = WatcherActions.Meteor;
            else
                previousWatcherAction = WatcherActions.Fireball;
        }

        if (playerRange == PlayerRange.Out)
            previousWatcherAction = WatcherActions.Seek;

        if (!isDead && targetPlayer)
        {

            if (previousWatcherAction != WatcherActions.Defend && previousWatcherAction != WatcherActions.Idle)
            {
                animator.SetBool("Defend", false);
                shieldModel.SetActive(false);
            }

            if (previousWatcherAction != WatcherActions.Seek)
            {
                seek = false;
            }


            if (previousWatcherAction == WatcherActions.Defend)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
                {
                    animator.SetBool("Defend", true);
                    shieldModel.SetActive(true);
                }
            }
            else if (previousWatcherAction == WatcherActions.Fireball)
            {
                if (canDoAction)
                {
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Projectile");

                    var proj = Instantiate(projectile, projectileSpawnPoint.transform.position
                        , projectileSpawnPoint.transform.rotation);
                    proj.GetComponent<Fireball>().damage = projectileDamage;
                    proj.GetComponent<Fireball>().sourceBiped = gameObject;
                    Destroy(proj, 5);

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousWatcherAction == WatcherActions.Meteor)
            {
                if (canDoAction)
                {
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Summon");

                    var pSurro = targetPlayer.GetComponent<Player>().playerSurroundings;
                    var meteo = Instantiate(meteor, pSurro.top.transform.position + new Vector3(0, 10, 0), pSurro.top.transform.rotation);
                    meteo.GetComponent<Fireball>().radius = meteorRadius;
                    meteo.GetComponent<Fireball>().damage = meteorDamage;
                    meteo.GetComponent<Fireball>().sourceBiped = gameObject;
                    meteo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    meteo.transform.Rotate(180, 0, 0);

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousWatcherAction == WatcherActions.Seek)
            {
                seek = true;
            }
        }
        else if (!isDead && !targetPlayer)
        {
            watcherAction = WatcherActions.Idle;
            seek = false;
        }
    }

    public override void ChildUpdate()
    {
        if (!targetPlayer)
            return;

        Vector3 targetPostition = new Vector3(targetPlayer.position.x,
                                        this.transform.position.y,
                                        targetPlayer.position.z);
        this.transform.LookAt(targetPostition);
    }

    protected override void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (isDead)
            return;
        photonView.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI, damageSource, isHeadshot);
    }

    [PunRPC]
    public override void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (isDead)
            return;

        Player pp = GameManager.GetPlayerWithPhotonView(playerWhoShotPDI);
        pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        health -= damage;
        if (isDead)
        {
            //SpawnKillFeed(this.GetType().ToString(), playerWhoShotPDI, damageSource: damageSource, isHeadshot: isHeadshot);

            pp.GetComponent<PlayerSwarmMatchStats>().kills++;
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);
        }
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        Debug.Log($"Target in line of sight. Player range");
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

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {

    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        meteorDamage += SwarmManager.instance.currentWave * 2;
        projectileDamage += SwarmManager.instance.currentWave * 2;
    }
}
