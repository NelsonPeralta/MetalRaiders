using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BlackKnight : MonoBehaviour
{
    public NavMeshAgent nma;
    public Animator anim;
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
    public GameObject SummonFamiliarPoint;
    public GameObject wereratPrefab;
    public GameObject summoningFX;

    [Header("Projectile Attacks")]
    public GameObject axeAttackPrefab;
    public GameObject axeAttackSpawnPoint;
    public float potionBombThrowForce;
    public GameObject potionBombPrefab;
    public GameObject potionBombSpawnPoint;

    [Header("Animation Bools")]
    public bool isIdle;
    public bool isRunning;
    public bool isGuarding; // For Hitbox

    [Header("Extra Life")]
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
    public Hurtzone hurtzone;

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
        UpdateLeftAndRightOfPlayer();
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
                    //anim.SetBool("Run", true);
                    //anim.SetBool("Idle", false);
                }

                if (IsInMidRange)
                {
                    nma.speed = 0;
                    //anim.SetBool("Run", false);
                    //anim.SetBool("Idle", true);
                }
            }
            if (target == null)
            {
                nma.speed = 0;
                //anim.SetBool("Walk", false);
                //anim.SetBool("Idle", true);
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
                    if (nextActionInRange == "Axe Attack")
                    {
                        anim.Play("Axe Attack");
                        if (editMode == false)
                        {
                            var axeAttack = Instantiate(axeAttackPrefab, axeAttackSpawnPoint.transform.position, axeAttackSpawnPoint.transform.rotation);
                        }
                    }
                    if (nextActionInRange == "Projectile Attack")
                    {
                        anim.Play("Projectile Attack");
                        if (editMode == false)
                        {
                            var potionBomb = Instantiate(potionBombPrefab, potionBombSpawnPoint.transform.position, potionBombSpawnPoint.transform.rotation);
                            potionBomb.GetComponent<Rigidbody>().AddForce(potionBombSpawnPoint.transform.forward * potionBombThrowForce);

                            potionBomb.GetComponent<AIGrenade>().playerWhoThrewGrenade = gameObject;
                            potionBomb.GetComponent<AIGrenade>().playerRewiredID = 99;
                            potionBomb.GetComponent<AIGrenade>().team = hitboxes.AIHitboxes[0].team;
                        }
                    }
                    if (nextActionInRange == "Summon")
                    {
                        anim.Play("Summon");
                        if (editMode == false)
                        {
                            GameObject wererat = Instantiate(wereratPrefab, SummonFamiliarPoint.transform.position, SummonFamiliarPoint.transform.rotation);
                            GameObject fx = Instantiate(summoningFX, SummonFamiliarPoint.transform.position, SummonFamiliarPoint.transform.rotation);
                            Destroy(fx, 3);                            
                            wererat.GetComponent<Wererat>().target = target;
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
        if (!isDead)
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
        Destroy(hurtzone);
        Destroy(gameObject, 10f);
        Destroy(Aura);
        nma.enabled = false;
        anim.Play("Die");

        int randomSound = Random.Range(0, deathClips.Length - 1);
        audioSource.clip = deathClips[randomSound];
        audioSource.Play();

        //if (swarmMode != null)
        //{
        //    swarmMode.blackKnightsAlive = swarmMode.blackKnightsAlive - 1;
        //}

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
                shieldIsActive = true;
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
            if (shieldParticleSystem != null)
            {
                shieldParticleSystem.gameObject.SetActive(true);
                shieldIsActive = true;
            }
        }
        else
        {
            isIdle = false;

            if (shieldParticleSystem != null)
            {
                shieldParticleSystem.gameObject.SetActive(false);
                shieldIsActive = false;
                /*
                if (shieldParticleSystem.isPlaying)
                {
                    shieldParticleSystem.Stop();
                    shieldIsActive = false;
                    shieldCollider.enabled = false;
                }*/
            }
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Roll Forward"))
        {
            int randomInt = Random.Range(0, 2);

            if (randomInt == 0)
            {
                nma.SetDestination(leftOfPlayer.transform.position);
            }
            else
            {
                nma.SetDestination(rightOfPlayer.transform.position);
            }

            nma.speed = 25;
            nma.acceleration = 50;
        }
        else
        {
            nma.acceleration = defaultAcceleration;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard"))
        {
            nma.speed = defaultSpeed / 2;
            isGuarding = true;
        }
        else
        {
            isGuarding = false;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Die") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || anim.GetCurrentAnimatorStateInfo(0).IsName("Shield Break"))
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
        Instantiate(extraLife, transform.position + new Vector3(0, 1, 0), transform.rotation);
    }

    public void TargetSwitch(GameObject playerWhoShotLast)
    {
        if (target != null)
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
        else
        {
            target = playerWhoShotLast.gameObject.transform;
            nma.SetDestination(target.position);
            lastPlayerWhoShot = playerWhoShotLast.transform;
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
        yield return new WaitForSeconds(10f);

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
        int randomInt = Random.Range(1, 81);

        if (randomInt >= 1 && randomInt <= 40)
        {
            nextActionInRange = "Axe Attack";
        }
        if (randomInt >= 41 && randomInt <= 60)
        {
            nextActionInRange = "Projectile Attack";
        }
        if (randomInt >= 61 && randomInt <= 80)
        {
            nextActionInRange = "Summon";
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

    public void UpdateLeftAndRightOfPlayer()
    {
        if (target != null)
        {
            Vector3 offsetLeft = new Vector3(-7.5f, 0, 0);
            Vector3 offsetRight = new Vector3(7.5f, 0, 0);

            leftOfPlayer.transform.position = target.transform.position + offsetLeft;
            rightOfPlayer.transform.position = target.transform.position + offsetRight;
        }
    }

    void LookForNewRandomPlayer()
    {
        //if (swarmMode != null)
        //{
        //    target = swarmMode.NewTargetFromSwarmScript();
        //}
    }
}
