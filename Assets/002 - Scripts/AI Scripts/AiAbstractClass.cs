using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using System;

abstract public class AiAbstractClass : MonoBehaviourPunCallbacks
{
    // events
    public delegate void AiEvent(AiAbstractClass aiAbstractClass);
    public AiEvent OnHealthChange, OnDeath, OnPlayerRangeChange, OnActionChange, OnNextActionReset, OnNextActionReady, OnTargeInLineOfSightChange, OnTargetDeath;

    // enums
    public enum PlayerRange { Out, Close, Medium, Long }

    // private variables
    PlayerRange _playerRange;
    PlayerRange _previousPlayerRange;
    public Animator animator;
    PhotonView PV;
    Transform _target;
    int _health;
    float newTargetSwitchingDelay;
    float _nextActionCooldown;
    bool _seek;
    bool _canSeek;
    bool _canDoAction;
    bool _isDead;

    // public variables
    public Hitboxes hitboxes;
    public NavMeshAgent nma;
    public Transform projectileSpawnPoint;
    public GameObject motionTrackerDot;

    [Header("Properties")]
    [SerializeField]
    int _defaultHealth;
    public int speed;

    [Header("Combat")]
    public List<AiRangeTrigger> rangeColliders = new List<AiRangeTrigger>();
    public float defaultNextActionCooldown;

    [Header("Player Switching")]
    public int newTargetPhotonId;

    [Header("Line Of Sight")]
    int maxRangeDistance = 15;
    bool _targetInLineOfSight;
    public GameObject LOSSpawn;
    public GameObject objectInLineOfSight;
    public LayerMask layerMask;
    Vector3 raySpawn;
    RaycastHit hit;

    bool _targetOutOfSight;
    float targetOutOfSightDefaultCountdown;
    float targetOutOfSightCountdown;

    public PlayerRange playerRange
    {
        get { return _playerRange; }
        set
        {
            if (_playerRange != value)
            {
                _previousPlayerRange = playerRange;
                _playerRange = value;
                //Debug.Log($"Player range change: {playerRange}");
                OnPlayerRangeChange?.Invoke(this);
            }
        }
    }

    public PlayerRange previousPlayerRange
    {
        get { return _previousPlayerRange; }
        set
        {
            _previousPlayerRange = value;
        }
    }
    public Transform target
    {
        get { return _target; }
        set
        {
            if (value)
            {
                if (value.GetComponent<PlayerProperties>().isDead || value.GetComponent<PlayerProperties>().isRespawning)
                    target = null;
                else
                {
                    _target = value;
                    _target.GetComponent<PlayerProperties>().OnDeath += OnTargetDeath_Delegate;
                }
            }
            else
            {
                _target = null;

                try
                {
                    GetNewTarget();
                }
                catch (Exception e)
                {
                    //Debug.Log(e);
                    _target = null;
                }
            }
        }
    }
    public int health
    {
        get
        {
            return _health;
        }

        set
        {
            if (_health != value)
            {
                _health = value;
                OnHealthChange?.Invoke(this);
            }

            if (_health <= 0)
                isDead = true;

            if (value > 0)
                isDead = false;
        }
    }

    public int defaultHealth { get { return _defaultHealth; } }
    public bool isDead
    {
        get { return _isDead; }
        set { _isDead = value; if (_isDead) OnDeath?.Invoke(this); }
    }

    public bool seek
    {
        get { return _seek; }
        set
        {
            _seek = value;

            if (!seek)
            {
                nma.speed = 0;
                nma.velocity = Vector3.zero;
                animator.SetBool("Run", false);
            }
            else
            {
                nma.speed = speed;
                animator.SetBool("Run", true);
            }
        }
    }
    public bool canSeek
    {
        get { return !isDead && target && seek; }
    }

    public float nextActionCooldown
    {
        get { return _nextActionCooldown; }
        set { _nextActionCooldown = value; if (nextActionCooldown > 0) canDoAction = false; }
    }
    public bool canDoAction { get { return _canDoAction; } set { _canDoAction = value; if (_canDoAction) OnNextActionReady?.Invoke(this); } }

