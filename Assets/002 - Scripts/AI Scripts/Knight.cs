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
    KnightActions _knightAction;

    public KnightActions knightAction
    {
        get { return _knightAction; }
        set
        {
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
        knightAction = KnightActions.Seek;
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

        knightAction = previousAction;
    }

    public override void DoAction()
    {
        int ran = Random.Range(0, 3);
        KnightActions previousKnightAction = knightAction;

        if (playerRange == PlayerRange.Medium || playerRange == PlayerRange.Long)
        {
            if (ran == 0)
                previousKnightAction = KnightActions.Grenade;
            else
                previousKnightAction = KnightActions.Fireball;
        }

        if (!isDead && target)
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
                    animator.Play("Projectile");

                    var proj = Instantiate(projectile.gameObject, projectileSpawnPoint.transform.position
                        , projectileSpawnPoint.transform.rotation);
                    foreach (AIHitbox c in hitboxes.AIHitboxes)
                        Physics.IgnoreCollision(projectile.GetComponent<Collider>(), c.GetComponent<Collider>());
                    proj.GetComponent<Fireball>().damage = projectileDamage;
                    proj.GetComponent<Fireball>().force = projectileSpeed;
                    proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                    Destroy(proj, 5);

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousKnightAction == KnightActions.Grenade)
            {
                if (canDoAction)
                {
                    animator.Play("Throw");

                    var potionBomb = Instantiate(grenade.gameObject, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
                    foreach (AIHitbox c in hitboxes.AIHitboxes)
                        Physics.IgnoreCollision(potionBomb.GetComponent<Collider>(), c.GetComponent<Collider>());

                    potionBomb.GetComponent<Rigidbody>().AddForce(projectileSpawnPoint.transform.forward * 300);

                    potionBomb.GetComponent<AIGrenade>().radius = grenadeRadius;
                    potionBomb.GetComponent<AIGrenade>().damage = grenadeDamage;
                    potionBomb.GetComponent<AIGrenade>().playerWhoThrewGrenade = gameObject;
                    potionBomb.GetComponent<AIGrenade>().playerRewiredID = 99;
                    potionBomb.GetComponent<AIGrenade>().team = hitboxes.AIHitboxes[0].team;

                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else if (previousKnightAction == KnightActions.Seek)
            {
                seek = true;
            }
        }
        else if (!isDead && !target)
        {
            knightAction = KnightActions.Idle;
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
        photonView.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI);
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
}