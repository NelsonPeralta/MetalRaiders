using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class Tyrant : AiAbstractClass
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

        if (!isDead && target)
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
            else if (previousWatcherAction == TyrantActions.Summon)
            {
                if (canDoAction)
                {
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
        else if (!isDead && !target)
        {
            tyrantAction = TyrantActions.Idle;
            seek = false;
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
        pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        health -= damage;
        if (isDead)
        {
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
        SwarmManager.instance.SpawnAi(SwarmManager.AiType.Hellhound, minionSpawnPoints[_minionsToSpawn - 1]);
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
}
