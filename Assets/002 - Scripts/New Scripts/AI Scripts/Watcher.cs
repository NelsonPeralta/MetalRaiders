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

    [Header("Line Of Sight")]
    public bool targetInLOS;
    public GameObject LOSSpawn;
    public GameObject objectInLOS;
    public LayerMask layerMask;
    Vector3 raySpawn;
    public RaycastHit hit;
    bool resettingTargetInLOS;


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
    public WatcherActions watcherAction;
    private void Start()
    {
        shieldModel.SetActive(false);
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {

        if (aiAbstractClass.playerRange == PlayerRange.Close)
            watcherAction = WatcherActions.Defend;
        else if (aiAbstractClass.playerRange == PlayerRange.Medium)
            watcherAction = WatcherActions.Fireball;
        else if (aiAbstractClass.playerRange == PlayerRange.Long)
            watcherAction = WatcherActions.Meteor;
        else if (aiAbstractClass.playerRange == PlayerRange.Out)
            watcherAction = WatcherActions.Seek;

        InvokeOnActionChanged();
    }

    public override void DoAction()
    {
        if (isDead)
            return;

        Debug.Log("Do Action");
        if (watcherAction != WatcherActions.Defend)
        {
            animator.SetBool("Defend", false);
            shieldModel.SetActive(false);
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
            seek = false;
            nma.velocity = Vector3.zero;

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
}
