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
    public AiEvent OnHealthChange, OnDeath, OnDeathEnd, OnPlayerRangeChange, OnActionChange, 
        OnNextActionReset, OnNextActionReady, OnTargeInLineOfSightChange, OnTargetDeath, OnPrepareEnd,
        OnDestinationChanged, OnDestinationNull;

    // enums
    public enum PlayerRange { Out, Close, Medium, Long }

    // private variables
    [SerializeField] protected Transform _destination;
    [SerializeField] PlayerRange _playerRange;
    PlayerRange _previousPlayerRange;
    public Animator animator;
    [SerializeField] int _health;
    float newTargetSwitchingDelay;
    float _nextActionCooldown;
    [SerializeField] protected bool _seek;
    [SerializeField] bool _canSeek;
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

    [SerializeField] protected float _newTargetCountdown;
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
    public virtual Transform destination
    {
        get { return _destination; }
        set
        {
            Debug.Log(System.Environment.StackTrace);
            Debug.Log($"New AI Target: {value}");
            if (value)
            {
                if (value.GetComponent<Player>() && (value.GetComponent<Player>().isDead || value.GetComponent<Player>().isRespawning))
                    destination = null;
                else
                {
                    _destination = value;
                    try
                    {
                        _destination.GetComponent<Player>().OnPlayerDeath -= OnTargetDeath_Delegate;
                        _destination.GetComponent<Player>().OnPlayerDeath += OnTargetDeath_Delegate;
                    }
                    catch { }
                    DoAction();
                    OnDestinationChanged?.Invoke(this);
                }
            }
            else
            {
                Debug.Log($"New AI Target is NULL");
                _destination = null;
                seek = false;
                playerRange = PlayerRange.Out;
                _newTargetCountdown = 1;
                OnDestinationNull?.Invoke(this);

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

    public virtual bool seek
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
        get { _canSeek = (!isDead && destination && seek); return _canSeek; }
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
        if (GetComponent<Animator>())
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
            arc.AiAbstractClass = this;
            arc.OnRangeTriggerEnter += OnRangeTriggerEnter_Delegate;
            arc.OnRangeTriggerExit += OnRangeTriggerExit_Delegate;
            Debug.Log(arc.AiAbstractClass.name);
        }

        OnPrepareEnd?.Invoke(this);
    }

    protected void Prepare()
    {
        nma = GetComponent<NavMeshAgent>();
        health = defaultHealth + SwarmManager.instance.currentWave * 2;
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

        OnPrepareEnd?.Invoke(this);
    }
    public void Spawn(int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        Prepare();

        transform.position = spawnPointPosition;
        transform.rotation = spawnPointRotation;
        destination = PhotonView.Find(targetPhotonId).transform;
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
        destination = null;
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

    public virtual void NewTargetCountdown()
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
            nma.SetDestination(destination.transform.position);
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
        if (!destination)
            return;

        LOSSpawn.transform.LookAt(destination);
        if (projectileSpawnPoint)
            projectileSpawnPoint.transform.LookAt(destination);

        raySpawn = LOSSpawn.transform.position + new Vector3(0, 0f, 0);
        //Debug.DrawRay(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, Color.green);

        // Need a Raycast Range Overload to work with LayerMask
        if (Physics.Raycast(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, out hit, maxRangeDistance, layerMask))
        {
            objectInLineOfSight = hit.transform.gameObject;
            //if (hit.transform.gameObject.GetComponent<PlayerHitbox>())
            if (hit.transform.gameObject.GetComponent<Player>())
            {
                Player playerInLOS = objectInLineOfSight.GetComponent<Player>();

                if (playerInLOS == destination.GetComponent<Player>())
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
    public virtual void OnRangeTriggerEnter_Delegate(AiRangeTrigger aiRangeCollider, Collider triggerObj = null)
    {
        if (!destination.GetComponent<Player>())
            GetNewTarget(emptyTarget: true);
        if (destination && aiRangeCollider.playersInRange.Contains(destination.GetComponent<Player>()))
        {
            playerRange = aiRangeCollider.range;

            //foreach(AiRangeTrigger rt in rangeColliders)
            //{
            //    if (rt != aiRangeCollider)
            //        rt.playersInRange.Remove(target.GetComponent<Player>());
            //}
        }
    }

    public virtual void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider, Collider triggerObj = null)
    {
        if (destination && aiRangeCollider.playersInRange.Contains(destination.GetComponent<Player>()))
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
                if (rt.playersInRange.Contains(destination.GetComponent<Player>()))
                    rt.playersInRange.Remove(destination.GetComponent<Player>());
            }
            catch (System.Exception e)
            {

            }
        }
        destination = null;
    }

    public virtual void GetNewTarget(float walkRadius = 100, bool emptyTarget = false)
    {
        //if (gameObject.activeSelf)
        //    StartCoroutine(GetRandomPlayerTransformSlow_Coroutine());
        if (!emptyTarget)
            destination = SwarmManager.instance.GetRandomPlayerTransform();
        else
            destination = GetRandomDestinationTransform(walkRadius);
    }
    IEnumerator GetRandomPlayerTransformSlow_Coroutine()
    {
        yield return new WaitForSeconds(1);
        destination = SwarmManager.instance.GetRandomPlayerTransform();
    }

    protected void ChangeAction(string actionString)
    {
        if (!destination || !destination.GetComponent<Player>().PV.IsMine)
            return;

        photonView.RPC("ChangeAction_RPC", RpcTarget.All, actionString);
    }

    protected void SpawnKillFeed(string className, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        Player player = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        string nickName = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI).nickName;
        string teamColorCode = KillFeedManager.killFeedColorCodeDict["blue"];


        int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
        string weaponColorCode = player.playerInventory.activeWeapon.ammoType.ToString().ToLower();

        string colorCode = "";

        if (className == "Watcher")
            colorCode = KillFeedManager.killFeedColorCodeDict["green"];
        if (className == "Knight")
            colorCode = KillFeedManager.killFeedColorCodeDict["blue"];
        if (className == "Tyrant")
            colorCode = KillFeedManager.killFeedColorCodeDict["purple"];

        foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
        {
            string feed = $"{nickName} killed";
            if (kfm.GetComponent<Player>() == this)
            {
                try
                {
                    int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                    feed = $"<color={teamColorCode}>You <color=\"white\"><sprite={damageSourceSpriteCode}>";

                    if (isHeadshot)
                        feed += $"<sprite={hsCode}>";

                    feed += $" <color={colorCode}>{className}";
                    kfm.EnterNewFeed(feed);
                }
                catch
                {
                    kfm.EnterNewFeed($"<color={teamColorCode}>You <color=\"white\">killed a <color={colorCode}>{className}");
                }
            }
            else
            {
                try
                {
                    int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                    feed = $"<color={teamColorCode}>{nickName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

                    if (isHeadshot)
                        feed += $"<sprite={hsCode}>";

                    feed += $" <color={colorCode}>{className}";
                    kfm.EnterNewFeed(feed);
                }
                catch
                {
                    kfm.EnterNewFeed($"<color={teamColorCode}>{nickName} <color=\"white\">killed a <color={colorCode}>{className}");
                }
            }
        }
    }
    public abstract void ChangeAction_RPC(string actionString);

    public void Damage(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        { // Hit Marker Handling
            Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);

            if (isHeadshot)
            {
                if (health <= damage)
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.HeadshotKill);
                else
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Headshot);
            }
            else
            {
                if (health <= damage)
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
                else
                    p.GetComponent<PlayerUI>().SpawnHitMarker();
            }
        }

        Damage_Abstract(damage, playerWhoShotPDI, damageSource, isHeadshot);
    }
    protected abstract void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false);
    public abstract void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false);
    public abstract void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void DoAction();
    public abstract void ChildUpdate();










    private Vector3 GetRandomDestinationPosition(float walkRadius = 100)
    {
        Debug.Log($"Walk radius: {walkRadius}");
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        Vector3 finalPosition = hit.position;

        return finalPosition;
    }

    private Transform GetRandomDestinationTransform(float walkRadius = 100)
    {
        GameObject d = new GameObject();
        d.transform.position = GetRandomDestinationPosition(walkRadius);
        d.layer = 6;
        d.AddComponent<BoxCollider>();
        d.GetComponent<BoxCollider>().isTrigger = true;
        d.AddComponent<Rigidbody>();
        d.GetComponent<Rigidbody>().useGravity = false;
        Debug.Log("GetRandomDestinationTransform: " + d.name);
        return d.transform;
    }

    protected void GoToRandomPoint()
    {
        int walkRadius = 100;

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        Vector3 finalPosition = hit.position;

        GetComponent<NavMeshAgent>().destination = finalPosition;
        Debug.Log($"AI goint to random point: {finalPosition}");
    }

    protected bool DestinationReached()
    {
        // Check if we've reached the destination
        if (!GetComponent<NavMeshAgent>().pathPending)
        {
            if (GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance)
            {
                if (!GetComponent<NavMeshAgent>().hasPath || GetComponent<NavMeshAgent>().velocity.sqrMagnitude == 0f)
                {
                    // Done
                    return true;
                }
            }
        }
        return false;
    }
}