using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Skeleton : MonoBehaviour
{
    [Header("Other Scripts")]
    public NavMeshAgent nma;
    public Animator anim;
    public Hitboxes hitboxes;
    public AIMeleeTrigger meleeTrigger;
    public SimpleAILineOfSight simpleLOS;
    public GameObject motionTrackerDot;
    public DestroyableObject shield;

    [Header("Settings")]
    public bool editMode;
    public bool autoSeekTarget;

    [Header("Properties")]
    public bool isDead;
    public float DefaultHealth;
    public float Health;
    public int points;
    public float defaultSpeed;
    public float defaultAcceleration = 8;
    public int damage;

    [Header("Target Settings")]
    public Transform target;
    public Movement targetMovement;
    public GameObject LOSSpawn;
    public GameObject objectInLOS;
    public LayerMask layerMask;
    Vector3 raySpawn;
    public RaycastHit hit;
    public float targetDistance;
    public float minRange;
    public float maxRange;

    [Header("Combat")]
    public bool targetInLOS;
    public string nextActionInRange;
    public float defaultAttackCooldown;
    public float nextAttackCooldown;
    public bool hasBeenMeleedRecently;
    public bool IsInMeleeRange;
    public bool IsInMidRange;
    public bool IsOutOfRange;
    public bool isReadyToAttack = true;
    public bool shieldIsActive;
    public bool shieldIsBroken;
    public bool resettingTargetInLOS;

    [Header("Projectile Attack")]
    public GameObject fireballSpawnPoint;
    public GameObject fireballPrefab;

    [Header("Explosive Attack")]
    public float potionBombThrowForce;
    public GameObject potionBombPrefab;
    public GameObject potionBombSpawnPoint;

    [Header("Animation")]
    public bool isIdle;
    public bool isAttacking;
    public bool isRunning;
    public bool isGuarding; // For Hitbox

    [Header("Loot")]
    public GameObject[] droppableWeapons;

    [Header("Player Switching")]
    public Transform lastPlayerWhoShot;
    public bool otherPlayerShot;
    public float targetSwitchCountdownDefault;
    public float targetSwitchCountdown;
    public float targetSwitchResetCountdown;
    public bool targetSwitchReady;
    public bool targetSwitchStarted;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] genericClips;
    public AudioClip[] attackClips;
    public AudioClip[] hitClips;
    public AudioClip[] deathClips;
    bool isLaughing;
    int firstHitHealth;
    bool firstHitAnimationPlayed;

    [Header("Other")]
    public GameObject Aura;


    // Start is called before the first frame update
    void OnEnable()
    {
        ResetSkeleton();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        HealthCheck();
        Attack();
        AttackCooldown();
        Hit();
        AnimationCheck();
        TargetSwitchCountdown();
        ShootLOSRay();
        ProjectileSpawnLookAtTarget();
    }


    void HealthCheck()
    {
        if (Health > 0)
        {
            if (target != null)
            {
                if (target.gameObject.GetComponent<Player>().hitPoints > 0)
                {
                    nma.SetDestination(target.position);
                }
                else if (target.gameObject.GetComponent<Player>().hitPoints <= 0)
                {
                    target = null;
                }

                //if (swarmMode != null)
                //{
                //    if (swarmMode.editMode)
                //    {
                //        nma.speed = 0.01f;
                //    }
                //}
            }
            else
            {
                LookForNewRandomPlayer();
            }
        }
        if (Health <= 0 && !isDead)
        {
            nma.speed = 0;
            StartCoroutine(Die());
            isDead = true;
        }
    }

    void Movement()
    {
        if (!isDead)
        {
            if (target != null)
            {
                if (targetInLOS)
                {
                    if (IsOutOfRange)
                    {
                        ChasePlayer();
                    }
                    else if (IsInMidRange || IsInMeleeRange)
                    {
                        if (!isReadyToAttack && !isAttacking)
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

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
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
        if (IsInMidRange && isReadyToAttack && !isDead && targetInLOS)
        {
            if (target != null)
            {
                if (!isLaughing)
                {
                    if (nextActionInRange == "Projectile Attack")
                    {
                        anim.Play("Projectile Attack");
                        var fireball = Instantiate(fireballPrefab, fireballSpawnPoint.transform.position, fireballSpawnPoint.transform.rotation);
                        fireball.GetComponent<Fireball>().sourceBiped = gameObject;
                    }
                    if (nextActionInRange == "Throw Explosive")
                    {
                        anim.Play("Throw Explosive");
                        var potionBomb = Instantiate(potionBombPrefab, potionBombSpawnPoint.transform.position, potionBombSpawnPoint.transform.rotation);
                        potionBomb.GetComponent<Rigidbody>().AddForce(potionBombSpawnPoint.transform.forward * potionBombThrowForce);

                        potionBomb.GetComponent<AIStickyGrenade>().playerWhoThrewGrenade = gameObject;
                        potionBomb.GetComponent<AIStickyGrenade>().playerRewiredID = 99;
                        //potionBomb.GetComponent<AIStickyGrenade>().team = hitboxes.AIHitboxes[0].team;
                    }

                    int randomSound = Random.Range(0, attackClips.Length - 1);
                    audioSource.clip = attackClips[randomSound];
                    audioSource.Play();
                }

                InRangeActionManager();

                isReadyToAttack = false;
            }
        }
        else if (IsInMeleeRange && isReadyToAttack && !isDead && targetInLOS)
        {
            if (target != null)
            {
                if (!isLaughing)
                {
                    if (nextActionInRange == "Melee Attack")
                    {
                        anim.Play("Melee Attack");
                        int randomSound = Random.Range(0, attackClips.Length - 1);
                        audioSource.clip = attackClips[randomSound];
                        audioSource.Play();

                        Debug.Log("Skeleton Melee Attack");
                    }
                }

                InRangeActionManager();

                isReadyToAttack = false;
            }
        }
    }

    void AttackCooldown()
    {
        if (!isReadyToAttack)
        {
            nextAttackCooldown -= Time.deltaTime;

            if (nextAttackCooldown <= 0)
            {
                isReadyToAttack = true;
                nextAttackCooldown = defaultAttackCooldown;
            }
        }
    }

    void Hit()
    {
        if (!isDead)
        {
            if (Health <= firstHitHealth && !firstHitAnimationPlayed)
            {
                anim.Play("Hit 1");

                int randomSound = Random.Range(0, hitClips.Length - 1);
                audioSource.clip = hitClips[randomSound];
                audioSource.Play();
                firstHitAnimationPlayed = true;
            }
        }
    }

    IEnumerator Die()
    {
        Debug.Log("Die");
        Aura.SetActive(false);
        nma.enabled = false;
        anim.Play("Die");

        int randomSound = Random.Range(0, deathClips.Length - 1);
        audioSource.clip = deathClips[randomSound];
        audioSource.Play();

        //if (swarmMode != null)
        //    swarmMode.skeletonsAlive = swarmMode.skeletonsAlive - 1;

        //foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        //{
        //    //hitbox.gameObject.layer = 23; //Ground
        //    hitbox.gameObject.SetActive(false);
        //}

        motionTrackerDot.SetActive(false);
        if (lastPlayerWhoShot)
        {
            TransferPoints();
            lastPlayerWhoShot.GetComponent<PlayerSwarmMatchStats>().kills++;
        }
        DropRandomWeapon();
        target = null;

        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }

    public IEnumerator Guard()
    {
        isGuarding = true; // Cooldown is determined by WaitForSeconds
        anim.SetBool("Guard", true);
        yield return new WaitForSeconds(2f);
        anim.SetBool("Guard", false);
        isGuarding = false;
    }

    public void ShieldBreack()
    {
        //if (shield.Health <= 0 && !shieldIsBroken)
        //{
        //    Destroy(shield.gameObject);
        //    anim.Play("Shield Break");
        //    anim.SetBool("Guard", false);
        //    shieldIsBroken = true;
        //}
    }

    void AnimationCheck()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            nma.speed = 0;
            nma.velocity = Vector3.zero;
            isIdle = true;
            if (target)
            {
                Vector3 targetPostition = new Vector3(target.position.x,
                                            this.transform.position.y,
                                            target.position.z);
                this.transform.LookAt(targetPostition);
            }
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            nma.speed = defaultSpeed;
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard"))
        {
            nma.speed = defaultSpeed / 2;
        }
        else
        {

        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Die") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || anim.GetCurrentAnimatorStateInfo(0).IsName("Shield Break"))
        {
            nma.speed = 0;
            nma.velocity = Vector3.zero;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Melee Attack"))
            isAttacking = true;
        else
            isAttacking = false;
    }

    void DropRandomWeapon()
    {
        int ChanceToDrop = Random.Range(0, 10);

        if (ChanceToDrop <= 3)
        {
            int randomInt = Random.Range(0, droppableWeapons.Length - 1);
            GameObject weapon = Instantiate(droppableWeapons[randomInt], gameObject.transform.position + new Vector3(0, 0.5f, 0), gameObject.transform.rotation);
            weapon.GetComponent<LootableWeapon>().RandomAmmo();
            weapon.gameObject.name = weapon.name.Replace("(Clone)", "");

            Destroy(weapon, 60);
        }
    }

    void LookForNewRandomPlayer()
    {
        //if (swarmMode != null)
        //{
        //    target = swarmMode.NewTargetFromSwarmScript();
        //}
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

    public IEnumerator MeleeReset()
    {
        yield return new WaitForEndOfFrame();

        hasBeenMeleedRecently = false;
    }

    void TransferPoints()
    {
        if (lastPlayerWhoShot)
        {
            if (lastPlayerWhoShot.gameObject.GetComponent<PlayerSwarmMatchStats>() != null)
            {
                PlayerSwarmMatchStats pPoints = lastPlayerWhoShot.gameObject.GetComponent<PlayerSwarmMatchStats>();

                pPoints.AddPoints(this.points);
            }
        }
    }

    public void TransferDamageToPoints(int points)
    {
        if (lastPlayerWhoShot.gameObject != null)
        {
            if (lastPlayerWhoShot.gameObject.GetComponent<PlayerSwarmMatchStats>() != null)
            {
                PlayerSwarmMatchStats pPoints = lastPlayerWhoShot.gameObject.GetComponent<PlayerSwarmMatchStats>();

                pPoints.AddPoints(points);
            }
        }
    }

    void InRangeActionManager()
    {
        if (target != null)
        {
            if (IsInMeleeRange)
            {
                nextActionInRange = "Melee Attack";
            }
            else if (IsInMidRange || IsOutOfRange)
            {
                int randomInt = Random.Range(1, 101);

                if (randomInt >= 1 && randomInt <= 30)
                {
                    nextActionInRange = "Throw Explosive";
                }
                if (randomInt >= 31 && randomInt <= 100)
                {
                    nextActionInRange = "Projectile Attack";
                }
            }
        }
    }

    void ShootLOSRay()
    {
        raySpawn = LOSSpawn.transform.position + new Vector3(0, 0f, 0);
        Debug.DrawRay(raySpawn, LOSSpawn.transform.forward * maxRange, Color.green);

        if (Physics.Raycast(raySpawn, LOSSpawn.transform.forward * maxRange, out hit, maxRange, layerMask)) // Need a Raycast Range Overload to work with LayerMask
        {
            objectInLOS = hit.transform.gameObject;

            if (objectInLOS.GetComponent<PlayerHitbox>())
            {
                GameObject playerInLOS = objectInLOS.GetComponent<PlayerHitbox>().player.gameObject;
                if (target)
                {
                    if (playerInLOS == target.gameObject)
                        targetInLOS = true;
                    else
                    {
                        if (!resettingTargetInLOS)
                            StartCoroutine(ResetTargetInLOS());
                    }
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

    void ChasePlayer()
    {
        nma.speed = defaultSpeed;
        anim.SetBool("Run", true);
        //anim.SetBool("Idle", false);
    }

    void Idle()
    {
        nma.speed = 0;
        anim.SetBool("Run", false);
        //anim.SetBool("Idle", true);
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(8f);

        int playSound = Random.Range(0, 2);

        if (playSound == 0)
        {
            int randomSound = Random.Range(0, genericClips.Length - 1);

            if (!isDead)
            {
                audioSource.clip = attackClips[randomSound];
                audioSource.Play();
            }
        }

        StartCoroutine(PlaySound());
    }

    IEnumerator ResetTargetInLOS()
    {
        resettingTargetInLOS = true;
        yield return new WaitForSeconds(5f);

        targetInLOS = false;
        resettingTargetInLOS = false;
    }

    void ProjectileSpawnLookAtTarget()
    {
        if (target)
        {
            fireballSpawnPoint.transform.LookAt(target);
        }
    }

    void ResetSkeleton()
    {
        firstHitHealth = Mathf.CeilToInt((DefaultHealth / 2));
        nma.enabled = true;
        nma.speed = defaultSpeed;
        if (target)
            targetMovement = target.gameObject.GetComponent<Movement>();
        StartCoroutine(PlaySound());
        InRangeActionManager();

        Health = DefaultHealth;
        isDead = false;
        IsInMeleeRange = false;
        IsInMidRange = false;
        targetInLOS = false;
        isReadyToAttack = true;

        //foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        //{
        //    hitbox.gameObject.SetActive(true);
        //}

        motionTrackerDot.SetActive(true);

        nextAttackCooldown = 0;
        lastPlayerWhoShot = null;
        otherPlayerShot = false;
        targetSwitchCountdown = targetSwitchCountdownDefault;
        targetSwitchReady = true;
        Aura.SetActive(true);
    }
}
