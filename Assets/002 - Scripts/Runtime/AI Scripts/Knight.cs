using Photon.Pun;
using UnityEngine;

public class Knight : AiAbstractClass
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


    public enum KnightActions { Defend, Fireball, Grenade, Seek, Idle }
    [SerializeField] KnightActions _knightAction;

    public KnightActions knightAction
    {
        get { return _knightAction; }
        set
        {
            if (value == KnightActions.Seek)
                staticAnimationPlaying = false;
            else
                staticAnimationPlaying = true;

            if (_knightAction != value)
            {
                _knightAction = value;
                //Debug.Log($"{name} action change: {_knightAction}");
                InvokeOnActionChanged();
            }
        }
    }

    public override void OnEnable()
    {
        _health += FindObjectOfType<SwarmManager>().currentWave * 8;
        knightAction = KnightActions.Seek;
        seek = true;
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        PlayerRange newPlayerRange = aiAbstractClass.playerRange;
        PlayerRange previousPlayerRange = aiAbstractClass.previousPlayerRange;
        KnightActions previousAction = knightAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            if (newPlayerRange == PlayerRange.Medium && (previousPlayerRange == PlayerRange.Close || previousPlayerRange == PlayerRange.Long))
            {
                if (ran == 0)
                    previousAction = KnightActions.Grenade;
                else
                    previousAction = KnightActions.Fireball;
            }
            else if (newPlayerRange == PlayerRange.Out)
                previousAction = KnightActions.Seek;
        }
        else
        {
            previousAction = KnightActions.Seek;
        }

        if (newPlayerRange == PlayerRange.Close)
            previousAction = KnightActions.Defend;
        else if (newPlayerRange == PlayerRange.Out)
            previousAction = KnightActions.Seek;

        ChangeAction(previousAction.ToString());
    }

    public override void DoAction()
    {
        int ran = Random.Range(0, 4);
        KnightActions previousKnightAction = knightAction;

        if (playerRange == PlayerRange.Medium || playerRange == PlayerRange.Long)
        {
            if (ran == 0)
                previousKnightAction = KnightActions.Grenade;
            else
                previousKnightAction = KnightActions.Fireball;
        }

        if (playerRange == PlayerRange.Out)
            previousKnightAction = KnightActions.Seek;

        if (!isDead && targetPlayer)
        {
            if (previousKnightAction != KnightActions.Defend)
            {
                animator.SetBool("Defend", false);
                shieldModel.SetActive(false);
            }

            if (previousKnightAction != KnightActions.Seek)
            {
                seek = false;
            }


            if (previousKnightAction == KnightActions.Defend)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
                {
                    animator.SetBool("Defend", true);
                    shieldModel.SetActive(true);
                }
            }
            else if (previousKnightAction == KnightActions.Fireball)
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
            else if (previousKnightAction == KnightActions.Grenade)
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
                        potionBomb.GetComponent<AIGrenade>().playerWhoThrewGrenade = gameObject;


                        //potionBomb.GetComponent<AIGrenade>().playerRewiredID = 99;
                        //potionBomb.GetComponent<AIGrenade>().team = hitboxes.AIHitboxes[0].team;
                    }

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousKnightAction == KnightActions.Seek)
            {
                seek = true;
            }
        }
        else if (!isDead && !targetPlayer)
        {
            knightAction = KnightActions.Idle;
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

        Player pp = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
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
            knightAction = KnightActions.Seek;
        else
        {
            Debug.Log($"Target in line of sight. Player range: {playerRange}");
            if (playerRange == PlayerRange.Medium)
                knightAction = KnightActions.Fireball;
            else if (playerRange == PlayerRange.Long)
                knightAction = KnightActions.Grenade;
        }
    }


    [PunRPC]
    public override void ChangeAction_RPC(string actionString)
    {
        knightAction = (KnightActions)System.Enum.Parse(typeof(KnightActions), actionString);
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