    public bool targetInLineOfSight
    {
        get { return _targetInLineOfSight; }
        private set
        {
            if (value)
            {
                targetOutOfSight = false;
            }
            if (value != _targetInLineOfSight)
            {
                _targetInLineOfSight = value;
                //Debug.Log($"Target in line of sight change: {_targetInLineOfSight}");
                OnTargeInLineOfSightChange?.Invoke(this);
            }
        }
    }

    public bool targetOutOfSight
    {
        get { return _targetOutOfSight; }
        set
        {
            //Debug.Log($"_targetOutOfSight {value}, {targetOutOfSight}, {targetOutOfSightCountdown}");

            if (value && !_targetOutOfSight)
            {
                //Debug.Log("Target is out of sight");
                _targetOutOfSight = true;
                targetOutOfSightCountdown = targetOutOfSightDefaultCountdown;
            }
            else if (!value && _targetOutOfSight)
            {
                //Debug.Log("Target found");
                targetOutOfSightCountdown = 999;
                _targetOutOfSight = false;
            }
        }
    }
    void Awake()
    {
        animator = GetComponent<Animator>();
        targetOutOfSightDefaultCountdown = defaultNextActionCooldown * 2.5f;
        Prepare();
    }

    void Prepare()
    {
        PV = GetComponent<PhotonView>();
        nma = GetComponent<NavMeshAgent>();
        health = defaultHealth;
        nma.speed = speed;
        seek = true;
        objectInLineOfSight = null;
        targetInLineOfSight = false;
        targetOutOfSight = false;
        playerRange = PlayerRange.Out;
        previousPlayerRange = PlayerRange.Out;

        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
            hitbox.gameObject.SetActive(true);

        foreach (AiRangeTrigger arc in rangeColliders)
        {
            arc.OnRangeTriggerEnter += OnRangeTriggerEnter_Delegate;
            arc.OnRangeTriggerExit += OnRangeTriggerExit_Delegate;

            arc.playersInRange.Clear();
        }

        OnActionChange += OnActionChanged_Delegate;
        OnPlayerRangeChange += OnPlayerRangeChange_Delegate;
        OnNextActionReady += OnNextActionReady_Delegate;
        OnDeath += OnDeath_Delegate;
        OnTargeInLineOfSightChange += OnTargetInLineOfSightChanged_Delegate;
    }
    public void Spawn(int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        Prepare();

        transform.position = spawnPointPosition;
        transform.rotation = spawnPointRotation;
        target = PhotonView.Find(targetPhotonId).transform;
        gameObject.SetActive(true);

        OnPlayerRangeChange?.Invoke(this);
    }
    void OnDeath_Delegate(AiAbstractClass aiAbstractClass)
    {
        StartCoroutine(Die_Coroutine());
    }

    IEnumerator Die_Coroutine()
    {
        target = null;
        GetComponent<NavMeshAgent>().speed = 0;
        GetComponent<Animator>().Play("Die");


        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
            hitbox.gameObject.SetActive(false);

        SwarmManager.instance.OnAiDeath();

        DropRandomLoot();
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }


    private void Update()
    {
        ShootLineOfSightRay();
        TargetOutOfSightDelay();
        Movement();
        NextActionCooldown();
        ChildUpdate();
    }
    public void Movement()
    {
        if (canSeek)
            nma.SetDestination(target.transform.position);
    }

    void NextActionCooldown()
    {
        if (nextActionCooldown > 0)
            nextActionCooldown -= Time.deltaTime;

        if (nextActionCooldown <= 0)
            if (!canDoAction)
                canDoAction = true;
    }

