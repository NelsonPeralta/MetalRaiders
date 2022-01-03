using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

abstract public class AiAbstractClass : MonoBehaviourPunCallbacks
{
    // events
    public delegate void AiEvent(AiAbstractClass aiAbstractClass);
    public AiEvent OnHealthChange, OnDeath, OnPlayerRangeChange, OnActionChanged, OnNextActionReset, OnNextActionReady;

    // enums
    public enum PlayerRange { Out, Close, Medium, Long }

    // private variables
    PhotonView PV;
    int _health;
    float newTargetSwitchingDelay;
    float _nextActionCooldown;
    bool _seek;
    bool _canSeek;
    bool _canDoAction;
    bool _isDead;

    // public variables
    public PlayerRange playerRange;
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
                nma.velocity = Vector3.zero;
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
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        nma = GetComponent<NavMeshAgent>();
        health = defaultHealth;
        nma.speed = speed;


        foreach (AiRangeTrigger arc in rangeColliders)
        {
            arc.OnRangeTriggerEnter += OnRangeTriggerEnter_Delegate;
            arc.OnRangeTriggerExit += OnRangeTriggerExit_Delegate;
        }

        OnActionChanged += OnActionChanged_Delegate;
        OnPlayerRangeChange += OnPlayerRangeChange_Delegate;
        OnNextActionReady += OnNextActionReady_Delegate;
        OnDeath += OnDeath_Delegate;
    }

    private void OnEnable()
    {
        OnPlayerRangeChange?.Invoke(this);
    }

    //public void Damage(int damage, int playerWhoShotPDI)
    //{
    //    if (isDead)
    //        return;
    //    PV.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI);
    //}

    //[PunRPC]
    //void Damage_RPC(int damage, int playerWhoShotPDI)
    //{
    //    if (isDead)
    //        return;

    //    PlayerProperties pp = GameManager.instance.GetPlayerWithPhotonViewId(playerWhoShotPDI);
    //    pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(damage);

    //    health -= damage;
    //    if (isDead)
    //    {
    //        pp.GetComponent<OnlinePlayerSwarmScript>().kills++;
    //        pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(defaultHealth);
    //    }
    //}

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
    void OnRangeTriggerEnter_Delegate(AiRangeTrigger aiRangeCollider)
    {
        if (playerRange == aiRangeCollider.range)
            return;

        playerRange = aiRangeCollider.range;
        OnPlayerRangeChange?.Invoke(this);
    }

    void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider)
    {
        if (aiRangeCollider.range == PlayerRange.Close)
            playerRange = PlayerRange.Medium;
        else if (aiRangeCollider.range == PlayerRange.Medium)
            playerRange = PlayerRange.Long;
        else if (aiRangeCollider.range == PlayerRange.Long)
            playerRange = PlayerRange.Out;

        OnPlayerRangeChange?.Invoke(this);
    }

    public void InvokeOnActionChanged()
    {
        Debug.Log("On Action Changed");
        OnActionChanged?.Invoke(this);
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
    public abstract void DoAction();

    public abstract void ChildUpdate();
}
