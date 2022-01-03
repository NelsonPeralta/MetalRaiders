using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

abstract public class AiAbstractClass : MonoBehaviourPunCallbacks
{
    // events
    public delegate void AiEvent(AiAbstractClass aiAbstractClass);
    public AiEvent OnHealthChange, OnDeath, OnPlayerRangeChange, OnActionChange, OnNextActionReset, OnNextActionReady, OnTargeInLineOfSightChange;

    // enums
    public enum PlayerRange { Out, Close, Medium, Long }

    // private variables
    PlayerRange _playerRange;
    PhotonView PV;
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

    [Header("Properties")]
    [SerializeField]
    int _defaultHealth;
    public int speed;

    [Header("Combat")]
    public Transform target;
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
    public RaycastHit hit;
    bool resettingTargetInLOS;

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
                _playerRange = value;
                Debug.Log($"Player range change: {playerRange}");
                OnPlayerRangeChange?.Invoke(this);
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
            _health = value;
            OnHealthChange?.Invoke(this);

            if (_health <= 0)
                isDead = true;
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
            }
            else
            {
                nma.speed = speed;
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
                Debug.Log($"Target in line of sight change: {_targetInLineOfSight}");
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
                Debug.Log("Target is out of sight");
                _targetOutOfSight = true;
                targetOutOfSightCountdown = targetOutOfSightDefaultCountdown;
            }else if (!value)
            {
                Debug.Log("Target found");
                targetOutOfSightCountdown = 999;
                _targetOutOfSight = false;
            }
        }
    }
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        nma = GetComponent<NavMeshAgent>();
        health = defaultHealth;
        nma.speed = speed;
        targetOutOfSightDefaultCountdown = nextActionCooldown * 2.5f;

        foreach (AiRangeTrigger arc in rangeColliders)
        {
            arc.OnRangeTriggerEnter += OnRangeTriggerEnter_Delegate;
            arc.OnRangeTriggerExit += OnRangeTriggerExit_Delegate;
        }

        OnActionChange += OnActionChanged_Delegate;
        OnPlayerRangeChange += OnPlayerRangeChange_Delegate;
        OnNextActionReady += OnNextActionReady_Delegate;
        OnDeath += OnDeath_Delegate;
        OnTargeInLineOfSightChange += OnTargetInLineOfSightChanged_Delegate;
    }

    private void OnEnable()
    {
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
        playerRange = aiRangeCollider.range;
    }

    void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider)
    {
        PlayerRange newPlayerRange = playerRange;

        if (aiRangeCollider.range == PlayerRange.Close)
            newPlayerRange = PlayerRange.Medium;
        else if (aiRangeCollider.range == PlayerRange.Medium)
            newPlayerRange = PlayerRange.Long;
        else if (aiRangeCollider.range == PlayerRange.Long)
            newPlayerRange = PlayerRange.Out;

        playerRange = newPlayerRange;
    }

    public void InvokeOnActionChanged()
    {
        Debug.Log("On Action Changed");
        OnActionChange?.Invoke(this);
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

    public abstract void Damage(int damage, int playerWhoShotPDI);
    public abstract void Damage_RPC(int damage, int playerWhoShotPDI);
    public abstract void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void DoAction();
    public abstract void ChildUpdate();
}
