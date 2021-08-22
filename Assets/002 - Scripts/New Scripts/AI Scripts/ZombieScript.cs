using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

public class ZombieScript : MonoBehaviour
{
    public PhotonView PV;
    public OnlineSwarmManager onlineSwarmManager;
    public NavMeshAgent nma;
    public Animator anim;
    public SwarmMode swarmMode;
    public Hitboxes hitboxes;
    public AIMeleeTrigger meleeTrigger;

    [Header("Zombie Settings")]
    public float DefaultHealth;
    public float Health = 50;
    public int points;
    public float defaultSpeed;
    public int zombieNumber;
    public int damage;

    [Header("Player Switching")]
    public Transform lastPlayerWhoShot;
    public bool otherPlayerShot;
    public float targetSwitchCountdownDefault;
    public float targetSwitchCountdown;
    public float targetSwitchResetCountdown;
    public bool targetSwitchReady;
    public bool targetSwitchStarted;
    
    public Transform target;
    public GameObject motionTrackerDot;

    public float defaultAttackCooldown;
    public float meleeAttackCooldown;
    public bool IsInMeleeRange;
    public bool isReadyToAttack;
    public bool isDead;
    public bool isRunning;
    public bool hasBeenMeleedRecently;

    [Header("Drops")]
    public GameObject extraHealth;
    public GameObject smallAmmoPack;
    public GameObject heavyAmmoPack;
    public GameObject powerAmmoPack;
    public GameObject grenadeAmmoPack;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    [Header("Skins")]
    public GameObject placeholderSkin;
    public GameObject[] skins;

    // Start is called before the first frame update
    void OnEnable()
    {
        ResetZombie();        
    }

    // Update is called once per frame
    void Update()
    {
        HealthCheck();
        Movement();
        Attack();
        AttackCooldown();
        AnimationCheck();
        TargetSwitchCountdown();
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
            StartCoroutine(Die());
            isDead = true;
        }

    }

    void Movement()
    {
        if (!isDead)
        {
            if (!IsInMeleeRange)
            {
                nma.speed = defaultSpeed;
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
            }

            if (IsInMeleeRange && !isReadyToAttack)
            {
                nma.speed = 0;
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", true);
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
                motionTrackerDot.SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (IsInMeleeRange && isReadyToAttack && !isDead)
        {
            if (meleeTrigger.pProperties != null)
            {
                if (!meleeTrigger.pProperties.isDead)
                {
                    meleeTrigger.pProperties.BleedthroughDamage(damage, false, 99);
                    target.GetComponent<PlayerController>().ScopeOut();
                    anim.Play("Attack");
                    nma.velocity = Vector3.zero;

                    //int randomSound = Random.Range(0, attackClips.Length - 1);
                    //audioSource.clip = audioClips[randomSound];
                    //audioSource.Play();
                    //var fireBird = Instantiate(fireAttack, gameObject.transform.position + new Vector3(0, 1f, 0), gameObject.transform.rotation);

                    isReadyToAttack = false;
                }
            }
        }
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

    IEnumerator Die()
    {
        nma.enabled = false;
        anim.Play("Die");

        if (swarmMode != null)
        {
            swarmMode.zombiesAlive = swarmMode.zombiesAlive - 1;
        }

        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        {
            hitbox.gameObject.SetActive(false);
        }

        motionTrackerDot.SetActive(false);
        if(lastPlayerWhoShot == null)
        {
            Debug.Log("ZOMBIE HAS NO LAST PLAYER");
        }
        //lastPlayerWhoShot.gameObject.GetComponent<Announcer>().AddToMultiKill();
        if (lastPlayerWhoShot)
        {
            lastPlayerWhoShot.GetComponent<AllPlayerScripts>().announcer.AddToMultiKill();
            TransferPoints();
        }
        DropRandomLoot();
        target = null;

        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }

    void AnimationCheck()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            nma.speed = defaultSpeed;
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }

    void DropRandomLoot()
    {
        int ChanceToDrop = Random.Range(1, 11);
        GameObject loot = new GameObject();
        
        if (ChanceToDrop == 1)
        {
            loot = Instantiate(powerAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
        }

        if (ChanceToDrop == 2)
        {
            loot = Instantiate(heavyAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
        }

        if (ChanceToDrop == 3)
        {
            loot = Instantiate(smallAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
        }
        

        if (ChanceToDrop >= 4 && ChanceToDrop <= 6)
        {
            loot = Instantiate(grenadeAmmoPack, gameObject.transform.position, gameObject.transform.rotation);
        }

        if (ChanceToDrop >= 7 && ChanceToDrop <= 9)
        {
            loot = Instantiate(extraHealth, gameObject.transform.position + new Vector3(0, 1, 0) , gameObject.transform.rotation);
        }

        Destroy(loot, 60);
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(8f);

        int playSound = Random.Range(0, 2);

        if(playSound == 0)
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
            lastPlayerWhoShot = playerWhoShotLast.transform;
            nma.SetDestination(target.position);
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

    void TransferPoints()
    {
        if (lastPlayerWhoShot)
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

    void SimpleTargetChange()
    {
        if (swarmMode != null)
        {
            int activePlayers = swarmMode.ssManager.numberOfPlayers;
            int randomActivePlayer = Random.Range(0, activePlayers);
            target = swarmMode.players[randomActivePlayer].transform;
        }
    }

    public IEnumerator MeleeReset()
    {
        yield return new WaitForEndOfFrame();

        hasBeenMeleedRecently = false;
    }

    void LookForNewRandomPlayer()
    {
        if (swarmMode != null)
        {
            target = swarmMode.NewTargetFromSwarmScript();
        }
    }

    void randomSkin()
    {
        int skinNum = Random.Range(0, skins.Length);

        if (skins[skinNum])
            skins[skinNum].SetActive(true);
    }

    void ResetZombie()
    {
        nma.enabled = true;
        nma.speed = defaultSpeed;
        StartCoroutine(PlaySound());
        if (placeholderSkin)
            placeholderSkin.SetActive(false);
        randomSkin();

        Health = DefaultHealth;
        isDead = false;
        IsInMeleeRange = false;
        isReadyToAttack = true;

        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
        {
            hitbox.gameObject.SetActive(true);
        }

        motionTrackerDot.SetActive(true);

        meleeAttackCooldown = 0;
        lastPlayerWhoShot = null;
        otherPlayerShot = false;
        targetSwitchCountdown = targetSwitchCountdownDefault;
        targetSwitchReady = true;
    }
}
