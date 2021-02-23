using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlameTyrant : MonoBehaviour
{
    public NavMeshAgent nma;
    public Animator anim;
    public SwarmMode swarmMode;
    public Hitboxes hitboxes;
    public AIMeleeTrigger meleeTrigger;
    public SimpleAILineOfSight simpleLOS;
    public GameObject motionTrackerDot;

    [Header("Skeleton Settings")]
    public bool editMode;
    public bool isDead;
    public float Health = 500;
    public int points;
    public float defaultSpeed;
    public float defaultAcceleration = 8;
    public int damage;

    [Header("Target Settings")]
    public Transform target;
    public Movement targetMovement;
    public float targetDistance;
    public float minRange;
    public float maxRange;

    [Header("Combat")]
    public float defaultAttackCooldown;
    public float nextAttackCooldown;
    public bool hasBeenMeleedRecently;
    public bool IsInMeleeRange;
    public bool IsInMidRange;
    public bool IsOutOfRange;
    public bool isReadyToAttack;
    public bool hasKickedPlayer;
    public bool shieldIsActive;
    public ParticleSystem shieldParticleSystem;
    public SphereCollider shieldCollider;
    public GameObject leftOfPlayer;
    public GameObject rightOfPlayer;


    [Header("Action Manager")]
    public string nextActionInRange;

    [Header("Summon Action")]
    public GameObject hellhoundSummonPoint1;
    public GameObject hellhoundSummonPoint2;
    public GameObject hellhoundSummonPoint3;
    public GameObject hellhoundPrefab;
    public GameObject summoningFX;

    [Header("Projectile Attacks")]
    public GameObject fireballSpawnPoint;
    public GameObject fireballPrefab;
    public GameObject meteorPrefab;
    public GameObject meteorExlposion;

    [Header("Animation Bools")]
    public bool isIdle;
    public bool isRunning;
    public bool isGuarding; // For Hitbox

    [Header("Life Drop")]
    public GameObject extraLife;

    [Header("Ammo Packs")]
    public GameObject smallAmmoPack;
    public GameObject heavyAmmoPack;
    public GameObject powerAmmoPack;
    public GameObject grenadeAmmoPack;

    [Header("Weapons")]
    public GameObject[] droppableWeapons;

    [Header("Weapons")]
    public GameObject[] smallWeapons;
    public GameObject[] heavyWeapons;
    public GameObject[] powerWeapons;

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
    public AudioClip[] audioClips;
    public AudioClip[] attackClips;
    public AudioClip[] hitClips;
    public AudioClip[] deathClips;
    bool isLaughing;
    int firstHitHealth;
    int secondHitHealth;
    bool firstHitAnimationPlayed;
    bool secondHitAnimationPlayed;

    [Header("Other")]
    public GameObject Aura;
    public GameObject explosionCharge;
    public GameObject flamePrefab;
    public bool flameTrailReady;
    

    // Start is called before the first frame update
    void Start()
    {
        firstHitHealth = Mathf.CeilToInt((Health / 3) * 2);
        secondHitHealth = Mathf.CeilToInt(Health / 3);
        nma.speed = defaultSpeed;
        if(target)
            targetMovement = target.gameObject.GetComponent<Movement>();
        StartCoroutine(PlaySound());
        InRangeActionManager();
    }

    // Update is called once per frame
    void Update()
    {
        CheckTargetDistance();
        Movement();
        HealthCheck();
        Attack();
        AttackCooldown();
        //Kick();
        Hit();
        AnimationCheck();
        TargetSwitchCountdown();
    }

    public void CheckTargetDistance()
    {
        if (target != null)
        {
            targetDistance = Vector3.Distance(target.position, transform.position);

            if (targetDistance <= maxRange && targetDistance >= minRange)
            {
                if (IsInMidRange != true)
                {
                    IsInMidRange = true;
                }
                if (IsInMeleeRange == true)
                {
                    IsInMeleeRange = false;
                }
                if (IsOutOfRange == true)
                {
                    IsOutOfRange = false;
                }
            }
            else if (targetDistance > maxRange)
            {
                if (IsInMidRange == true)
                {
                    IsInMidRange = false;
                }
                if (IsInMeleeRange == true)
                {
                    IsInMeleeRange = false;
                }
                if (IsOutOfRange != true)
                {
                    IsOutOfRange = true;
                }
            }
            else if (targetDistance < minRange)
            {
                if (IsInMidRange == true)
                {
                    IsInMidRange = false;
                }
                if (IsInMeleeRange != true)
                {
                    IsInMeleeRange = true;
                }
                if (IsOutOfRange == true)
                {
                    IsOutOfRange = false;
                }
            }
        }
    }

    void HealthCheck()
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
    }

    void Movement()
    {
        if (!isDead)
        {
            if (target != null)
            {
                if (!IsInMeleeRange)
                {
                    nma.speed = defaultSpeed;
                    anim.SetBool("Run", true);
                    anim.SetBool("Idle", false);
                }

                if (IsInMidRange)
                {
                    nma.speed = 0;
                    anim.SetBool("Run", false);
                    anim.SetBool("Idle", true);
                }
            }
            if (target == null)
            {
                nma.speed = 0;
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", true);
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
        if (IsInMidRange && isReadyToAttack && !isDead)
        {
            if (target != null)
            {
                if (!isLaughing)
                {
                    if (nextActionInRange == "Projectile Attack")
                    {
                        anim.Play("Projectile Attack");
                        var fireball = Instantiate(fireballPrefab, fireballSpawnPoint.transform.position, fireballSpawnPoint.transform.rotation);
                        fireball.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                    }
                    if (nextActionInRange == "Summon Meteor")
                    {
                        anim.Play("Summon Meteor");
                        var meteor = Instantiate(meteorPrefab, target.transform.position + new Vector3(0, 15, 0), target.transform.rotation);
                        meteor.GetComponent<Fireball>().playerWhoThrewGrenade = gameObject;
                        meteor.transform.Rotate(90, 0, 0);
                    }
                    if (nextActionInRange == "Summon Familiar")
                    {
                        anim.Play("Summon Familiar");
                        if (editMode == false)
                        {
                            GameObject hellhound1 = Instantiate(hellhoundPrefab, hellhoundSummonPoint1.transform.position, hellhoundSummonPoint1.transform.rotation);
                            GameObject fx1 = Instantiate(summoningFX, hellhoundSummonPoint1.transform.position, hellhoundSummonPoint1.transform.rotation);
                            Destroy(fx1, 3);
                            hellhound1.GetComponent<Hellhound>().target = target;

                            GameObject hellhound2 = Instantiate(hellhoundPrefab, hellhoundSummonPoint2.transform.position, hellhoundSummonPoint2.transform.rotation);
                            GameObject fx2 = Instantiate(summoningFX, hellhoundSummonPoint2.transform.position, hellhoundSummonPoint2.transform.rotation);
                            Destroy(fx2, 3);
                            hellhound2.GetComponent<Hellhound>().target = target;

                            GameObject hellhound3 = Instantiate(hellhoundPrefab, hellhoundSummonPoint3.transform.position, hellhoundSummonPoint3.transform.rotation);
                            GameObject fx3 = Instantiate(summoningFX, hellhoundSummonPoint3.transform.position, hellhoundSummonPoint3.transform.rotation);
                            Destroy(fx3, 3);
                            hellhound3.GetComponent<Hellhound>().target = target;
                        }
                    }
                    if (nextActionInRange == "Roll Forward")
                    {
                        anim.Play("Roll Forward");
                    }

                    int randomSound = Random.Range(0, attackClips.Length - 1);
                    audioSource.clip = attackClips[randomSound];
                    audioSource.Play();
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

    void Kick()
    {
        if (IsInMeleeRange)
        {
            anim.Play("Kick");
            targetMovement.velocity.y = targetMovement.jumpForce;
            //targetMovement.velocity.z = -targetMovement.jumpForce * 0.5f;
            hasKickedPlayer = true;
        }
        if (hasKickedPlayer)
        {
            if (targetMovement.isGrounded)
            {
                targetMovement.velocity.z = 0;
                hasKickedPlayer = false;
            }
        }
    }

    void Hit()
    {
        if (!isDead)
        {
            //Debug.Log("In hit void");
            if (Health <= firstHitHealth && !firstHitAnimationPlayed)
            {
                anim.Play("Hit 1");
                //Debug.Log("First Hit animation p[layerd");

                int randomSound = Random.Range(0, hitClips.Length - 1);
                audioSource.clip = hitClips[randomSound];
                audioSource.Play();
                firstHitAnimationPlayed = true;
            }

            if (Health <= secondHitHealth && !secondHitAnimationPlayed)
            {
                anim.Play("Hit 2");

                int randomSound = Random.Range(0, hitClips.Length - 1);
                audioSource.clip = hitClips[randomSound];
                audioSource.Play();
                secondHitAnimationPlayed = true;
            }
        }
    }

    void Die()
    {
        Destroy(Aura);
        nma.enabled = false;
        anim.Play("Die");

        int randomSound = Random.Range(0, deathClips.Length - 1);
        audioSource.clip = deathClips[randomSound];
        audioSource.Play();

        if (swarmMode != null)
        {
            swarmMode.flameTyrantsAlive = swarmMode.flameTyrantsAlive - 1;
        }

        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        {
            hitbox.gameObject.layer = 23; //Ground
            hitbox.gameObject.SetActive(false);
        }

        if (lastPlayerWhoShot != null)
        {

        }

        motionTrackerDot.SetActive(false);

        if (lastPlayerWhoShot)
        {
            lastPlayerWhoShot.gameObject.GetComponent<Announcer>().AddToMultiKill();
            TransferPoints();
        }
        StartCoroutine(ExplodeOnDeath());
        DropExtraLife();
        //DropRandomWeapon();
        //DropRandomAmmoPack();
    }

    public IEnumerator Guard()
    {
        isGuarding = true; // Cooldown is determined by WaitForSeconds
        anim.SetBool("Guard", true);
        yield return new WaitForSeconds(5f);
        anim.SetBool("Guard", false);
        isGuarding = false;
    }

    void AnimationCheck()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            nma.speed = 0;
            nma.velocity = Vector3.zero;
            isIdle = true;
            gameObject.transform.LookAt(target);

            if (shieldParticleSystem != null)
            {
                shieldParticleSystem.gameObject.SetActive(true);
                /*
                if(!shieldParticleSystem.isPlaying)
                {
                    shieldParticleSystem.Play();
                    shieldIsActive = true;
                    shieldCollider.enabled = true;
                }*/
            }
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            if(flameTrailReady)
            {
                var flame = Instantiate(flamePrefab, transform.position, transform.rotation);
                flame.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                flameTrailReady = false;
                Destroy(flame, 5);
                StartCoroutine(flameTrailReset());
            }

            if (shieldParticleSystem != null)
            {
                shieldParticleSystem.gameObject.SetActive(true);
            }
        }
        else
        {
            isIdle = false;

            if (shieldParticleSystem != null)
            {
                shieldParticleSystem.gameObject.SetActive(false);
                /*
                if (shieldParticleSystem.isPlaying)
                {
                    shieldParticleSystem.Stop();
                    shieldIsActive = false;
                    shieldCollider.enabled = false;
                }*/
            }
        }

        if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            nma.speed = 0;
            nma.velocity = Vector3.zero;
        }
    }

    void DropRandomAmmoPack()
    {
        int ChanceToDrop = Random.Range(0, 10);

        if (ChanceToDrop <= 2)
        {
            Instantiate(powerAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
        }

        if (ChanceToDrop <= 3)
        {
            /*
            int i = Random.Range(0, 4);

            if (i == 0)
            {
                Instantiate(smallAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
            }
            else if (i == 1)
            {
                Instantiate(heavyAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
            }
            else if (i == 2)
            {
                Instantiate(powerAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
            }
            else if (i == 3)
            {
                Instantiate(grenadeAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
            }
            */
        }
    }

    void DropRandomWeapon()
    {
        int ChanceToDrop = Random.Range(0, 7);

        if (ChanceToDrop <= 3)
        {
            int randomInt = Random.Range(0, droppableWeapons.Length - 1);
            GameObject weapon = Instantiate(droppableWeapons[randomInt], gameObject.transform.position + new Vector3(0, 0.5f, 0), gameObject.transform.rotation);
            weapon.GetComponent<LootableWeapon>().RandomAmmo();
            weapon.gameObject.name = weapon.name.Replace("(Clone)", "");

            Destroy(weapon, 120);
        }
    }

    void DropExtraLife()
    {
        var Life = Instantiate(extraLife, transform.position + new Vector3(0, 1, 0), transform.rotation);
        Life.GetComponent<ExtraLife>().numberOfLives = 2;
    }

    void LookForNewRandomPlayer()
    {
        if(swarmMode != null)
        {
            target = swarmMode.NewTargetFromSwarmScript();
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

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(6f);

        int playSound = Random.Range(0, 2);

        if (playSound == 0)
        {
            int randomSound = Random.Range(0, audioClips.Length);

            if (!isDead)
            {
                audioSource.clip = audioClips[randomSound];
                audioSource.Play();
                isLaughing = true;
                StartCoroutine(ResetLaughingSound());
            }
        }

        StartCoroutine(PlaySound());
    }

    IEnumerator ResetLaughingSound()
    {
        yield return new WaitForSeconds(nextAttackCooldown * 2);
        isLaughing = false;
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

    void InRangeActionManager()
    {
        int randomInt = Random.Range(1, 101);

        if (randomInt >= 1 && randomInt <= 40)
        {
            nextActionInRange = "Summon Familiar";
        }
        if (randomInt >= 41 && randomInt <= 70)
        {
            nextActionInRange = "Summon Meteor";
        }
        if (randomInt >= 71 && randomInt <= 100)
        {
            nextActionInRange = "Projectile Attack";
        }
        /*
        if (randomInt >= 81 && randomInt <= 100)
        {
            nextActionInRange = "Roll Forward";
        }*/
    }

    public IEnumerator MeleeReset()
    {
        yield return new WaitForEndOfFrame();

        hasBeenMeleedRecently = false;
    }

    public IEnumerator flameTrailReset()
    {
        yield return new WaitForSeconds(0.1f);

        flameTrailReady = true;
    }

    public IEnumerator ExplodeOnDeath()
    {
        explosionCharge.SetActive(true);

        yield return new WaitForSeconds(2);

        Instantiate(meteorExlposion, transform.position + new Vector3(0, 1, 0), transform.rotation);
        Destroy(gameObject, 0);
    }

    public IEnumerator Block()
    {
        anim.SetBool("Block", true);
        anim.SetBool("Run", false);
        yield return new WaitForEndOfFrame();
        anim.SetBool("Run", true);
        anim.SetBool("Block", false);
    }
}
