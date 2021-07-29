using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WatcherBackup : MonoBehaviour
{
    public NavMeshAgent nma;
    public Animator anim;
    public SwarmMode swarmMode;
    public Hitboxes hitboxes;
    public GameObject motionTrackerDot;
    public AIFieldOfVision fov;


    [Header("Watcher Settings")]
    public float Health = 150;
    public int points;
    public float defaultSpeed;
    public int damage;
    public GameObject rig;

    [Header("Combat")]
    public Transform target;
    public Transform projectileSpawnPoint;
    public float projectileSpeed;
    public float targetDistance;
    public float minRange;
    public float maxRange;
    public float turnSpeed;
    public float defaultAttackCooldown;
    public float rangedAttackCooldown;
    public float defaultAttackDelay;
    public float attackDelay;
    public bool IsInAttackRange;
    public bool isReadyToAttack;
    public bool attackDelayStarted;
    public bool isDead;
    public bool shieldIsBroken;
    public bool hasBeenMeleedRecently;

    [Header("Animation Bools")]
    public bool isIdle;
    public bool isRunning;
    public bool isGuarding; // For Hitbox

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

    [Header("Combat FX")]
    public GameObject projectile;
    public GameObject bomb;
    public GameObject aoeAttack;
    public DestroyableObject magicShield;
    public GameObject smoke;

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
    public AudioClip[] deathClips;

    private void Start()
    {
        StartCoroutine(PlaySound());
    }

    private void Update()
    {
        HealthCheck();
        RangedAttack();
        AnimationCheck();
        AttackCooldown();
        AttackDelay();
        TargetSwitchCountdown();
        CheckTargetDistance();
        Aim();
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

    void Die()
    {
        Destroy(gameObject, 0.5f);
        nma.enabled = false;
        anim.Play("Take Damage");
        StartCoroutine(SpawnSmoke());

        int randomSound = Random.Range(0, deathClips.Length);
        audioSource.clip = deathClips[randomSound];
        audioSource.Play();

        if (swarmMode != null)
        {
            swarmMode.watchersAlive = swarmMode.watchersAlive - 1;
        }

        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        {
            hitbox.gameObject.layer = 23; //Ground
            hitbox.gameObject.SetActive(false);
        }

        motionTrackerDot.SetActive(false);

        lastPlayerWhoShot.gameObject.GetComponent<Announcer>().AddToMultiKill();
        TransferPoints();
        //DropRandomAmmoPack();
        DropRandomWeapon();
    }

    void RangedAttack()
    {
        if (target != null)
        {
            if (IsInAttackRange && isReadyToAttack && !isDead)
            {
                anim.Play("Throw Projectile");
                attackDelayStarted = true;

                int randomSound = Random.Range(0, attackClips.Length);
                audioSource.clip = attackClips[randomSound];
                audioSource.Play();

                //var projec = Instantiate(projectile, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);

                isReadyToAttack = false;
            }
        }
    }



    /*
    public IEnumerator Guard()
    {
        isGuarding = true; // Cooldown is determined by WaitForSeconds
        anim.SetBool("Guard", true);
        yield return new WaitForSeconds(5f);
        anim.SetBool("Guard", false);
        isGuarding = false;
    }

    public void ShieldBreack()
    {
        if (magicShield.Health <= 0 && !shieldIsBroken)
        {
            Destroy(magicShield.gameObject);
            anim.Play("Shield Break");
            anim.SetBool("Guard", false);
            shieldIsBroken = true;
        }
    }
    */

    ///////////////////////// Actions

    public void CheckTargetDistance()
    {
        if (target != null)
        {
            targetDistance = Vector3.Distance(target.position, transform.position);

            if (targetDistance <= maxRange && targetDistance >= minRange)
            {
                IsInAttackRange = true;
                anim.SetBool("Fly Forward", false);
                anim.SetBool("Fly Backwards", false);
                rig.transform.localScale = new Vector3(1, 1, 1);
            }
            else if (targetDistance > maxRange)
            {
                IsInAttackRange = false;
                anim.SetBool("Fly Forward", true);
                anim.SetBool("Fly Backwards", false);
                rig.transform.localScale = new Vector3(1, 1, 1);
                ChasePlayer();
            }
            else if (targetDistance < minRange)
            {
                IsInAttackRange = false;
                anim.SetBool("Fly Forward", false);
                anim.SetBool("Fly Backwards", true);

                BackAwayFromPlayer();
            }
        }
    }

    void ThrowProjectile()
    {
        var projec = Instantiate(projectile, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
        projec.GetComponent<Rocket>().damage = damage;

        projec.GetComponent<Rocket>().force = projectileSpeed;
        //projec.GetComponent<Rocket>().playerWhoThrewGrenade = gameObject;
    }

    void BackAwayFromPlayer()
    {
        if (!isDead)
        {
            Vector3 dirToPlayer = transform.position - target.transform.position;
            Vector3 newPos = transform.position + dirToPlayer;

            nma.SetDestination(newPos);
            nma.speed = defaultSpeed * 2;
        }
    }

    void ChasePlayer()
    {
        if (target != null)
        {
            nma.SetDestination(target.position);
            nma.speed = defaultSpeed;
        }
    }

    void LookAtPlayer()
    {
        var targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        rig.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    ///////////////////////// Passive

    void AnimationCheck()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Throw Projectile"))
        {
            nma.velocity = Vector3.zero;
            if (target != null)
            {
                var look = new Vector3(target.position.x, transform.position.y, target.position.z);
                transform.LookAt(look);
            }
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Fly Forward"))
        {
            isRunning = true;
            motionTrackerDot.SetActive(true);
        }
        else
        {
            isRunning = false;
            motionTrackerDot.SetActive(false);
        }
    }

    void Aim()
    {
        if (target != null)
        {
            projectileSpawnPoint.LookAt(target.position + new Vector3(0, -0.25f, 0));
        }
    }

    void AttackCooldown()
    {
        if (!isReadyToAttack)
        {
            rangedAttackCooldown -= Time.deltaTime;

            if (rangedAttackCooldown <= 0)
            {
                isReadyToAttack = true;
                rangedAttackCooldown = defaultAttackCooldown;
            }
        }
    }

    void AttackDelay()
    {
        if (!attackDelayStarted)
        {
            if (attackDelay != defaultAttackDelay)
            {
                attackDelay = defaultAttackDelay;
            }
        }

        if (attackDelayStarted)
        {
            attackDelay -= Time.deltaTime;

            if (attackDelay <= 0)
            {
                attackDelay = 0;
                attackDelayStarted = false;
                ThrowProjectile();
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

    void DropRandomAmmoPack()
    {
        int ChanceToDrop = Random.Range(0, 10);

        if (ChanceToDrop <= 3)
        {
            Instantiate(heavyAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
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
        int ChanceToDrop = Random.Range(0, 10);

        if (ChanceToDrop <= 3)
        {
            int randomInt = Random.Range(0, droppableWeapons.Length - 1);
            GameObject weapon = Instantiate(droppableWeapons[randomInt], gameObject.transform.position + new Vector3(0, 0.5f, 0), gameObject.transform.rotation);
            weapon.gameObject.name = weapon.name.Replace("(Clone)", "");

            Destroy(weapon, 120);
        }
    }

    IEnumerator TargetSwitchReset()
    {
        yield return new WaitForSeconds(targetSwitchCountdown);

        targetSwitchReady = true;
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(5f);

        int playSound = Random.Range(0, 3);

        if (playSound == 0)
        {
            int randomSound = Random.Range(0, audioClips.Length);

            if (!isDead)
            {
                audioSource.clip = audioClips[randomSound];
                audioSource.Play();
            }
        }

        StartCoroutine(PlaySound());
    }

    IEnumerator SpawnSmoke()
    {
        yield return new WaitForSeconds(.4f);

        var smoke1 = Instantiate(smoke, transform.position + new Vector3(0, 0.5f, 0), transform.rotation);

        Destroy(smoke1, 5f);
    }

    public IEnumerator MeleeReset()
    {
        yield return new WaitForEndOfFrame();

        hasBeenMeleedRecently = false;
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

    void LookForNewRandomPlayer()
    {
        if (swarmMode != null)
        {
            target = swarmMode.NewTargetFromSwarmScript();
        }
    }
}