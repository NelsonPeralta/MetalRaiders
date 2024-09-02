using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class Tyrant_Old : AiAbstractClass
{
    [Header("Combat")]
    public int projectileDamage;
    public int projectileSpeed;
    public List<Transform> minionSpawnPoints = new List<Transform>();

    [Header("Prefabs")]
    public GameObject projectile;
    public GameObject explosion;

    public enum TyrantActions { Block, Fireball, Summon, Seek, Idle }
    [SerializeField] TyrantActions _tyrantAction;

    int _minionsToSpawn;
    public TyrantActions tyrantAction
    {
        get { return _tyrantAction; }
        set
        {
            if (_tyrantAction != value)
            {
                _tyrantAction = value;
                InvokeOnActionChanged();
            }
        }
    }
    public override void OnEnable()
    {
        tyrantAction = TyrantActions.Seek;
        seek = true;
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        PlayerRange newPlayerRange = aiAbstractClass.playerRange;
        PlayerRange previousPlayerRange = aiAbstractClass.previousPlayerRange;
        TyrantActions previousAction = tyrantAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            if (newPlayerRange == PlayerRange.Medium)
                seek = false;
            if (newPlayerRange == PlayerRange.Medium && (previousPlayerRange == PlayerRange.Close || previousPlayerRange == PlayerRange.Long))
            {
                if (ran == 0)
                    previousAction = TyrantActions.Summon;
                else
                    previousAction = TyrantActions.Fireball;
            }
            else if (newPlayerRange == PlayerRange.Out)
                previousAction = TyrantActions.Seek;
        }
        else
        {
            previousAction = TyrantActions.Seek;
        }

        if (newPlayerRange == PlayerRange.Close)
            previousAction = TyrantActions.Block;
        else if (newPlayerRange == PlayerRange.Out)
            previousAction = TyrantActions.Seek;

        ChangeAction(previousAction.ToString());
    }

    public override void DoAction()
    {
        int ran = Random.Range(0, 3);
        TyrantActions previousWatcherAction = tyrantAction;

        if (playerRange == PlayerRange.Medium || playerRange == PlayerRange.Long)
        {
            seek = false;
            if (ran == 0)
                previousWatcherAction = TyrantActions.Summon;
            else
                previousWatcherAction = TyrantActions.Fireball;
        }

        if (playerRange == PlayerRange.Out)
            previousWatcherAction = TyrantActions.Seek;

        if (!isDead && targetPlayer)
        {

            if (previousWatcherAction != TyrantActions.Block && previousWatcherAction != TyrantActions.Idle)
            {
                animator.SetBool("Block", false);
            }

            if (previousWatcherAction != TyrantActions.Seek)
            {
                seek = false;
            }


            if (previousWatcherAction == TyrantActions.Block)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
                {
                    animator.SetBool("Block", true);
                }
            }
            else if (previousWatcherAction == TyrantActions.Fireball)
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
            else if (previousWatcherAction == TyrantActions.Summon)
            {
                if (canDoAction)
                {
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Summon");

                    _minionsToSpawn = minionSpawnPoints.Count;
                    StartCoroutine(SpawnHellhound_Coroutine());
                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousWatcherAction == TyrantActions.Seek)
            {
                seek = true;
            }
        }
        else if (!isDead && !targetPlayer)
        {
            tyrantAction = TyrantActions.Idle;
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

    protected override void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null , bool isHeadshot = false)
    {
        if (isDead)
            return;
        GetComponent<PhotonView>().RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI, damageSource, isHeadshot);
    }

    [PunRPC]
    public override void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null , bool isHeadshot = false)
    {
        if (isDead)
            return;

        Player pp = GameManager.GetPlayerWithPhotonView(playerWhoShotPDI);
        pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        health -= damage;
        if (isDead)
        {
            SpawnKillFeed(this.GetType().ToString(), playerWhoShotPDI, damageSource: damageSource, isHeadshot: isHeadshot);

            pp.GetComponent<PlayerSwarmMatchStats>().kills++;
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);
        }
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        if (!targetInLineOfSight)
            tyrantAction = TyrantActions.Seek;
        else
        {
            Debug.Log($"Target in line of sight. Player range: {playerRange}");
            if (playerRange == PlayerRange.Medium)
                tyrantAction = TyrantActions.Fireball;
            else if (playerRange == PlayerRange.Long)
                tyrantAction = TyrantActions.Summon;
        }
    }

    [PunRPC]
    public override void ChangeAction_RPC(string actionString)
    {
        tyrantAction = (TyrantActions)System.Enum.Parse(typeof(TyrantActions), actionString);
    }

    IEnumerator SpawnHellhound_Coroutine()
    {
        Debug.Log(_minionsToSpawn);
        SwarmManager.instance.SpawnAi(SwarmManager.AiType.Helldog, minionSpawnPoints[_minionsToSpawn - 1]);
        _minionsToSpawn--;
        yield return new WaitForSeconds(0.1f);
        if (_minionsToSpawn > 0)
            StartCoroutine(SpawnHellhound_Coroutine());
    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        GameObject ex = Instantiate(explosion, transform.position + new Vector3(0, 0.75f, 0), transform.rotation);
        Destroy(ex, 2);
    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        projectileDamage += SwarmManager.instance.currentWave * 2;
    }
}