    void TargetOutOfSightDelay()
    {
        if (targetOutOfSightCountdown > 0 && targetOutOfSight)
            targetOutOfSightCountdown -= Time.deltaTime;
        //Debug.Log($"Target ouf of sight countdown: {targetOutOfSightCountdown}");
        if (targetOutOfSightCountdown <= 0 && targetInLineOfSight)
            targetInLineOfSight = false;
    }
    void ShootLineOfSightRay()
    {
        if (!target)
            return;

        raySpawn = LOSSpawn.transform.position + new Vector3(0, 0f, 0);
        //Debug.DrawRay(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, Color.green);

        // Need a Raycast Range Overload to work with LayerMask
        if (Physics.Raycast(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, out hit, maxRangeDistance, layerMask))
        {
            objectInLineOfSight = hit.transform.gameObject;
            if (hit.transform.gameObject.GetComponent<PlayerHitbox>())
            {
                PlayerProperties playerInLOS = objectInLineOfSight.GetComponent<PlayerHitbox>().player;

                if (playerInLOS == target.GetComponent<PlayerProperties>())
                    targetInLineOfSight = true;
                else
                    targetOutOfSight = true;
            }
            else
            {
                targetOutOfSight = true;
            }
        }
        else
        {
            targetOutOfSight = true;
            objectInLineOfSight = null;
        }
    }
    void OnRangeTriggerEnter_Delegate(AiRangeTrigger aiRangeCollider)
    {
        if (target && aiRangeCollider.playersInRange.Contains(target.GetComponent<PlayerProperties>()))
            playerRange = aiRangeCollider.range;
    }

    void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider)
    {
        if (target && aiRangeCollider.playersInRange.Contains(target.GetComponent<PlayerProperties>()))
        {
            PlayerRange exitingRange = aiRangeCollider.range;
            PlayerRange newPlayerRange = playerRange;

            if (exitingRange == PlayerRange.Close)
                newPlayerRange = PlayerRange.Medium;
            else if (exitingRange == PlayerRange.Medium)
                newPlayerRange = PlayerRange.Long;
            else if (exitingRange == PlayerRange.Long)
                newPlayerRange = PlayerRange.Out;

            playerRange = newPlayerRange;
        }
    }

    protected void InvokeOnActionChanged()
    {
        OnActionChange?.Invoke(this);
    }

    protected void InvokeOnRangeChanged()
    {
        if (canDoAction)
            OnPlayerRangeChange?.Invoke(this);
    }
    void OnActionChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        DoAction();

        nextActionCooldown = defaultNextActionCooldown;
        OnNextActionReset?.Invoke(this);
    }

    void OnNextActionReady_Delegate(AiAbstractClass aiAbstractClass)
    {
        DoAction();
    }
    void OnTargetDeath_Delegate(PlayerProperties playerProperties)
    {
        //Debug.Log("On target death delegate");
        target = null;
    }

    void GetNewTarget()
    {
        if (gameObject.activeSelf)
            StartCoroutine(GetRandomPlayerTransformSlow_Coroutine());
    }
    IEnumerator GetRandomPlayerTransformSlow_Coroutine()
    {
        yield return new WaitForSeconds(1);
        target = SwarmManager.instance.GetRandomPlayerTransform();
    }

    protected void DropRandomLoot()
    {
        int chanceToDrop = UnityEngine.Random.Range(0, 25);
        string ammoType = "";

        if (chanceToDrop == 0)
            ammoType = "power";

        if (chanceToDrop == 1)
            ammoType = "grenade";

        if (chanceToDrop == 2 && chanceToDrop == 3)
            ammoType = "heavy";

        if (chanceToDrop >= 4 && chanceToDrop <= 6)
            ammoType = "small";


        PV.RPC("DropRandomLoot_RPC", RpcTarget.All, ammoType, transform.position, transform.rotation);
    }

    [PunRPC]
    protected void DropRandomLoot_RPC(string ammotype, Vector3 position, Quaternion rotation)
    {
        GameObject loot = new GameObject();
        Quaternion rotFix = new Quaternion(0, 0, 0, 0);
        rotFix.eulerAngles = new Vector3(0, 180, 0);

        if (ammotype == "power")
            loot = Instantiate(GameManager.instance.powerAmmoPack.gameObject, position, rotation * rotFix);

        if (ammotype == "heavy")
            loot = Instantiate(GameManager.instance.heavyAmmoPack.gameObject, position, rotation * rotFix);

        if (ammotype == "small")
            loot = Instantiate(GameManager.instance.lightAmmoPack.gameObject, position, rotation * rotFix);

        if (ammotype == "grenade")
            loot = Instantiate(GameManager.instance.grenadeAmmoPack.gameObject, position, rotation * rotFix);

        Destroy(loot, 60);
    }

    public abstract void Damage(int damage, int playerWhoShotPDI);
    public abstract void Damage_RPC(int damage, int playerWhoShotPDI);
    public abstract void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void DoAction();
    public abstract void ChildUpdate();
}