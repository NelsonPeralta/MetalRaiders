using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SkeletonBackup : MonoBehaviour
{
    public NavMeshAgent nma;
    public Animator anim;
    public Hitboxes hitboxes;
    public AIMeleeTrigger meleeTrigger;
    public SimpleAILineOfSight simpleLOS;
    public DestroyableObject shield;
    public GameObject motionTrackerDot;
    public AIFieldOfVision fov;

    [Header("Skeleton Settings")]
    public float Health = 250;
    public int points;
    public float defaultSpeed;
    public int damage;
    public bool hasBeenMeleedRecently;

    public Transform target;


    public float defaultAttackCooldown;
    public float meleeAttackCooldown;
    public bool IsInMeleeRange;
    public bool isReadyToAttack;
    public bool isDead;
    public bool isAttacking;
    public bool shieldIsBroken;

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

    [Header("Attack FX")]
    public GameObject fireAttack;
    public GameObject crystalWall;
    public GameObject crystallWallSpawnPoint;

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

    [Header("Other")]
    public GameObject Aura;

    // Start is called before the first frame update
    void Start()
    {
        nma.speed = defaultSpeed;

        StartCoroutine(PlaySound());
    }

    // Update is called once per frame
    void Update()
    {
        HealthCheck();
        Attack();
        AttackCooldown();
        AnimationCheck();
        ShieldBreack();
        TargetSwitchCountdown();
        FOV();

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
                    anim.SetBool("Run", true);

                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                    {
                        motionTrackerDot.SetActive(true);
                    }
                    else
                    {
                        motionTrackerDot.SetActive(false);
                    }
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
                nma.speed = 0;
                anim.SetBool("Run", false);

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

    void Attack()
    {
        //if (IsInMeleeRange && isReadyToAttack && !isDead)
        //{
        //    if (meleeTrigger.pProperties != null)
        //    {
        //        if (!meleeTrigger.pProperties.isDead)
        //        {
        //            Debug.Log("Skeleton Attack");
        //            meleeTrigger.pProperties.BleedthroughDamage(damage, false, 99);
        //            anim.Play("Attack");

        //            int randomSound = Random.Range(0, attackClips.Length - 1);
        //            audioSource.clip = audioClips[randomSound];
        //            audioSource.Play();
        //            //var fireBird = Instantiate(fireAttack, gameObject.transform.position + new Vector3(0, 1f, 0), gameObject.transform.rotation);

        //            isReadyToAttack = false;
        //        }
        //    }
        //}
        //else if(IsInMeleeRange && !isReadyToAttack && !isDead)
        //{
        //    nma.speed = 0;
        //    anim.SetBool("Run", false);
        //}
    }

    void AttackCooldown()
    {
        if (!isReadyToAttack)
        {
            meleeAttackCooldown -= Time.deltaTime;

            if (meleeAttackCooldown <= 0)
            {
                isReadyToAttack = true;
                meleeAttackCooldown = defaultAttackCooldown;
            }
        }
    }

    void Die()
    {
        Destroy(gameObject, 10f);
        Destroy(Aura);
        nma.enabled = false;
        anim.Play("Die");

        //if (swarmMode != null)
        //{
        //    swarmMode.skeletonsAlive = swarmMode.skeletonsAlive - 1;
        //}

        //foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        //{
        //    hitbox.gameObject.layer = 23; //Ground
        //    hitbox.gameObject.SetActive(false);
        //}

        if(lastPlayerWhoShot != null)
        {

        }

        motionTrackerDot.SetActive(false);

        TransferPoints();
        DropRandomWeapon();
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
        int ChanceToDrop = Random.Range(0, 10);

        if (ChanceToDrop <= 3)
        {
            int randomInt = Random.Range(0, droppableWeapons.Length - 1);
            GameObject weapon = Instantiate(droppableWeapons[randomInt], gameObject.transform.position + new Vector3(0, 0.5f, 0), gameObject.transform.rotation);
            weapon.gameObject.name = weapon.name.Replace("(Clone)", "");

            Destroy(weapon, 120);
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

    void FOV()
    {
        if(target == null)
        {
            if(fov.closestPlayer != null)
            {
                target = fov.closestPlayer;
                nma.SetDestination(fov.closestPlayer.position);
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
        yield return new WaitForSeconds(5f);

        int playSound = Random.Range(0, 2);

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

    void LookForNewRandomPlayer()
    {
        //if (swarmMode != null)
        //{
        //    target = swarmMode.NewTargetFromSwarmScript();
        //}
    }
}
