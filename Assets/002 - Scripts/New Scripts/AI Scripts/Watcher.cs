using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Watcher : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent nma;
    public AudioSource aSource;
    public SwarmMode swarmMode;
    public PlayerManager pManager;
    public Hitboxes hitboxes;

    [Header("Properties")]
    public float Health;
    public int points;
    public bool isDead;
    public int defaultSpeed;

    [Header("Combat")]
    public int projectileDamage;
    public int projectileSpeed;
    public int meteorDamage;
    public int meteorSpeed;
    public GameObject projectileSpawnPoint;
    public GameObject motionTrackerDot;

    [Header("Target Management")]
    public Transform target;
    public float maxMeleeDistance;
    public float maxRangeDistance;
    public bool isInMeleeRange;
    public bool isInRange;

    [Header("Action Management")]
    public bool isReadyToAttack;
    public string nextAction;
    public float nextActionCooldown;

    [Header("Player Switching")]
    public Transform lastPlayerWhoShot;
    public bool otherPlayerShot;
    public float targetSwitchCountdownDefault;
    public float targetSwitchCountdown;
    public float targetSwitchResetCountdown;
    public bool targetSwitchReady;
    public bool targetSwitchStarted;
    public bool hasBeenMeleedRecently;

    [Header("Line Of Sight")]
    public bool targetInLOS;
    public GameObject LOSSpawn;
    public GameObject objectInLOS;
    public LayerMask layerMask;
    Vector3 raySpawn;
    public RaycastHit hit;
    bool resettingTargetInLOS;

    [Header("Loot")]
    public GameObject[] droppableWeapons;

    [Header("Prefabs")]
    public GameObject projectile;
    public GameObject meteor;
    public GameObject wall;
    public GameObject deathSmoke;

    [Header("Shield")]
    public ParticleSystem shield;
    public SphereCollider shieldCollider;

    private void Start()
    {
        ActionManager();
    }

    private void Update()
    {
        Attack();
        Block();
        Movement();
        TargetSwitchCountdown();
        ShootLOSRay();
        ProjectileSpawnLookAtTarget();
    }

    void ActionManager()
    {
        if (target != null)
        {
            if (!isInMeleeRange && isInRange)
            {
                animator.SetBool("Defend", false);
                int randomInt = Random.Range(1, 101);

                if (randomInt >= 1 && randomInt <= 40)
                {
                    nextAction = "Projectile";
                }
                if (randomInt >= 41 && randomInt <= 70)
                {
                    var pSurro = target.GetComponent<PlayerProperties>().pSurroundings;
                    if (!pSurro.objectOverPlayerHead)
                        nextAction = "Meteor";
                    else
                        nextAction = "Projectile";
                }
                if (randomInt >= 71 && randomInt <= 100)
                {
                    var mov = target.GetComponent<Movement>();
                    if (mov.direction == "Backwards" || mov.direction == "Left" || mov.direction == "Right")
                        nextAction = "Wall";
                    else
                        nextAction = "Projectile";
                }
            }
        }
    }

    void Movement()
    {
            if (Health > 0)
            {
                if (target != null)
                {
                    if (target.gameObject.GetComponent<PlayerProperties>().Health > 0)
                    {
                        nma.SetDestination(target.position);
                    }
                    else if (target.gameObject.GetComponent<PlayerProperties>().Health <= 0)
                    {
                        target = null;
                    }

                    if (swarmMode != null)
                    {
                        if (swarmMode.editMode)
                        {
                            nma.speed = 0.01f;
                        }
                    }
                }
                else
                {
                    LookForNewRandomPlayer();
                }
            }
            if (Health <= 0 && !isDead)
            {
                nma.speed = 0;
                Die();
                isDead = true;
            }
        

        if (!isDead)
        {
            if (target != null)
            {
                if (targetInLOS)
                {
                    if (!isInMeleeRange && !isInRange)
                    {
                        ChasePlayer();
                    }
                    else if (isInMeleeRange || isInRange || !isReadyToAttack)
                    {
                        Idle();
                    }
                }
                else
                {
                    ChasePlayer();
                }
            }
            if (target == null)
            {
                Idle();
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("FlyForward"))
            {
                motionTrackerDot.SetActive(true);
            }
            else
            {
                if (motionTrackerDot.activeSelf)
                {
                    motionTrackerDot.SetActive(false);
                }
            }
        }
    }

    void Attack()
    {
        if (isReadyToAttack && !isDead && target && targetInLOS)
        {
            if (!isInMeleeRange && isInRange)
            {
                if (nextAction == "Projectile")
                {
                    animator.Play("Projectile");
                    var proj = Instantiate(projectile, projectileSpawnPoint.transform.position
                        , projectileSpawnPoint.transform.rotation);
                    //Debug.Log("Watcher Projectile Destination: " + projectileSpawnPoint.transform.position + " " + projectileSpawnPoint.transform.rotation.eulerAngles);
                    proj.GetComponent<Fireball>().damage = projectileDamage;
                    proj.GetComponent<Fireball>().force = projectileSpeed;
                    proj.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                    Destroy(proj, 5);
                    nextAction = "";
                    isReadyToAttack = false;
                    StartCoroutine(ActionCooldown(nextActionCooldown));
                    ActionManager();
                }
                else if (nextAction == "Meteor")
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
                    nextAction = "";
                    isReadyToAttack = false;
                    StartCoroutine(ActionCooldown(nextActionCooldown));
                    ActionManager();
                }
                else if (nextAction == "Wall")
                {
                    var pSurro = target.GetComponent<PlayerProperties>().pSurroundings;
                    var mov = target.GetComponent<Movement>();
                    if (mov.direction == "Backwards")
                    {
                        animator.Play("Summon");
                        var wal = Instantiate(wall, pSurro.back.transform.position, pSurro.back.transform.rotation);
                        wal.transform.Rotate(-90, 0, 0);
                    }
                    else if (mov.direction == "Left")
                    {
                        animator.Play("Summon");
                        var wal = Instantiate(wall, pSurro.left.transform.position, pSurro.left.transform.rotation);
                        wal.transform.Rotate(-90, 90, 0);
                    }
                    else if (mov.direction == "Right")
                    {
                        animator.Play("Summon");
                        var wal = Instantiate(wall, pSurro.right.transform.position, pSurro.right.transform.rotation);
                        wal.transform.Rotate(-90, 90, 0);
                    }
                    else
                    {
                        animator.Play("Projectile");
                        var proj = Instantiate(projectile, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
                        proj.GetComponent<Fireball>().damage = projectileDamage;
                        proj.GetComponent<Fireball>().force = projectileSpeed;
                        Destroy(proj, 5);
                    }
                    nextAction = "";
                    isReadyToAttack = false;
                    StartCoroutine(ActionCooldown(nextActionCooldown/2));
                    ActionManager();
                }
            }
        }
    }

    void Block()
    {
        if (isInMeleeRange && target)
        {
            animator.SetBool("Defend", true);
            if (nextAction != "")
                nextAction = "";
        }
        else
        {
            animator.SetBool("Defend", false);

            if (nextAction == "")
                ActionManager();
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
        {
            if (shield)
            {
                if (!shield.isPlaying)
                    shield.gameObject.SetActive(true);
            }
        }
        else
        {
            if (shield)
            {
                if (shield.isPlaying)
                    shield.gameObject.SetActive(false);
            }
        }
    }

    void ChasePlayer()
    {
        nma.speed = defaultSpeed;
        animator.SetBool("Fly Forward", true);
        animator.SetBool("Idle", false);
    }

    void Idle()
    {
        nma.speed = 0;
        animator.SetBool("Fly Forward", false);
        animator.SetBool("Idle", true);
        if (target)
        {
            Vector3 targetPostition = new Vector3(target.position.x,
                                        this.transform.position.y,
                                        target.position.z);
            this.transform.LookAt(targetPostition);
        }
    }

    void ProjectileSpawnLookAtTarget()
    {
        if (target)
        {
            projectileSpawnPoint.transform.LookAt(target);
            //Debug.Log("Watcher Porjectile Look At: " + target.transform.position + " " + target.transform.rotation.eulerAngles);
        }
    }

    void Die()
    {
        var ds = Instantiate(deathSmoke, transform.position + new Vector3(0, 1, 0), transform.rotation);
        Destroy(ds, 5);
        Destroy(gameObject, 0.5f);
        nma.enabled = false;
        animator.Play("Take Damage");
        isDead = true;

        //int randomSound = Random.Range(0, deathClips.Length - 1);
        //audioSource.clip = deathClips[randomSound];
        //audioSource.Play();

        if (swarmMode != null)
            swarmMode.watchersAlive = swarmMode.watchersAlive - 1;

        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        {
            hitbox.gameObject.layer = 23; //Ground
            hitbox.gameObject.SetActive(false);
        }

        motionTrackerDot.SetActive(false);

        if (lastPlayerWhoShot)
        {
            lastPlayerWhoShot.gameObject.GetComponent<Announcer>().AddToMultiKill();
            TransferPoints();
        }
        DropRandomWeapon();
    }

    void LookForNewRandomPlayer()
    {
        if (swarmMode != null)
        {
            target = swarmMode.NewTargetFromSwarmScript();
        }
    }
    void TargetSwitchCountdown()
    {
        if (otherPlayerShot)
        {
            targetSwitchCountdown -= Time.deltaTime;

            if (targetSwitchCountdown <= 0)
            {
                if (targetSwitchReady)
                {
                    targetSwitchReady = false;
                    target = lastPlayerWhoShot.transform;
                    nma.SetDestination(target.position);
                    targetSwitchCountdown = targetSwitchCountdownDefault;
                    StartCoroutine(TargetSwitchReset());
                }
                otherPlayerShot = false;
                targetSwitchStarted = false;
            }
        }
    }

    IEnumerator TargetSwitchReset()
    {
        yield return new WaitForSeconds(targetSwitchCountdown);

        targetSwitchReady = true;
    }

    public void TransferDamageToPoints(int points)
    {
        //StartCoroutine(Block());

        if (lastPlayerWhoShot.gameObject != null)
        {
            if (lastPlayerWhoShot.gameObject.GetComponent<PlayerPoints>() != null)
            {
                PlayerPoints pPoints = lastPlayerWhoShot.gameObject.GetComponent<PlayerPoints>();

                pPoints.swarmPoints = pPoints.swarmPoints + points;
                pPoints.swarmPointsText.text = pPoints.swarmPoints.ToString();
            }
        }
    }

    public void TargetSwitch(GameObject playerWhoShotLast)
    {
        if (target != null)
        {
            if (playerWhoShotLast != null)
            {
                if (playerWhoShotLast.name != target.gameObject.name)
                {
                    if (lastPlayerWhoShot != playerWhoShotLast)
                    {
                        targetSwitchCountdown = targetSwitchCountdownDefault;
                        otherPlayerShot = true;
                        lastPlayerWhoShot = playerWhoShotLast.transform;
                    }
                }
                else if (playerWhoShotLast.name == target.gameObject.name)
                {
                    targetSwitchCountdown = targetSwitchCountdownDefault;
                    otherPlayerShot = false;
                    lastPlayerWhoShot = playerWhoShotLast.transform;
                }
            }
        }
        else
        {
            if (playerWhoShotLast != null)
            {
                target = playerWhoShotLast.gameObject.transform;
                nma.SetDestination(target.position);
                lastPlayerWhoShot = playerWhoShotLast.transform;
            }
        }
    }

    void ShootLOSRay()
    {
        raySpawn = LOSSpawn.transform.position + new Vector3(0, 0f, 0);
        Debug.DrawRay(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, Color.green);

        if (Physics.Raycast(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, out hit, maxRangeDistance, layerMask)) // Need a Raycast Range Overload to work with LayerMask
        {
            objectInLOS = hit.transform.gameObject;

            if (objectInLOS.GetComponent<PlayerHitbox>())
            {
                GameObject playerInLOS = objectInLOS.GetComponent<PlayerHitbox>().player.gameObject;

                if (playerInLOS == target.gameObject)
                {
                    targetInLOS = true;
                }
                else
                {
                    if (!resettingTargetInLOS)
                        StartCoroutine(ResetTargetInLOS());
                }
            }
        }
        else
        {
            objectInLOS = null;
            if (!resettingTargetInLOS)
                StartCoroutine(ResetTargetInLOS());
        }
    }

    void TransferPoints()
    {
        if (lastPlayerWhoShot.gameObject != null)
        {
            if (lastPlayerWhoShot.gameObject.GetComponent<PlayerPoints>() != null)
            {
                PlayerPoints pPoints = lastPlayerWhoShot.gameObject.GetComponent<PlayerPoints>();

                pPoints.swarmPoints = pPoints.swarmPoints + points;
                pPoints.swarmPointsText.text = pPoints.swarmPoints.ToString();
            }
        }
    }

    void DropRandomWeapon()
    {
        int ChanceToDrop = Random.Range(0, 10);

        if (ChanceToDrop <= 3)
        {
            int randomInt = Random.Range(0, droppableWeapons.Length - 1);
            GameObject weapon = Instantiate(droppableWeapons[randomInt], gameObject.transform.position + new Vector3(0, 0.5f, 0), gameObject.transform.rotation);
            weapon.gameObject.name = weapon.name.Replace("(Clone)", "");

            Destroy(weapon, 60);
        }
    }

    IEnumerator ActionCooldown(float cooldown)
    {
        if (!isReadyToAttack)
        {
            yield return new WaitForSeconds(cooldown);
            isReadyToAttack = true;
        }
    }

    public IEnumerator MeleeReset()
    {
        yield return new WaitForEndOfFrame();

        hasBeenMeleedRecently = false;
    }

    IEnumerator ResetTargetInLOS()
    {
        resettingTargetInLOS = true;
        yield return new WaitForSeconds(5f);

        targetInLOS = false;
        resettingTargetInLOS = false;
    }
}
