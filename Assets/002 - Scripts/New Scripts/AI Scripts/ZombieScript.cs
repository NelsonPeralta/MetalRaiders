using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

public class ZombieScript : AiAbstractClass
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
    public int defaultDamage;
    int damage;
    string movementAnimationName;

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
    public bool _isDead;
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

    public override bool IsDead()
    {
        return _isDead;
    }

    private void Awake()
    {
        gameObject.transform.parent = AIPool.aIPoolInstance.transform;
        gameObject.SetActive(false);
    }
    private void Start()
    {
        onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;
    }

    // Update is called once per frame
    void Update()
    {

        Movement();

        if (!PV.IsMine)
            return;
        Attack();
        AttackCooldown();
        //TargetSwitchCountdown();
    }

    public override void Damage(int damage, int playerWhoShotPDI)
    {
        if (IsDead())
            return;
        PV.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI);
    }

    [PunRPC]
    void Damage_RPC(int damage, int playerWhoShotPDI)
    {
        if (IsDead())
            return;
        Health -= damage;
        PlayerProperties pp = PhotonView.Find(playerWhoShotPDI).GetComponent<PlayerProperties>();
        pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(damage);

        if (Health <= 0)
        {
            PhotonView.Find(playerWhoShotPDI).GetComponent<OnlinePlayerSwarmScript>().kills++;
            Die();
        }
    }
    void Die()
    {
        StartCoroutine(Die_Coroutine());
    }
    IEnumerator Die_Coroutine()
    {
        try
        {
            _isDead = true;
            onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;
            onlineSwarmManager.RemoveOneZombie();
            gameObject.name = $"{gameObject.name} (DEAD)";
            nma.speed = 0;
            nma.enabled = false;
            anim.Play("Die");


            foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
                hitbox.gameObject.SetActive(false);

            motionTrackerDot.SetActive(false);
            if (lastPlayerWhoShot == null)
                Debug.Log("ZOMBIE HAS NO LAST PLAYER");

            //lastPlayerWhoShot.gameObject.GetComponent<Announcer>().AddToMultiKill();
            //if (lastPlayerWhoShot)
            //    lastPlayerWhoShot.GetComponent<AllPlayerScripts>().announcer.AddToMultiKill();
            TransferPoints();

            if (PhotonNetwork.IsMasterClient)
                DropRandomLoot();
            target = null;

        }
        catch (System.Exception e)
        {
            Debug.Log($"ERROR: {e}");

            gameObject.name = $"{gameObject.name} (DEAD)";
            _isDead = true;
            nma.speed = 0;
            nma.enabled = false;
            anim.Play("Die");


            foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
                hitbox.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }

    public void EnableThisAi(int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        PV.RPC("EnableThisAi_RPC", RpcTarget.All, targetPhotonId, spawnPointPosition, spawnPointRotation);
    }

    [PunRPC]
    public void EnableThisAi_RPC(int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        gameObject.transform.position = spawnPointPosition;
        gameObject.transform.rotation = spawnPointRotation;
        gameObject.SetActive(true);
        ResetZombie();
        target = PhotonView.Find(targetPhotonId).transform;
    }
    void Movement()
    {
        AnimationCheck();
        if (Health > 0)
        {
            if (onlineSwarmManager && onlineSwarmManager.editMode)
                nma.velocity = Vector3.zero;
            if (target)
            {
                if (target.gameObject.GetComponent<PlayerProperties>().Health > 0)
                    try
                    {
                        nma.SetDestination(target.position); // Error: "SetDestination" can only be called on an active agent that has been placed on a NavMesh.

                    }
                    catch
                    {
                        Debug.Log($"{gameObject.name} is active ({gameObject.activeSelf} on position {transform.position})");
                    }
                if (target.gameObject.GetComponent<PlayerProperties>().Health <= 0 || target.gameObject.GetComponent<PlayerProperties>().isDead || target.gameObject.GetComponent<PlayerProperties>().isRespawning)
                {
                    Debug.Log("Zombie target null");
                    target = null;
                }

            }
            else
                LookForNewRandomPlayer();
        }

        if (!_isDead)
        {
            if (!IsInMeleeRange)
            {
                nma.speed = defaultSpeed;
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", false);
                anim.SetBool("Run", false);
                anim.SetBool(movementAnimationName, true);
            }

            if (IsInMeleeRange && !isReadyToAttack)
            {
                nma.speed = 0;
                anim.SetBool("Walk", false);
                anim.SetBool("Run", false);
                anim.SetBool("Idle", true);
            }

            if (target == null)
            {
                nma.speed = 0;
                anim.SetBool("Walk", false);
                anim.SetBool("Run", false);
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
        if (IsInMeleeRange && isReadyToAttack && !_isDead)
            if (meleeTrigger.player && meleeTrigger.player.CanBeDamaged())
                PV.RPC("Attack_RPC", RpcTarget.All, meleeTrigger.player.PV.ViewID);
    }

    [PunRPC]
    void Attack_RPC(int playerPID)
    {
        PlayerProperties pp = PhotonView.Find(playerPID).GetComponent<PlayerProperties>();
        pp.Damage(damage, false, 99);
        anim.Play("Attack");
        nma.velocity = Vector3.zero;

        //int randomSound = Random.Range(0, attackClips.Length - 1);
        //audioSource.clip = audioClips[randomSound];
        //audioSource.Play();
        //var fireBird = Instantiate(fireAttack, gameObject.transform.position + new Vector3(0, 1f, 0), gameObject.transform.rotation);

        isReadyToAttack = false;
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
        int ChanceToDrop = Random.Range(1, 26);
        string ammoType = "";

        if (ChanceToDrop == 1)
            ammoType = "power";

        if (ChanceToDrop == 2)
            ammoType = "heavy";

        if (ChanceToDrop == 3)
            ammoType = "small";


        if (ChanceToDrop >= 4 && ChanceToDrop <= 6)
            ammoType = "grenade";

        PV.RPC("DropRandomLoot_RPC", RpcTarget.All, ammoType, transform.position, transform.rotation);
    }

    [PunRPC]
    void DropRandomLoot_RPC(string ammotype, Vector3 position, Quaternion rotation)
    {
        GameObject loot = new GameObject();
        Quaternion rotFix = new Quaternion(0, 0, 0, 0);
        rotFix.eulerAngles = new Vector3(0, 180, 0);

        if (ammotype == "power")
            loot = Instantiate(powerAmmoPack, position, rotation * rotFix);

        if (ammotype == "heavy")
            loot = Instantiate(heavyAmmoPack, position, rotation * rotFix);

        if (ammotype == "small")
            loot = Instantiate(smallAmmoPack, position, rotation * rotFix);

        if (ammotype == "grenade")
            loot = Instantiate(grenadeAmmoPack, position, rotation * rotFix);

        Destroy(loot, 60);
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(8f);

        int playSound = Random.Range(0, 2);

        if (playSound == 0)
        {
            int randomSound = Random.Range(0, audioClips.Length);

            if (!_isDead && gameObject.activeSelf)
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
            if (lastPlayerWhoShot.gameObject.GetComponent<OnlinePlayerSwarmScript>() != null)
            {
                OnlinePlayerSwarmScript pPoints = lastPlayerWhoShot.gameObject.GetComponent<OnlinePlayerSwarmScript>();

                pPoints.AddPoints(this.points);
            }
        }
    }

    public void TransferDamageToPoints(int points)
    {
        if (lastPlayerWhoShot.gameObject != null)
        {
            if (lastPlayerWhoShot.gameObject.GetComponent<OnlinePlayerSwarmScript>() != null)
            {
                OnlinePlayerSwarmScript pPoints = lastPlayerWhoShot.gameObject.GetComponent<OnlinePlayerSwarmScript>();

                pPoints.AddPoints(points);
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
        if (!PV.IsMine || !onlineSwarmManager)
            return;
        List<PlayerProperties> allPlayers = GetAllPlayers();
        int ran = Random.Range(0, allPlayers.Count);
        int targetPhotonId = allPlayers[ran].PV.ViewID;

        PlayerProperties newTargetProperties = PhotonView.Find(targetPhotonId).GetComponent<PlayerProperties>();
        if (newTargetProperties.isDead || newTargetProperties.isRespawning)
            return;

        Debug.Log("RPC Call: LookForNewPlayer_RPC");
        PV.RPC("LookForNewPlayer_RPC", RpcTarget.All, targetPhotonId);
    }

    [PunRPC]
    void LookForNewPlayer_RPC(int targetPhotonId)
    {
        target = PhotonView.Find(targetPhotonId).transform;
    }

    void randomSkin()
    {
        int skinNum = Random.Range(0, skins.Length);

        if (skins[skinNum])
            skins[skinNum].SetActive(true);
    }

    void ResetZombie()
    {
        gameObject.name = gameObject.name.Replace("(DEAD)", "");
        onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;
        movementAnimationName = "Run";
        nma.enabled = true;
        nma.speed = defaultSpeed;
        StartCoroutine(PlaySound());
        if (placeholderSkin)
            placeholderSkin.SetActive(false);
        randomSkin();

        Health = DefaultHealth + (onlineSwarmManager.waveNumber * 10);
        damage = defaultDamage + (onlineSwarmManager.waveNumber * 2);
        meleeTrigger.ResetTrigger();
        _isDead = false;
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

    public List<PlayerProperties> GetAllPlayers()
    {
        List<PlayerProperties> allPlayers = new List<PlayerProperties>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
            allPlayers.Add(go.GetComponent<PlayerProperties>());

        return allPlayers;
    }

    public void SetEditMode()
    {
        defaultSpeed = 0.1f;
        nma.acceleration = 0.1f;
    }

    public override int GetHealth()
    {
        return (int)Health;
    }
}
