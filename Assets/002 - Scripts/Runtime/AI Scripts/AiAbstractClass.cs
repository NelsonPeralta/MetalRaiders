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
    [SerializeField] Transform _destination;
    [SerializeField] PlayerRange _playerRange;
    PlayerRange _previousPlayerRange;
    public Animator animator;
    [SerializeField] protected int _health;
    float newTargetSwitchingDelay;
    [SerializeField] float _nextActionCooldown;
    [SerializeField] protected bool _seek;
    [SerializeField] protected bool _canSeek, _staticAnimationPlaying;
    bool _canDoAction;
    bool _isDead;
    int _deathDespawnTime = 5;

    // public variables
    public Hitboxes hitboxes;
    public NavMeshAgent nma;
    public Transform projectileSpawnPoint;
    public GameObject motionTrackerDot;

    [Header("Properties")]
    [SerializeField]
    int _defaultHealth;
    public float speed;

    [Header("Combat")]
    public List<AiRangeTrigger> rangeColliders = new List<AiRangeTrigger>();
    public float defaultNextActionCooldown;

    [Header("Player Switching")]
    public int newTargetPhotonId;

    [Header("Line Of Sight")]
    int maxRangeDistance = 15;
    public GameObject objectInLineOfSight;
    [SerializeField] bool _targetInLineOfSight;
    [SerializeField] bool _targetOutOfSight;
    public GameObject LOSSpawn;
    public LayerMask layerMask;
    Vector3 raySpawn;
    RaycastHit hit;

    [Header("Sound")]
    protected AudioSource _voice;
    [SerializeField] protected AudioClip _laughClip;
    [SerializeField] protected AudioClip _attackClip;
    [SerializeField] protected AudioClip _dieClip;

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
    public virtual Transform targetPlayer
    {
        get { return _destination; }
        set
        {
            Debug.Log(System.Environment.StackTrace);
            Debug.Log($"New AI Target: {value}");
            if (value)
            {
                if (value.GetComponent<Player>() && (value.GetComponent<Player>().isDead || value.GetComponent<Player>().isRespawning))
                    targetPlayer = null;
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

                try
                {
                    //GetNewTarget();
                }
                catch (Exception e)
                {
                    Debug.Log($"ERROR while trying to get new target for AI");
                    //Debug.LogWarning(e);
                    //_target = null;
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

            if (_health <= 0 && !isDead)
            {
                targetPlayer = null;
                isDead = true;
            }
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
        get { _canSeek = (!isDead && targetPlayer && seek); return _canSeek; }
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

    public bool staticAnimationPlaying
    {
        set { _staticAnimationPlaying = value; }
        get { return _staticAnimationPlaying; }
    }



    Vector3 _defaultLOSLocalPosition;

    private void Awake()
    {
        _defaultLOSLocalPosition = LOSSpawn.transform.localPosition;
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

        try
        {
            foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
                hitbox.gameObject.SetActive(true);
        }
        catch { }

        foreach (AiRangeTrigger arc in rangeColliders)
            arc.playersInRange.Clear();

        OnPrepareEnd?.Invoke(this);

        transform.position = new Vector3(0, -10, 0);
    }
    public void Spawn(int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        Prepare();

        transform.position = spawnPointPosition;
        transform.rotation = spawnPointRotation;
        targetPlayer = PhotonView.Find(targetPhotonId).transform;
        gameObject.SetActive(true);

        OnPlayerRangeChange?.Invoke(this);
    }
    void OnDeath_Delegate(AiAbstractClass aiAbstractClass) // Bug: Event called twice
    {
        targetPlayer = null;
        foreach (AIHitbox h in GetComponentsInChildren<AIHitbox>())
            h.gameObject.layer = 3;
        Debug.Log($"AI on death delegate. Is dead: {isDead}");
        //SwarmManager.instance.DropRandomLoot(transform.position, transform.rotation);
        try
        {
            DropRandomWeapon();
        }
        catch (Exception e) { Debug.LogError(e); }
        _voice.clip = _dieClip;
        _voice.Play();
        StartCoroutine(Die_Coroutine());
    }

    IEnumerator Die_Coroutine()
    {
        Debug.Log("Die_Coroutine");
        targetPlayer = null;
        GetComponent<NavMeshAgent>().speed = 0;
        GetComponent<Animator>().Play("Die");


        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
            hitbox.gameObject.SetActive(false);

        SwarmManager.instance.InvokeOnAiDeath();

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
        if (canSeek && GetComponent<PhotonView>().IsMine && !staticAnimationPlaying)
            nma.SetDestination(targetPlayer.transform.position);


        //if (canSeek && GetComponent<PhotonView>().IsMine)
        //    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") ||
        //        animator.GetCurrentAnimatorStateInfo(0).IsName("Run") ||
        //        animator.GetCurrentAnimatorStateInfo(0).IsName("Sprint"))
        //        nma.SetDestination(targetPlayer.transform.position);
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


        if (targetPlayer)
            LOSSpawn.transform.LookAt(targetPlayer);
        else
            LOSSpawn.transform.localPosition = _defaultLOSLocalPosition;

        try
        {
            if (projectileSpawnPoint)
                projectileSpawnPoint.transform.LookAt(targetPlayer);
        }
        catch { }



        float raycastRange = maxRangeDistance * 1.2f;

        Ray ray = new Ray(LOSSpawn.transform.position, LOSSpawn.transform.forward);
        //Debug.DrawRay(raySpawn, LOSSpawn.transform.forward * maxRangeDistance, Color.green);

        // Need a Raycast Range Overload to work with LayerMask
        if (Physics.Raycast(ray, out hit, raycastRange, layerMask))
        {
            objectInLineOfSight = hit.transform.gameObject;
            //if (hit.transform.gameObject.GetComponent<PlayerHitbox>())

            if (targetPlayer)
                if (hit.transform.gameObject.GetComponent<Player>())
                {
                    Player playerInLOS = objectInLineOfSight.GetComponent<Player>();

                    if (playerInLOS == targetPlayer.GetComponent<Player>())
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
        try
        {
            if (!targetPlayer.GetComponent<Player>())
                GetNewTarget(emptyTarget: true);
            if (targetPlayer && aiRangeCollider.playersInRange.Contains(targetPlayer.GetComponent<Player>()))
            {
                playerRange = aiRangeCollider.range;

                //foreach(AiRangeTrigger rt in rangeColliders)
                //{
                //    if (rt != aiRangeCollider)
                //        rt.playersInRange.Remove(target.GetComponent<Player>());
                //}
            }
        }
        catch { }
    }

    public virtual void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider, Collider triggerObj = null)
    {
        if (targetPlayer && aiRangeCollider.playersInRange.Contains(targetPlayer.GetComponent<Player>()))
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

        //nextActionCooldown = defaultNextActionCooldown;
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
                if (rt.playersInRange.Contains(targetPlayer.GetComponent<Player>()))
                    rt.playersInRange.Remove(targetPlayer.GetComponent<Player>());
            }
            catch (System.Exception e)
            {

            }
        }
        targetPlayer = null;
    }

    public virtual void GetNewTarget(float walkRadius = 100, bool emptyTarget = false)
    {
        if (!PhotonNetwork.IsMasterClient) return;



        if (gameObject.activeSelf)
            StartCoroutine(GetRandomPlayerTransformSlow_Coroutine());
        //if (!emptyTarget)
        //    targetPlayer = SwarmManager.instance.GetRandomPlayerTransform();
        //else
        //    targetPlayer = GetRandomDestinationTransform(walkRadius);
    }
    IEnumerator GetRandomPlayerTransformSlow_Coroutine()
    {
        yield return new WaitForSeconds(1);


        Debug.Log("GetRandomPlayerTransformSlow_Coroutine");
        try
        {
            targetPlayer = GameManager.instance.pid_player_Dict[FindObjectOfType<NetworkSwarmManager>().GetRandomAlivePlayerPhotonId()].transform;
        }
        catch { targetPlayer = null; }
        //int pid = SwarmManager.instance.GetRandomPlayerTransform().GetComponent<PhotonView>().ViewID;
        //photonView.RPC("UpdateTarget_RPC", RpcTarget.All, pid);
    }

    protected void ChangeAction(string actionString)
    {
        if (!targetPlayer || !targetPlayer.GetComponent<Player>().PV.IsMine)
            return;

        photonView.RPC("ChangeAction_RPC", RpcTarget.All, actionString);
    }

    protected void SpawnKillFeed(string className, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        Player player = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        string nickName = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI).nickName;
        string teamColorCode = GameManager.colorDict["blue"];


        int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
        string weaponColorCode = player.playerInventory.activeWeapon.ammoType.ToString().ToLower();

        string colorCode = "";

        if (className == "Watcher")
            colorCode = GameManager.colorDict["green"];
        if (className == "Knight")
            colorCode = GameManager.colorDict["blue"];
        if (className == "Tyrant")
            colorCode = GameManager.colorDict["purple"];

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

    void DropRandomWeapon()
    {
        int ChanceToDrop = UnityEngine.Random.Range(0, 10);

        if (ChanceToDrop <= 4)
        {
            float ranAmmoFactor = UnityEngine.Random.Range(0.2f, 0.9f);
            float ranCapFactor = UnityEngine.Random.Range(0.2f, 0.6f);
            int randomWeaponInd = UnityEngine.Random.Range(0, GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory.Length);

            WeaponProperties wp = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[randomWeaponInd].GetComponent<WeaponProperties>();

            if (wp.weaponType == WeaponProperties.WeaponType.LMG || wp.weaponType == WeaponProperties.WeaponType.Launcher || wp.weaponType == WeaponProperties.WeaponType.Shotgun || wp.weaponType == WeaponProperties.WeaponType.Sniper)
                return;
            Debug.Log($"DropRandomWeapon: {wp.cleanName}");




            Vector3 spp = transform.position;
            Vector3 fDir = LOSSpawn.transform.forward + new Vector3(0, 2f, 0);
            //NetworkGameManager.SpawnNetworkWeapon(randomWeaponInd, spp, fDir, param);

            GameObject[] weapInv = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory;

            NetworkGameManager.SpawnNetworkWeapon(weapInv[randomWeaponInd].GetComponent<WeaponProperties>(),
                spp, fDir, currAmmo: (int)(wp.ammoCapacity * ranAmmoFactor), spareAmmo: (int)(wp.maxSpareAmmo * ranCapFactor));

        }
    }

    protected abstract void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false);
    public abstract void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false);
    public abstract void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass);
    public abstract void DoAction();
    public abstract void ChildUpdate();


    [PunRPC]
    void UpdateTarget_RPC(int pid)
    {
        Debug.Log($"UpdateTarget_RPC: {pid}");
        targetPlayer = PhotonView.Find(pid).transform;
    }







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
