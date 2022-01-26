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
    public AiEvent OnHealthChange, OnDeath, OnDeathEnd, OnPlayerRangeChange, OnActionChange, OnNextActionReset, OnNextActionReady, OnTargeInLineOfSightChange, OnTargetDeath;

    // enums
    public enum PlayerRange { Out, Close, Medium, Long }

    // private variables
    [SerializeField] Transform _target;
    [SerializeField] PlayerRange _playerRange;
    PlayerRange _previousPlayerRange;
    public Animator animator;
    [SerializeField] int _health;
    float newTargetSwitchingDelay;
    float _nextActionCooldown;
    [SerializeField] bool _seek;
    bool _canSeek;
    bool _canDoAction;
    [SerializeField] bool _isDead;
    [SerializeField] int _deathDespawnTime = 5;

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

    [Header("Sound")]
    protected AudioSource _voice;
    [SerializeField] protected AudioClip _laughClip;
    [SerializeField] protected AudioClip _attackClip;
    [SerializeField] protected AudioClip _dieClip;

    bool _targetOutOfSight;
    float targetOutOfSightDefaultCountdown;
    float targetOutOfSightCountdown;

    [SerializeField] float _newTargetCountdown;
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
            Debug.Log($"New AI Target: {value}");
            if (value)
            {
                if (value.GetComponent<Player>().isDead || value.GetComponent<Player>().isRespawning)
                    target = null;
                else
                {
                    _target = value;
                    _target.GetComponent<Player>().OnPlayerDeath -= OnTargetDeath_Delegate;
                    _target.GetComponent<Player>().OnPlayerDeath += OnTargetDeath_Delegate;
                    DoAction();
                }
            }
            else
            {
                Debug.Log($"New AI Target is NULL");
                _target = null;
                seek = false;
                playerRange = PlayerRange.Out;
                _newTargetCountdown = 1;

                //try
                //{
                //    GetNewTarget();
                //}
                //catch (Exception e)
                //{
                //    Debug.Log($"ERROR while trying to get new target for AI");
                //    Debug.LogWarning(e);
                //    _target = null;
                //}
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

            if (_health <= 0 && !isDead)
                isDead = true;
            else if (value > 0 && isDead)
                isDead = false;
        }
    }

    public int defaultHealth { get { return _defaultHealth; } }
    public bool isDead
    {
        get { return _isDead; }
        set
        {
            if (!_isDead && value)
            {
                Debug.Log($"{name} OnDeathInvoke");
                _isDead = true;
                OnDeath?.Invoke(this);
            }
            else if (!value)
            {
                _isDead = false;
            }
        }
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
    void Start()
    {
        _voice = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        targetOutOfSightDefaultCountdown = defaultNextActionCooldown * 2.5f;
        Prepare();

        OnActionChange += OnActionChanged_Delegate;
        OnPlayerRangeChange += OnPlayerRangeChange_Delegate;
        OnNextActionReady += OnNextActionReady_Delegate;
        OnDeath += OnDeath_Delegate;
        OnTargeInLineOfSightChange += OnTargetInLineOfSightChanged_Delegate;
        OnDeathEnd += OnDeathEnd_Delegate;
        foreach (AiRangeTrigger arc in rangeColliders)
        {
            arc.OnRangeTriggerEnter += OnRangeTriggerEnter_Delegate;
            arc.OnRangeTriggerExit += OnRangeTriggerExit_Delegate;
        }
    }

    void Prepare()
    {
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
            arc.playersInRange.Clear();
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
    void OnDeath_Delegate(AiAbstractClass aiAbstractClass) // Bug: Event called twice
    {
        Debug.Log($"AI on death delegate. Is dead: {isDead}");
        SwarmManager.instance.DropRandomLoot(transform.position, transform.rotation);
        _voice.clip = _dieClip;
        _voice.Play();
        StartCoroutine(Die_Coroutine());
    }

    IEnumerator Die_Coroutine()
    {
        target = null;
        GetComponent<NavMeshAgent>().speed = 0;
        GetComponent<Animator>().Play("Die");


        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
            hitbox.gameObject.SetActive(false);

        SwarmManager.instance.Invoke_OnAiDeath();

        yield return new WaitForSeconds(_deathDespawnTime);
        OnDeathEnd?.Invoke(this);

        try
        {
            gameObject.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error while trying to destroy game object: {e}");
        }
    }


    private void Update()
    {
        ShootLineOfSightRay();
        TargetOutOfSightDelay();
        Movement();
        NextActionCooldown();
        ChildUpdate();
        NewTargetCountdown();
    }

    void NewTargetCountdown()
    {
        if (!gameObject.activeSelf || isDead)
            return;
        if (_newTargetCountdown > 0)
        {
            _newTargetCountdown -= Time.deltaTime;

            if (_newTargetCountdown <= 0)
            {
                GetNewTarget();
            }
        }
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

        LOSSpawn.transform.LookAt(target);
        if (projectileSpawnPoint)
            projectileSpawnPoint.transform.LookAt(target);

        raySpawn = LOSSpawn.transform.position + new Vector3(0, 0f, 0);
        //Debug.DrawRay(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, Color.green);

        // Need a Raycast Range Overload to work with LayerMask
        if (Physics.Raycast(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, out hit, maxRangeDistance, layerMask))
        {
            objectInLineOfSight = hit.transform.gameObject;
            if (hit.transform.gameObject.GetComponent<PlayerHitbox>())
            {
                Player playerInLOS = objectInLineOfSight.GetComponent<PlayerHitbox>().player;

                if (playerInLOS == target.GetComponent<Player>())
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
        if (target && aiRangeCollider.playersInRange.Contains(target.GetComponent<Player>()))
        {
            playerRange = aiRangeCollider.range;

            //foreach(AiRangeTrigger rt in rangeColliders)
            //{
            //    if (rt != aiRangeCollider)
            //        rt.playersInRange.Remove(target.GetComponent<Player>());
            //}
        }
    }

    void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider)
    {
        if (target && aiRangeCollider.playersInRange.Contains(target.GetComponent<Player>()))
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
    void OnTargetDeath_Delegate(Player playerProperties)
    {
        foreach (AiRangeTrigger rt in rangeColliders)
        {
            try
            {
                if (rt.playersInRange.Contains(target.GetComponent<Player>()))
                    rt.playersInRange.Remove(target.GetComponent<Player>());
            }
            catch (System.Exception e)
            {

            }
        }
        target = null;
    }

    void GetNewTarget()
    {
        //if (gameObject.activeSelf)
        //    StartCoroutine(GetRandomPlayerTransformSlow_Coroutine());
        target = SwarmManager.instance.GetRandomPlayerTransform();
    }
    IEnumerator GetRandomPlayerTransformSlow_Coroutine()
    {
        yield return new WaitForSeconds(1);
        target = SwarmManager.instance.GetRandomPlayerTransform();
    }

    protected void ChangeAction(string actionString)
    {
        if (!target || !target.GetComponent<Player>().PV.IsMine)
            return;

        photonView.RPC("ChangeAction_RPC", RpcTarget.All, actionString);
    }
    public abstract void ChangeAction_RPC(string actionString);
    public abstract void Damage(int damage, int playerWhoShotPDI);
    public abstract void Damage_RPC(int damage, int playerWhoShotPDI);
    public abstract void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void DoAction();
    public abstract void ChildUpdate();
}
