using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAlienShooter : AiAbstractClass
{
    [Header("Combat")]
    public int projectileDamage;
    public int projectileSpeed;
    public int grenadeDamage;
    public int grenadeRadius;

    [Header("Prefabs")]
    public Fireball projectile;
    public AIGrenade grenade;

    [Header("Shield")]
    public GameObject shieldModel;


    public enum AlienShooterActions { Defend, Shoot, Grenade, Seek, Idle }
    [SerializeField] AlienShooterActions _knightAction;

    public AlienShooterActions knightAction
    {
        get { return _knightAction; }
        set
        {
            if (value == AlienShooterActions.Seek)
                staticAnimationPlaying = false;
            else
                staticAnimationPlaying = true;

            if (_knightAction != value)
            {
                _knightAction = value;
                //Log.Print(() =>$"{name} action change: {_knightAction}");
                InvokeOnActionChanged();
            }
        }
    }

    public override void OnEnable()
    {
        _health += FindObjectOfType<SwarmManager>().currentWave * 8;
        knightAction = AlienShooterActions.Seek;
        seek = true;
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        PlayerRange newPlayerRange = aiAbstractClass.playerRange;
        PlayerRange previousPlayerRange = aiAbstractClass.previousPlayerRange;
        AlienShooterActions previousAction = knightAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            if (newPlayerRange == PlayerRange.Medium && (previousPlayerRange == PlayerRange.Close || previousPlayerRange == PlayerRange.Long))
            {
                if (ran == 0)
                    previousAction = AlienShooterActions.Grenade;
                else
                    previousAction = AlienShooterActions.Shoot;
            }
            else if (newPlayerRange == PlayerRange.Out)
                previousAction = AlienShooterActions.Seek;
        }
        else
        {
            previousAction = AlienShooterActions.Seek;
        }

        if (newPlayerRange == PlayerRange.Close)
            previousAction = AlienShooterActions.Defend;
        else if (newPlayerRange == PlayerRange.Out)
            previousAction = AlienShooterActions.Seek;

        ChangeAction(previousAction.ToString());
    }

    public override void DoAction()
    {
        int ran = Random.Range(0, 4);
        AlienShooterActions previousKnightAction = knightAction;

        if (playerRange == PlayerRange.Medium || playerRange == PlayerRange.Long)
        {
            if (ran == 0)
                previousKnightAction = AlienShooterActions.Grenade;
            else
                previousKnightAction = AlienShooterActions.Shoot;
        }

        if (playerRange == PlayerRange.Out)
            previousKnightAction = AlienShooterActions.Seek;

        if (!isDead && targetPlayer)
        {
            if (previousKnightAction != AlienShooterActions.Defend)
            {
                animator.SetBool("Defend", false);
                shieldModel.SetActive(false);
            }

            if (previousKnightAction != AlienShooterActions.Seek)
            {
                seek = false;
            }


            if (previousKnightAction == AlienShooterActions.Defend)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
                {
                    animator.SetBool("Defend", true);
                    shieldModel.SetActive(true);
                }
            }
            else if (previousKnightAction == AlienShooterActions.Shoot)
            {
                if (canDoAction)
                {
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Projectile");

                    {
                        var proj = Instantiate(projectile.gameObject, projectileSpawnPoint.transform.position
                            , projectileSpawnPoint.transform.rotation);
                        //foreach (AIHitbox c in hitboxes.AIHitboxes)
                        //    Physics.IgnoreCollision(projectile.GetComponent<Collider>(), c.GetComponent<Collider>());
                        proj.GetComponent<Fireball>().damage = projectileDamage;
                        proj.GetComponent<Fireball>().sourceBiped = gameObject;
                        Destroy(proj, 5);
                    }

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousKnightAction == AlienShooterActions.Grenade)
            {
                if (canDoAction)
                {
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Throw");

                    {
                        var potionBomb = Instantiate(grenade.gameObject, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
                        //foreach (AIHitbox c in hitboxes.AIHitboxes)
                        //    Physics.IgnoreCollision(potionBomb.GetComponent<Collider>(), c.GetComponent<Collider>());

                        potionBomb.GetComponent<Rigidbody>().AddForce(projectileSpawnPoint.transform.forward * 300);

                        potionBomb.GetComponent<AIGrenade>().radius = grenadeRadius;
                        potionBomb.GetComponent<AIGrenade>().damage = grenadeDamage;
                        potionBomb.GetComponent<AIGrenade>().sourceBiped = gameObject;


                        //potionBomb.GetComponent<AIGrenade>().playerRewiredID = 99;
                        //potionBomb.GetComponent<AIGrenade>().team = hitboxes.AIHitboxes[0].team;
                    }

                    nextActionCooldown = 3;
                }
            }
            else if (previousKnightAction == AlienShooterActions.Seek)
            {
                seek = true;
            }
        }
        else if (!isDead && !targetPlayer)
        {
            knightAction = AlienShooterActions.Idle;
            seek = false;
        }
    }

    public override void ChildUpdate()
    {
        if (!targetPlayer)
            return;

        if (staticAnimationPlaying)
        {
            Vector3 targetPostition = new Vector3(targetPlayer.position.x,
                                            this.transform.position.y,
                                            targetPlayer.position.z);
            this.transform.LookAt(targetPostition);
        }
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
            pp.GetComponent<PlayerSwarmMatchStats>().kills++;
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);

            //SpawnKillFeed(this.GetType().ToString(), playerWhoShotPDI, damageSource: damageSource, isHeadshot: isHeadshot);
        }
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        if (!targetInLineOfSight)
            knightAction = AlienShooterActions.Seek;
        else
        {
            Log.Print(() =>$"Target in line of sight. Player range: {playerRange}");
            if (playerRange == PlayerRange.Medium)
                knightAction = AlienShooterActions.Shoot;
            else if (playerRange == PlayerRange.Long)
                knightAction = AlienShooterActions.Grenade;
        }
    }


    [PunRPC]
    public override void ChangeAction_RPC(string actionString)
    {
        knightAction = (AlienShooterActions)System.Enum.Parse(typeof(AlienShooterActions), actionString);
    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {

    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        grenadeDamage += SwarmManager.instance.currentWave * 2;
        projectileDamage += SwarmManager.instance.currentWave * 2;
    }
}
